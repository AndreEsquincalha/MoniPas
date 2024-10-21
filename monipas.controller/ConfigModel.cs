using MONIPAS.monipas.model;
using Newtonsoft.Json;


namespace MONIPAS.monipas.controller
{
    public class ConfigModel
    {
        public string PastaLcl { get; set; } // Caminho local da pasta
        public string PastaRmt { get; set; }  // Caminho remoto da pasta no FTP
        public FTPDetails FTPDetails { get; set; }

        // O método CarregarConfig deve estar dentro da classe
        public static ConfigModel CarregarConfiguracao(string caminhoArquivo)
        {
            using (StreamReader r = new StreamReader(caminhoArquivo))
            {
                string json = r.ReadToEnd();
                ConfigModel? config = JsonConvert.DeserializeObject<ConfigModel>(json);

                if (config == null || config.FTPDetails == null)
                {
                    throw new Exception("Falha ao carregar a configuração. Verifique o arquivo JSON.");
                }
                
                return config;
            }
        }
    }


}
