using MONIPAS.monipas.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using ThreadingTimer = System.Threading.Timer;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MONIPAS.monipas.controller
{
    public class LogSystemSend
    {
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

/*       public void IniciarVerificacaoPeriodica()
        {
            System.Threading.Timer timer = new System.Threading.Timer((e) =>
            {
                try
                {
                    List<string> dadosFalhados = ReadLog();

                    if (dadosFalhados.Count > 0)
                    {
                        EnviarArquivoFTP(dadosFalhados);


                        //////////////// funçaõ do FTP aqui
                    }
                }
                catch
                {

                }
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
        }*/
    }
}
