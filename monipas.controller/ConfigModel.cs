using MONIPAS.monipas.model;
using Newtonsoft.Json;
using System.Security.Cryptography;


namespace MONIPAS.monipas.controller
{
    public class ConfigModel
    {
        public string PastaLcl { get; set; }
        public string PastaRmt { get; set; }

        // Segundos que o arquivo precisa ficar sem alteração (tamanho/data) antes de ser enviado.
        // Evita upload "pela metade" de arquivos escritos de forma incremental. Padrão: 20s.
        public int? QuietudeSegundos { get; set; }

        public FTPDetails FTPDetails { get; set; }

        public static ConfigModel CarregarConfiguracao()
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MONIPAS");
            string configFilePath = Path.Combine(appDataFolder, "configFTP.json");

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"Arquivo de configuração não encontrado em: {configFilePath}");
            }

            string json = File.ReadAllText(configFilePath);
            ConfigModel? config = JsonConvert.DeserializeObject<ConfigModel>(json);

            if (config == null || config.FTPDetails == null)
            {
                throw new Exception("Falha ao carregar a configuração. Verifique o arquivo JSON.");
            }

            string? senha = config.FTPDetails.Senha;

            if (!string.IsNullOrEmpty(senha))
            {
                if (SenhaProtegida.EstaCriptografada(senha))
                {
                    try
                    {
                        config.FTPDetails.Senha = SenhaProtegida.Descriptografar(senha);
                    }
                    catch (CryptographicException)
                    {
                        throw new Exception(
                            "Falha ao descriptografar a senha. O configFTP.json provavelmente foi copiado " +
                            "de outro usuário/máquina (a chave do DPAPI é por usuário). " +
                            "Edite o arquivo e cole a senha em texto puro — o app irá cifrá-la novamente no próximo início.");
                    }
                }
                else
                {
                    config.FTPDetails.Senha = SenhaProtegida.Criptografar(senha);
                    string jsonAtualizado = JsonConvert.SerializeObject(config, new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    File.WriteAllText(configFilePath, jsonAtualizado);

                    config.FTPDetails.Senha = senha;
                }
            }

            return config;
        }
    }
}
