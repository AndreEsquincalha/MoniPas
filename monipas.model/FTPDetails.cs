
namespace MONIPAS.monipas.model
{
    public class FTPDetails
    {
        public string? Protocolo { get; set; } = "FTP";
        public string? Host { get; set; }
        public int? Porta { get; set; }
        public string? Usuario { get; set; }
        public string? Senha { get; set; }
        public string? PastaRmt { get; set; }
        public string? PastaLcl { get; set; }
    }
}
