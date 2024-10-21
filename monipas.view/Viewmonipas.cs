using MONIPAS.monipas.controller;
using System.Windows.Forms;

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

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void configenvio_Click(object sender, EventArgs e)
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
