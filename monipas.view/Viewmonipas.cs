using MONIPAS.monipas.controller;

namespace MONIPAS.monipas.view
{
    public partial class Viewmonipas : Form
    {

        public Viewmonipas()
        {
            InitializeComponent();

        }


        private void reenviardados_Click(object sender, EventArgs e)
        {
            String caminhoArquivoJson = "configFTP.json";
            ConfigModel config = ConfigModel.CarregarConfiguracao(caminhoArquivoJson);

            if (string.IsNullOrEmpty(config.PastaLcl) || !Directory.Exists(config.PastaLcl))
            {
                MessageBox.Show("O caminho da pasta local no JSON é invalido ou não foi encontrado, Verifique a configuração do JSON");
                return;
            }

            //Criar o dialogo de seleção de aruqivo
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = config.PastaLcl,
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Selecione um arquivo para reenviar",
                Multiselect = true,
            };

            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] filePaths = openFileDialog.FileNames;

                //Reenviar o arquivo por FTP
                MonitorController monitorController = new MonitorController(config.PastaLcl, config.FTPDetails, listBox);

                foreach (string filePath in filePaths)
                {
                    monitorController.EnviarArquivoFTP(filePath);

                    listBox.Invoke((MethodInvoker)delegate
                    {
                        listBox.Items.Insert(0, filePath);
                    });
                }
            }

        }
        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void Viewmonipas_Load(object sender, EventArgs e)
        {
            // Carregar a configuração do arquivo JSON
            string caminhoArquivoJson = @"configFTP.json";
            ConfigModel config = ConfigModel.CarregarConfiguracao(caminhoArquivoJson);

            // Inicializar o MonitorController com as informações do JSON
            MonitorController monitorController = new MonitorController(config.PastaLcl, config.FTPDetails, listBox);


            // Iniciar o monitoramento da pasta
            monitorController.StartMonitoring();

        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void OpenLogs_Click(object sender, EventArgs e)
        {

        }

        private void OpenJson_Click_1(object sender, EventArgs e)
        {
            string filePath = "configFTP.json"; // Caminho do arquivo que você deseja abrir

            if (File.Exists(filePath))
            {
                // Abre o arquivo JSON no editor de texto padrão (Notepad)
                System.Diagnostics.Process.Start("notepad.exe", filePath);
            }
            else
            {
                MessageBox.Show("O arquivo configFTP.json não foi encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }

        private void OpenLogs_Click_1(object sender, EventArgs e)
        {

        }
    }
}
