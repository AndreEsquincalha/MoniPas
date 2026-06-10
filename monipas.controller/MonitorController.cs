using FluentFTP.Exceptions;
using FluentFTP;
using MONIPAS.monipas.model;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace MONIPAS.monipas.controller
{
    public class MonitorController
    {
        private string caminhoPasta;
        private FTPDetails ftpDetails;
        private ListBox listBox;
        private FileSystemWatcher? watcher;
        private System.Threading.Timer? timer;
        private Thread? currentMonitoringThread;
        private string? currentMonitoredPath;
        private readonly object _monitorLock = new object();
        private readonly object _envioLock = new object();
        private readonly object _logLock = new object();

        ConfigModel config = new ConfigModel();

        public MonitorController(string caminhoPasta, FTPDetails ftpDetails, ListBox listBox)
        {
            this.caminhoPasta = caminhoPasta;
            this.ftpDetails = ftpDetails;
            this.listBox = listBox;
        }

        public async Task StartMonitoringAsync()
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
            string LogListBox = Path.Combine(appDataFolder, "LOG_SendData.txt");

            lock (_monitorLock)
            {
                if (currentMonitoringThread != null && currentMonitoringThread.IsAlive && currentMonitoredPath == caminhoPasta)
                {
                    return;
                }

                currentMonitoringThread = new Thread(() => MonitorarPasta(caminhoPasta))
                {
                    IsBackground = true
                };
                currentMonitoredPath = caminhoPasta;
                currentMonitoringThread.Start();
            }

            await Task.Delay(5000);

            List<string> DadosPresentesNaPasta = Directory.GetFiles(caminhoPasta).ToList();
            List<string> ListBoxData = File.Exists(LogListBox) ? File.ReadAllLines(LogListBox).ToList() : new List<string>();

            if (DadosPresentesNaPasta.Count > 0 && ListBoxData.Count > 0)
            {
                List<string> ArquivosNaoEnviados = DadosPresentesNaPasta.Except(ListBoxData).ToList();
                if (ArquivosNaoEnviados.Count > 0)
                {
                    string listarArquivos = string.Join("\n", ArquivosNaoEnviados);
                    string mensagem = $"Foram encontrados arquivos na pasta que ainda não foram enviados ao ARM após o reinício do MONIPAS.\n\n" +
                                      $"Os arquivos abaixo serão enviados a partir de agora:\n\n{listarArquivos}\n\n";

                    MessageBox.Show(mensagem, "Confirmação de Envio", MessageBoxButtons.OK, MessageBoxIcon.Question);

                    foreach (var data in ListBoxData)
                    {
                        listBox.Invoke((MethodInvoker)delegate
                        {
                            listBox.Items.Insert(0, data);
                        });
                    }

                    await Task.Run(() => EnviarArquivoFTP(ArquivosNaoEnviados));
                }
                else
                {
                    foreach (var data in ListBoxData)
                    {
                        listBox.Invoke((MethodInvoker)delegate
                        {
                            listBox.Items.Insert(0, data);
                        });
                    }
                }
            }
            else
            {
                foreach (var data in DadosPresentesNaPasta)
                {
                    listBox.Invoke((MethodInvoker)delegate
                    {
                        listBox.Items.Insert(0, data);
                        EscreverNoLog(LogListBox, data);
                    });
                }
            }

            await Task.Delay(5000);
            IniciarVerificacaoPeriodica();
        }

        private void MonitorarPasta(string path)
        {
            if (watcher != null)
            {
                watcher.Dispose();
            }

            watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = "*.txt",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            List<string> arquivos = new List<string> { e.FullPath };
            EnviarArquivoFTP(arquivos);
        }

        public void EnviarArquivoFTP(List<string> filePaths)
        {
            // Serializa os envios: watcher (OnChanged), Timer periódico e o envio inicial
            // podem chamar este método em paralelo. Sem este lock, dois uploads simultâneos
            // do mesmo arquivo se truncam/sobrescrevem no servidor.
            lock (_envioLock)
            {
                string protocolo = (ftpDetails.Protocolo ?? "FTP").Trim().ToUpperInvariant();

                if (protocolo == "SFTP")
                {
                    EnviarArquivosViaSFTP(filePaths);
                }
                else
                {
                    EnviarArquivosViaFTP(filePaths);
                }
            }
        }

        private void EnviarArquivosViaFTP(List<string> filePaths)
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
            string logFilePath = Path.Combine(appDataFolder, "LOG_SendData.txt");
            string currentFilePath = "";

            int porta = ftpDetails.Porta ?? 21;

            try
            {
                using (var client = new FtpClient(ftpDetails.Host, ftpDetails.Usuario, ftpDetails.Senha, porta))
                {
                    client.Connect();

                    foreach (var filePath in filePaths)
                    {
                        currentFilePath = filePath;

                        try
                        {
                            // Ponto 1: só envia depois que o arquivo terminou de ser escrito.
                            if (!AguardarArquivoLiberado(filePath))
                            {
                                EscreverNoLogErro($"Arquivo '{filePath}' continua em uso/em escrita após o tempo limite. Envio adiado para o próximo ciclo.");
                                EscreverNoLogUpload($"ADIADO     | FTP  | '{filePath}' ainda em escrita/bloqueado.");
                                continue;
                            }

                            string remoteFilePath = $"{ftpDetails.PastaRmt}/{Path.GetFileName(filePath)}";
                            long tamanhoLocal = new FileInfo(filePath).Length;

                            EscreverNoLogUpload($"INICIANDO  | FTP  | '{filePath}' ({tamanhoLocal} bytes) -> '{remoteFilePath}'");

                            // FtpVerify.Retry: o FluentFTP confere tamanho/checksum e refaz o upload se divergir.
                            FtpStatus status = client.UploadFile(filePath, remoteFilePath, FtpRemoteExists.Overwrite, false, FtpVerify.Retry);

                            if (status != FtpStatus.Success)
                            {
                                EscreverNoLogErro($"Upload FTP não confirmado para '{filePath}' (status={status}). Não será marcado como enviado.");
                                EscreverNoLogUpload($"FALHA      | FTP  | '{filePath}' status={status}");
                                continue;
                            }

                            // Ponto 3: confirma que o tamanho no servidor bate com o da origem.
                            long tamanhoRemoto = client.GetFileSize(remoteFilePath);
                            if (tamanhoRemoto != tamanhoLocal)
                            {
                                EscreverNoLogErro($"Upload FTP INCOMPLETO '{filePath}': local={tamanhoLocal} bytes, remoto={tamanhoRemoto} bytes. Não será marcado como enviado.");
                                EscreverNoLogUpload($"INCOMPLETO | FTP  | '{filePath}' local={tamanhoLocal} != remoto={tamanhoRemoto} bytes.");
                                continue;
                            }

                            EscreverNoLogUpload($"OK         | FTP  | '{filePath}' local={tamanhoLocal} == remoto={tamanhoRemoto} bytes. Upload completo confirmado.");

                            listBox.Invoke((MethodInvoker)delegate
                            {
                                listBox.Items.Insert(0, filePath);
                                EscreverNoLog(logFilePath, currentFilePath);
                            });
                        }
                        catch (Exception ex)
                        {
                            EscreverNoLogErro($"Falha ao enviar via FTP '{filePath}': {ex.Message}");
                            EscreverNoLogUpload($"ERRO       | FTP  | '{filePath}': {ex.Message}");
                        }
                    }

                    client.Disconnect();
                }
            }
            catch (FtpException ex)
            {
                EscreverNoLogErro($"Erro FTP (host={ftpDetails.Host}:{porta}, arquivo='{currentFilePath}'): {ex.Message}");
            }
            catch (IOException ex)
            {
                EscreverNoLogErro($"Erro IO no envio FTP (arquivo='{currentFilePath}'): {ex.Message}");
            }
            catch (Exception ex)
            {
                EscreverNoLogErro($"Erro inesperado no envio FTP (arquivo='{currentFilePath}'): {ex.Message}");
            }
        }

        private void EnviarArquivosViaSFTP(List<string> filePaths)
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
            string logFilePath = Path.Combine(appDataFolder, "LOG_SendData.txt");
            string currentFilePath = "";

            int porta = ftpDetails.Porta ?? 22;

            if (string.IsNullOrWhiteSpace(ftpDetails.Host) ||
                string.IsNullOrWhiteSpace(ftpDetails.Usuario) ||
                ftpDetails.Senha == null)
            {
                EscreverNoLogErro("Configuração SFTP inválida: Host, Usuario e Senha são obrigatórios.");
                return;
            }

            try
            {
                var keybInteractive = new KeyboardInteractiveAuthenticationMethod(ftpDetails.Usuario);
                keybInteractive.AuthenticationPrompt += (sender, e) =>
                {
                    foreach (var prompt in e.Prompts)
                    {
                        prompt.Response = ftpDetails.Senha;
                    }
                };
                var passwordAuth = new PasswordAuthenticationMethod(ftpDetails.Usuario, ftpDetails.Senha);
                var connectionInfo = new ConnectionInfo(ftpDetails.Host, porta, ftpDetails.Usuario, keybInteractive, passwordAuth)
                {
                    Timeout = TimeSpan.FromMinutes(2)
                };

                using (var client = new SftpClient(connectionInfo))
                {
                    client.OperationTimeout = TimeSpan.FromMinutes(5);
                    client.Connect();

                    string pastaRemota = (ftpDetails.PastaRmt ?? "/").TrimEnd('/');
                    if (string.IsNullOrEmpty(pastaRemota)) pastaRemota = "/";

                    if (!client.Exists(pastaRemota))
                    {
                        EscreverNoLogErro(
                            $"Pasta remota '{pastaRemota}' não existe ou não é acessível para o usuário '{ftpDetails.Usuario}'. " +
                            $"Home do usuário no servidor: '{client.WorkingDirectory}'. " +
                            $"Ajuste o campo 'PastaRmt' no configFTP.json (talvez o caminho seja relativo ao home).");
                        client.Disconnect();
                        return;
                    }

                    foreach (var filePath in filePaths)
                    {
                        currentFilePath = filePath;

                        try
                        {
                            // Ponto 1: só envia depois que o arquivo terminou de ser escrito.
                            if (!AguardarArquivoLiberado(filePath))
                            {
                                EscreverNoLogErro($"Arquivo '{filePath}' continua em uso/em escrita após o tempo limite. Envio adiado para o próximo ciclo.");
                                EscreverNoLogUpload($"ADIADO     | SFTP | '{filePath}' ainda em escrita/bloqueado.");
                                continue;
                            }

                            string remoteFilePath = $"{pastaRemota}/{Path.GetFileName(filePath)}";
                            long tamanhoLocal = new FileInfo(filePath).Length;

                            EscreverNoLogUpload($"INICIANDO  | SFTP | '{filePath}' ({tamanhoLocal} bytes) -> '{remoteFilePath}'");

                            using (var fs = File.OpenRead(filePath))
                            {
                                client.UploadFile(fs, remoteFilePath);
                            }

                            // Ponto 3: confirma que o tamanho no servidor bate com o da origem.
                            long tamanhoRemoto = client.GetAttributes(remoteFilePath).Size;
                            if (tamanhoRemoto != tamanhoLocal)
                            {
                                EscreverNoLogErro($"Upload SFTP INCOMPLETO '{filePath}': local={tamanhoLocal} bytes, remoto={tamanhoRemoto} bytes. Não será marcado como enviado.");
                                EscreverNoLogUpload($"INCOMPLETO | SFTP | '{filePath}' local={tamanhoLocal} != remoto={tamanhoRemoto} bytes.");
                                continue;
                            }

                            EscreverNoLogUpload($"OK         | SFTP | '{filePath}' local={tamanhoLocal} == remoto={tamanhoRemoto} bytes. Upload completo confirmado.");

                            listBox.Invoke((MethodInvoker)delegate
                            {
                                listBox.Items.Insert(0, filePath);
                                EscreverNoLog(logFilePath, currentFilePath);
                            });
                        }
                        catch (Exception ex)
                        {
                            EscreverNoLogErro($"Falha ao enviar via SFTP '{filePath}': {ex.Message}");
                            EscreverNoLogUpload($"ERRO       | SFTP | '{filePath}': {ex.Message}");
                        }
                    }

                    client.Disconnect();
                }
            }
            catch (SshException ex)
            {
                EscreverNoLogErro($"Erro SSH/SFTP (host={ftpDetails.Host}:{porta}, arquivo='{currentFilePath}'): {ex.Message}");
            }
            catch (IOException ex)
            {
                EscreverNoLogErro($"Erro IO no envio SFTP (arquivo='{currentFilePath}'): {ex.Message}");
            }
            catch (Exception ex)
            {
                EscreverNoLogErro($"Erro inesperado no envio SFTP (arquivo='{currentFilePath}'): {ex.Message}");
            }
        }

        /// <summary>
        /// Aguarda o arquivo ser liberado pelo processo que o está escrevendo.
        /// Tenta abrir em modo exclusivo (FileShare.None): enquanto o escritor mantiver
        /// o arquivo aberto, lança IOException e tentamos de novo. Quando consegue abrir,
        /// significa que a escrita terminou e o arquivo está íntegro para envio.
        /// Retorna false se o arquivo continuar bloqueado após o tempo limite (será reenviado depois).
        /// </summary>
        private bool AguardarArquivoLiberado(string caminho)
        {
            const int maxTentativas = 20;
            const int delayMs = 500; // até ~10s de espera total

            for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
            {
                try
                {
                    using (File.Open(caminho, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        return true;
                    }
                }
                catch (IOException)
                {
                    // Arquivo ainda em uso/em escrita; aguarda e tenta novamente.
                    Thread.Sleep(delayMs);
                }
                catch (Exception ex)
                {
                    EscreverNoLogErro($"Erro ao verificar disponibilidade de '{caminho}': {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        private void EscreverNoLog(string logFilePath, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;

            lock (_logLock)
            {
                if (File.Exists(logFilePath))
                {
                    string[] linhas = File.ReadAllLines(logFilePath);
                    if (linhas.Contains(filePath)) return;
                }

                File.AppendAllText(logFilePath, $"{filePath}\n");
            }
        }

        private void EscreverNoLogErro(string mensagem)
        {
            try
            {
                string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
                string errorLogPath = Path.Combine(appDataFolder, "LOG_Errors.txt");
                lock (_logLock)
                {
                    File.AppendAllText(errorLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {mensagem}\n");
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Log dedicado ao detalhe de cada upload (LOG_Upload.txt): início, tamanho local,
        /// tamanho confirmado no servidor e resultado (OK / INCOMPLETO / FALHA / ERRO / ADIADO).
        /// Permite auditar se o arquivo subiu completo (mesmo tamanho na origem e no servidor).
        /// </summary>
        private void EscreverNoLogUpload(string mensagem)
        {
            try
            {
                string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
                string uploadLogPath = Path.Combine(appDataFolder, "LOG_Upload.txt");
                lock (_logLock)
                {
                    File.AppendAllText(uploadLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {mensagem}\n");
                }
            }
            catch
            {
            }
        }

        private void VerificarEEnviarArquivos(string pasta)
        {
            if (!Directory.Exists(pasta))
            {
                MessageBox.Show($"A pasta {pasta} não foi encontrada!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<string> arquivosNaPasta = Directory.GetFiles(pasta).ToList();
            List<string> arquivosNaListBox = new List<string>();

            listBox.Invoke((MethodInvoker)delegate
            {
                foreach (var item in listBox.Items)
                {
                    if (item != null)
                    {
                        arquivosNaListBox.Add(item.ToString());
                    }
                }
            });

            List<string> arquivosParaEnviar = arquivosNaPasta.Except(arquivosNaListBox).ToList();

            if (arquivosParaEnviar.Count > 0)
            {
                EnviarArquivoFTP(arquivosParaEnviar);
            }
        }

        public void IniciarVerificacaoPeriodica()
        {
            if (timer == null)
            {
                timer = new System.Threading.Timer(VerificarArquivosCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
            }
        }

        private void VerificarArquivosCallback(object? state)
        {
            VerificarEEnviarArquivos(caminhoPasta);
        }
    }
}
