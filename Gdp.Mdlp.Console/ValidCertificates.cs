using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Console
{
    public static class ValidCertificates
    {
        public static void Execute()
        {
            using (var cm = new Mdlp.CryptographyManager())
            {

                foreach (X509Certificate2 certificate in cm.GetValidCertificates())
                {
                    System.Console.WriteLine($"{certificate.GetNameInfo(X509NameType.SimpleName, false)}: {certificate.Thumbprint}");
                }
            }
        }
    }
}
