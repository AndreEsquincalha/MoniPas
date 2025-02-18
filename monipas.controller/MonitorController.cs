using FluentFTP.Exceptions;
using FluentFTP;
using MONIPAS.monipas.model;

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

        ConfigModel config = new ConfigModel();

        public MonitorController(string caminhoPasta, FTPDetails ftpDetails, ListBox listBox)
        {
            this.caminhoPasta = caminhoPasta;
            this.ftpDetails = ftpDetails;
            this.listBox = listBox;
        }

        public void StartMonitoring()
        {
            lock (this)
            {
                string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
                string LogListBox = Path.Combine(appDataFolder, "LOG_SendData.txt");

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

                Task.Delay(5000).Wait(); // Substitui Thread.Sleep para evitar travamento

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

                        EnviarArquivoFTP(ArquivosNaoEnviados);
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

                Task.Delay(5000).Wait(); // Substitui Thread.Sleep para evitar travamento
                IniciarVerificacaoPeriodica();
            }
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
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
            string logFilePath = Path.Combine(appDataFolder, "LOG_SendData.txt");
            string currentFilePath = "";

            try
            {
                using (var client = new FtpClient(ftpDetails.Host, ftpDetails.Usuario, ftpDetails.Senha))
                {
                    client.Connect();

                    foreach (var filePath in filePaths)
                    {
                        currentFilePath = filePath;

                        try
                        {
                            string remoteFilePath = $"{ftpDetails.PastaRmt}/{Path.GetFileName(filePath)}";
                            client.UploadFile(filePath, remoteFilePath);

                            listBox.Invoke((MethodInvoker)delegate
                            {
                                listBox.Items.Insert(0, filePath);
                                EscreverNoLog(logFilePath, currentFilePath);
                            });
                        }
                        catch (Exception)
                        {
                            // Tratamento de erro pode ser adicionado aqui
                        }
                    }

                    client.Disconnect();
                }
            }
            catch (FtpException) { }
            catch (IOException) { }
            catch (Exception) { }
        }

        private void EscreverNoLog(string logFilePath, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;

            if (File.Exists(logFilePath))
            {
                string[] linhas = File.ReadAllLines(logFilePath);
                if (linhas.Contains(filePath)) return;
            }

            File.AppendAllText(logFilePath, $"{filePath}\n");
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
