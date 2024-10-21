using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MONIPAS.monipas.model
{
    public class FTPDetails
    {
        public string? Host { get; set; }
        public string? Usuario { get; set; }
        public string? Senha { get; set; }
        public string? PastaRmt { get; set; }
        public string? PastaLcl { get; set; }
    }
}
