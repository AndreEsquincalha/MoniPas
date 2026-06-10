using System.Security.Cryptography;
using System.Text;

namespace MONIPAS.monipas.controller
{
    public static class SenhaProtegida
    {
        private const string Prefixo = "enc:";
        private static readonly byte[] EntropiaAdicional = Encoding.UTF8.GetBytes("MONIPAS-v1");

        public static bool EstaCriptografada(string? valor) =>
            !string.IsNullOrEmpty(valor) && valor.StartsWith(Prefixo);

        public static string Criptografar(string textoPuro)
        {
            byte[] dados = Encoding.UTF8.GetBytes(textoPuro);
            byte[] cifrado = ProtectedData.Protect(dados, EntropiaAdicional, DataProtectionScope.CurrentUser);
            return Prefixo + Convert.ToBase64String(cifrado);
        }

        public static string Descriptografar(string valorCifrado)
        {
            string base64 = valorCifrado.Substring(Prefixo.Length);
            byte[] cifrado = Convert.FromBase64String(base64);
            byte[] dados = ProtectedData.Unprotect(cifrado, EntropiaAdicional, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(dados);
        }
    }
}
