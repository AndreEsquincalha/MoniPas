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

        ConfigModel config = new ConfigModel();

        // Construtor original
        public MonitorController(string caminhoPasta, FTPDetails ftpDetails, ListBox listBox)
        {
            this.caminhoPasta = caminhoPasta;
            this.ftpDetails = ftpDetails;
            this.listBox = listBox;
        }


        public void StartMonitoring()
        {
            Thread monitoringThread = new Thread(() => MonitorarPasta(caminhoPasta));
            monitoringThread.IsBackground = true;
            monitoringThread.Start();

            // Adiciona um delay de 5 segundos antes de iniciar a verificação periódica
            Thread.Sleep(5000);

            IniciarVerificacaoPeriodica();
        }

        private void MonitorarPasta(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = "*.txt"
            };
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {

            // Cria uma lista com o arquivo detectado
            List<string> arquivos = new List<string> { e.FullPath };

            // Enviar o arquivo via FTP após ser detectado
            EnviarArquivoFTP(arquivos);

        }

        public void EnviarArquivoFTP(List<string> filePaths)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LOG_SendFail.txt");
            string currentFilePath = "";  // Variável para armazenar o arquivo atual

            try
            {
                using (var client = new FtpClient(ftpDetails.Host, ftpDetails.Usuario, ftpDetails.Senha))
                {
                    // Conecta ao servidor FTP
                    client.Connect();

                    foreach (var filePath in filePaths)
                    {
                        currentFilePath = filePath;  // Armazena o arquivo atual
                        try
                        {
                            // Usa o caminho remoto vindo do JSON (ftpDetails.PastaRmt)
                            string remoteFilePath = $"{ftpDetails.PastaRmt}/{Path.GetFileName(filePath)}";

                            // Envia o arquivo para o diretório FTP
                            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                client.UploadFile(filePath, remoteFilePath);
                            }

                            listBox.Invoke((MethodInvoker)delegate {
                                listBox.Items.Insert(0, filePath);
                            });
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFilePath, $"{filePath}\n");
                            // Trata qualquer outro erro geral

                            MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    // Desconecta do servidor FTP
                    client.Disconnect();
                }
            }
            catch (FtpException ftpEx)
            {

                foreach (var filePath in filePaths)
                {
                    File.AppendAllText(logFilePath, $"{filePath}\n");  // Usando currentFilePath // Trata erros relacionados ao FTP
                }
                MessageBox.Show($"Erro de FTP: {ftpEx.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ioEx)
            {

                foreach (var filePath in filePaths)
                {
                    File.AppendAllText(logFilePath, $"{filePath}\n");  // Usando currentFilePath // Trata erros relacionados ao I/O (leitura/gravação de arquivos)
                }

                MessageBox.Show($"Erro de E/S: {ioEx.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                foreach (var filePath in filePaths)
                {
                    File.AppendAllText(logFilePath, $"{filePath}\n");  // Usando currentFilePath // Trata qualquer outro erro geral
                }

                MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //LOGICA PARA REENVIO AUTOMATICO DE DADOS FALHADOS
        string DadosNaoEnviados = @"LOG_SendFail.txt";
        public List<string> ReadLog()
        {
            List<string> dados = new List<string>();

            try
            {
                if (File.Exists(DadosNaoEnviados))
                {
                    dados = File.ReadAllLines(DadosNaoEnviados).ToList();
                }
                else
                {
                    File.Create(@"LOG_SendFail.txt").Dispose();
                    MessageBox.Show($"Arquivo {DadosNaoEnviados} não foi encontrado para realizar o reenvio automático", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao Ler o arquivo: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dados;
        }

//=======================================================================================================================================
        public void ReenviarArquivoFalhos(List<string> filePaths)
        {
            //string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LOG_SendFail.txt");
            string currentFilePath = "";  // Variável para armazenar o arquivo atual

            try
            {
                using (var client = new FtpClient(ftpDetails.Host, ftpDetails.Usuario, ftpDetails.Senha))
                {
                    // Conecta ao servidor FTP
                    client.Connect();

                    foreach (var filePath in filePaths)
                    {
                        currentFilePath = filePath;  // Armazena o arquivo atual
                        try
                        {
                            // Usa o caminho remoto vindo do JSON (ftpDetails.PastaRmt)
                            string remoteFilePath = $"{ftpDetails.PastaRmt}/{Path.GetFileName(filePath)}";

                            // Envia o arquivo para o diretório FTP
                            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                client.UploadFile(filePath, remoteFilePath);
                            }

                            listBox.Invoke((MethodInvoker)delegate {
                                listBox.Items.Insert(0, filePath);
                            });

                            // Apaga o conteúdo do arquivo após o envio com sucesso
                            File.WriteAllText(DadosNaoEnviados, string.Empty);

                        }
                        catch (Exception ex)
                        {
                            // Trata qualquer outro erro geral
                            MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    // Desconecta do servidor FTP
                    client.Disconnect();
                }
            }
            catch (FtpException ftpEx)
            {
                MessageBox.Show($"Erro de FTP: {ftpEx.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"Erro de E/S: {ioEx.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

//=======================================================================================================================================
        public void IniciarVerificacaoPeriodica()
        {
            System.Threading.Timer timer = new System.Threading.Timer((e) =>
            {

                List<string> dadosFalhados = ReadLog();

                if (dadosFalhados.Count > 0)
                {
                    ReenviarArquivoFalhos(dadosFalhados);   
                }
                
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }


    }

}
