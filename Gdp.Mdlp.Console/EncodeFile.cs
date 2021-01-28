using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gdp.Mdlp.Console
{
    static class EncodeFile
    {
        public static void Execute()
        {
            using (var cm = new Mdlp.CryptographyManager())
            {
                var cert = cm.FindCertificateByTumbprint("0C2D9F3FDA17E0374984B3FE853FB1FA605266B8");
                var text = File.ReadAllText(@"d:\temp\ctest\input.txt");
                File.WriteAllText(
                    @"d:\temp\ctest\output_net1.txt",
                    cm.Encrypt(text, cert));
                /*
                File.WriteAllText(
                    @"d:\temp\ctest\output_net2.txt",
                    cm.EncryptBase64CSP(text, cert.Thumbprint));*/
            }
        }
    }
}
