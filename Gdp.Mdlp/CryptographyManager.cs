using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace Gdp.Mdlp
{
    public class CryptographyManager : IDisposable
    {
        X509Store _Store;
        /// <summary>
        /// Хранилище сертификатов
        /// </summary>
        public X509Store Store
        {
            get
            {
                if (_Store == null)
                    lock (this)
                        if (_Store == null)
                        {
                            _Store = new X509Store(/*StoreName.My, StoreLocation.CurrentUser*/);
                            _Store.Open(OpenFlags.ReadOnly);
                        }
                return _Store;
            }
        }
        /// <summary>
        /// Возвращает список валидных на указанную дату сертификатов
        /// </summary>
        /// <returns></returns>
        public X509Certificate2Collection GetValidCertificates()
        {
            return Store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
        }
        public X509Certificate2 FindCertificateByTumbprint(string tumbprint)
        {
            var certificate = GetValidCertificates()
                .Cast<X509Certificate2>()
                .Where(c => c.Thumbprint == tumbprint)
                .FirstOrDefault();
            if (certificate == null)
                throw new Exception($"Not found cert {tumbprint}");
            return certificate;
        }
        public void Dispose()
        {
            if (_Store != null)
            {
                _Store.Dispose();
                _Store = null;
            }
        }
        public string Encrypt(string msg, X509Certificate2 certificate)
        {
            return Encrypt(Encoding.Default.GetBytes(msg), certificate);
        }
        public string Encrypt(string msg, X509Certificate2 certificate, Encoding encoding)
        {
            return Encrypt(encoding.GetBytes(msg), certificate);
        }
        public string Encrypt(byte[] msg, X509Certificate2 certificate)
        {
            ContentInfo contentInfo = new ContentInfo(msg);
            var signedCms = new SignedCms(contentInfo, true);
            CmsSigner cmsSigner = new CmsSigner(certificate);
            signedCms.ComputeSignature(cmsSigner);
            return Convert.ToBase64String(signedCms.Encode());
        }
        /*
        public string EncryptBase64CSP(string msg, string tumbprint)
        {
            string baseDir = @"d:\temp\ctest";
            string cspDir = @"C:\Program Files\Crypto Pro\CSP";
            string inputFile = System.IO.Path.Combine(baseDir, "csp_input.txt");
            string outputFile = System.IO.Path.Combine(baseDir, "csp_output.txt");
            System.IO.File.WriteAllText(inputFile, msg, Encoding.Default);
            StringBuilder args = new StringBuilder();
            args.Append("-sfsign -sign -in \"");
            args.Append(inputFile);
            args.Append("\" -out \"");
            args.Append(outputFile);
            args.Append("\" -my ");
            args.Append(tumbprint);
            args.Append(" -detached -base64 -add");
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = System.IO.Path.Combine(cspDir, "csptest.exe"),
                WorkingDirectory = cspDir,
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                Arguments = args.ToString()
            });
            process.WaitForExit();
            return string.Join("", System.IO.File.ReadAllLines(outputFile).Where(s=>s!=string.Empty));
        }*/

    }
}
