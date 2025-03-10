﻿using MONIPAS.monipas.controller;
using System;
using System.Drawing;

namespace MONIPAS.monipas.view
{
    public partial class Viewmonipas : Form
    {

        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;

        public Viewmonipas()
        {
            InitializeComponent();

            //adição do icone de bandeja do sistema:
            // Criar NotifyIcon
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon("./LogoMONIPAS.ico"), // Ícone padrão do sistema, substitua pelo seu
                Text = "MONIPAS - Monitoramento",
                Visible = false
            };

            // Criar menu de contexto para o NotifyIcon
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Abrir", null, MostrarJanela);
            contextMenu.Items.Add("Sair", null, FecharAplicacao);

            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.DoubleClick += MostrarJanela;

            // Associar evento de minimização
            this.Resize += Viewmonipas_Resize;
            this.FormClosing += Viewmonipas_FormClosing;
        }


        private void reenviardados_Click(object sender, EventArgs e)
        {

            //String caminhoArquivoJson = "/configFTP.json";
            ConfigModel config = ConfigModel.CarregarConfiguracao();

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

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] filePaths = openFileDialog.FileNames;

                //Reenviar o arquivo por FTP
                MonitorController monitorController = new MonitorController(config.PastaLcl, config.FTPDetails, listBox);

                foreach (string filePath in filePaths)
                {
                    List<string> arquivoUnico = new List<String> { filePath };
                    monitorController.EnviarArquivoFTP(arquivoUnico);

                }
            }


        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Viewmonipas_Load(object sender, EventArgs e)
        {
            // Carregar a configuração do arquivo JSON
            //string caminhoArquivoJson = @"configFTP.json";

            ConfigModel config = ConfigModel.CarregarConfiguracao();


            // Inicializar o MonitorController com as informações do JSON
            MonitorController monitorController = new MonitorController(config.PastaLcl, config.FTPDetails, listBox);
            // Iniciar o monitoramento da pasta
            monitorController.StartMonitoring();

        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
        }


        private void OpenJson_Click_1(object sender, EventArgs e)
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
            string configFilePath = Path.Combine(appDataFolder, "configFTP.json"); // Caminho do arquivo que você deseja abrir

            if (File.Exists(configFilePath))
            {
                // Abre o arquivo JSON no editor de texto padrão (Notepad)
                System.Diagnostics.Process.Start("notepad.exe", configFilePath);
            }
            else
            {
                MessageBox.Show("O arquivo configFTP.json não foi encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void OpenLogs_Click_1(object sender, EventArgs e)
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
            string filePath = Path.Combine(appDataFolder, "LOG_SendData.txt");
            //string filePath = "LOG_SendFail.txt";

            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start("notepad.exe", filePath);
            }
            else
            {
                MessageBox.Show("O Arquivo de LOG não foi encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Viewmonipas_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
            }
        }

        private void MostrarJanela(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void FecharAplicacao(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private void Viewmonipas_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon.Visible = false;
        }
        


    }
}
