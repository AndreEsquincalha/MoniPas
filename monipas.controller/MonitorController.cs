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

        private Thread? currentMonitoringThread; // Variável para armazenar a thread atual
        private string? currentMonitoredPath;    // Variável para armazenar o caminho da pasta atual

        public void StartMonitoring()
        {
            lock (this) // Garante que apenas uma thread acesse essa lógica por vez
            {

                string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
                string LogListBox = Path.Combine(appDataFolder, "LOG_SendData.txt");
                

                // Verifica se já existe uma thread monitorando a mesma pasta
                if (currentMonitoringThread != null && currentMonitoringThread.IsAlive && currentMonitoredPath == caminhoPasta)
                {
                    // Para a thread existente
                    currentMonitoringThread.Interrupt();
                    currentMonitoringThread = null;
                }

                // Cria uma nova thread para o monitoramento
                currentMonitoringThread = new Thread(() => MonitorarPasta(caminhoPasta));
                currentMonitoredPath = caminhoPasta; // Atualiza o caminho sendo monitorado
                currentMonitoringThread.IsBackground = true;
                currentMonitoringThread.Start();

                // Adiciona um delay de 5 segundos antes de iniciar a verificação periódica
                Thread.Sleep(5000);


                // Obtém todos os arquivos da pasta
                List<string> DadosPresentesNaPasta = Directory.GetFiles(caminhoPasta).ToList();
                List<string> ListBoxData = new List<string>();
                ListBoxData = File.ReadAllLines(LogListBox).ToList();

                if (DadosPresentesNaPasta.Count > 0 & ListBoxData.Count > 0)
                {
                    List<string> ArquivosNaoEnviados = DadosPresentesNaPasta.Except(ListBoxData).ToList();
                    if (ArquivosNaoEnviados.Count > 0)
                    {
                        string listarArquivos = string.Join("\n", ArquivosNaoEnviados);
                        // Criar a mensagem formatada
                        string mensagem = $"Foram encontrados arquivos na pasta que ainda não foram enviados ao ARM após o reinício do MONIPAS.\n\n" +
                                        $"Os arquivos abaixo serão enviados a partir de agora:\n\n{listarArquivos}\n\n";

                        DialogResult resultado = MessageBox.Show(
                            mensagem,
                            "Confirmação de Envio",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Question
                        );
                            // foreach para colocar na listbox arquivos que já foram enviados
                            foreach (var data in ListBoxData)
                            {
                                listBox.Invoke((MethodInvoker)delegate
                                {
                                    listBox.Items.Insert(0, data);
                                });
                            }
                            //Chama função pra enviar os arquivos que ainda não foram enviados enquanto o MONIPAS estava off
                            EnviarArquivoFTP(ArquivosNaoEnviados);

                    }else
                    {
                        //se não houver arquivos novos na pasta enquanto o MONIPAS estava off, ele só vai adicionar 
                        //ao listbox os arquivos q já foram enviados e gravados no LogSendData
                        foreach (var data in ListBoxData)
                        {
                            listBox.Invoke((MethodInvoker)delegate
                            {
                                listBox.Items.Insert(0, data);
                                //EscreverNoLog(LogListBox,data);
                            });
                        }
                    }

                }else
                {
                    // Se houver dados na pasta e não tem dados no LogSendData, ou seja, não houve nenhum envio pelo MONIPAS ainda,
                    // ele vai adicionar os arquivos da pasta ao log, pra não ocorrer envio de dados passados e atrapalhar invalidações
                    //ou seja vai começar a enviar os dados a partir da chegada de novos dados
                    foreach (var data in DadosPresentesNaPasta)
                    {
                        listBox.Invoke((MethodInvoker)delegate
                        {
                            listBox.Items.Insert(0, data);
                            EscreverNoLog(LogListBox,data);
                        });
                    }
                   
                }
            
                // Adiciona um delay de 5 segundos antes de iniciar a verificação periódica
                Thread.Sleep(10000);
                IniciarVerificacaoPeriodica();
                
            }
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
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
            string logFilePath = Path.Combine(appDataFolder, "LOG_SendData.txt");

            //antigo//string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LOG_SendFail.txt");
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

                            listBox.Invoke((MethodInvoker)delegate
                            {
                                listBox.Items.Insert(0, filePath);
                                EscreverNoLog(logFilePath, currentFilePath);
                            });
                        }
                        catch (Exception /*ex*/)
                        {
                            //EscreverNoLog(logFilePath, currentFilePath);
                            // Trata qualquer outro erro geral

                            //MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    // Desconecta do servidor FTP
                    client.Disconnect();
                }
            }
            catch (FtpException /*ftpEx*/)
            {
                //EscreverNoLog(logFilePath, currentFilePath);  // Usando currentFilePath // Trata erros relacionados ao FTP

                //MessageBox.Show($"Erro de FTP: {ftpEx.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException /*ioEx*/)
            {
                //EscreverNoLog(logFilePath, currentFilePath);  // Usando currentFilePath // Trata erros relacionados ao I/O (leitura/gravação de arquivos)

                //MessageBox.Show($"Erro de E/S: {ioEx.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception /*ex*/)
            {
                //EscreverNoLog(logFilePath, currentFilePath);  // Usando currentFilePath // Trata qualquer outro erro geral

                //MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //LOGICA PARA REENVIO AUTOMATICO DE DADOS FALHADOS

        
        private void EscreverNoLog(string logFilePath, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }//else
            //     File.AppendAllText(logFilePath, $"{filePath}\n");

            // Verifica se o arquivo de log já existe e se filePath já está registrado
            if (File.Exists(logFilePath))
            {
                string[] linhas = File.ReadAllLines(logFilePath);
                if (linhas.Contains(filePath))
                {
                    return; // Se o arquivo já está no log, não adiciona novamente
                }
            }
            // Escreve no log apenas se ainda não estiver registrado
            File.AppendAllText(logFilePath, $"{filePath}\n");
        }


        private void VerificarEEnviarArquivos(string pasta)
        {
            // Verifica se a pasta existe
            if (!Directory.Exists(pasta))
            {
                MessageBox.Show($"A pasta {pasta} não foi encontrada!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Obtém todos os arquivos da pasta
            List<string> arquivosNaPasta = Directory.GetFiles(pasta).ToList();

            // Obtém a lista de arquivos já adicionados na listBox
            List<string> arquivosNaListBox = new List<string>();

            listBox.Invoke((MethodInvoker)delegate
            {
                foreach (var item in listBox.Items)
                {
                    if (item != null) // Verifica se item não é nulo
                    {
                        arquivosNaListBox.Add(item.ToString());
                    }
                }
            });

            // Filtra os arquivos que ainda não estão na listBox
            List<string> arquivosParaEnviar = arquivosNaPasta.Except(arquivosNaListBox).ToList();

            // Se houver arquivos novos, chamar a função de envio
            if (arquivosParaEnviar.Count > 0)
            {
                EnviarArquivoFTP(arquivosParaEnviar);
            }
            else
            {
                //MessageBox.Show("Nenhum arquivo novo para enviar.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //=======================================================================================================================================
        public void IniciarVerificacaoPeriodica()
        {
            System.Threading.Timer timer = new System.Threading.Timer((e) =>
            {

                VerificarEEnviarArquivos(caminhoPasta);

            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }


    }

}
