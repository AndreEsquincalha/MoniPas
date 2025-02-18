using MONIPAS.monipas.view;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace MONIPAS
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("pt-BR");
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo("pt-BR");


            // Caminho para AppData\Roaming\MONIPAS
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
            string configFilePath = Path.Combine(appDataFolder, "configFTP.json");
            string LogSendData = Path.Combine(appDataFolder, "LOG_SendData.txt");

            // Verificar e criar a pasta MONIPAS, se necessário
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            if (!File.Exists(LogSendData))
            {
                using (FileStream fs = File.Create(LogSendData))
                {
                    fs.Close();
                }
            }

        
            // Verificar e criar o arquivo configFTP.json, se necessário
            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new
                {
                    PastaLcl = @"C:\",
                    FTPDetails = new
                    {
                        Host = "Host/IP",
                        Usuario = "user",
                        Senha = "password",
                        PastaRmt = "/pasta/remota"
                    }
                };

                string json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFilePath, json);

                string mensagem = $"Inicialização completa, A seguir você deverá configurar a pasta local em que o software irá monitorar\n" +
                                  $"além das configurações do FTP de envio e a pasta remoto para onde os arquivos deverão ser enviados.\n\n";


                DialogResult resultado = MessageBox.Show(
                            mensagem,
                            "Confirme abaixo após leitura.",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Question
                        );


                if (File.Exists(configFilePath))
                {
                    // Inicia o processo do Notepad e espera até que ele seja fechado
                    using (var process = System.Diagnostics.Process.Start("notepad.exe", configFilePath))
                    {
                        process.WaitForExit(); // Aguarda o Notepad ser fechado
                    }
                }
            }


            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Viewmonipas());
        }
    }
}