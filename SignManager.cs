using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Security;
using iTextSharp.text.pdf.security;
using iTextSharp.text.pdf;
using System.IO;
using System.Net;

namespace lnsign
{
    public class Cert
    {
        public string Subject { get; set; }
        public string Description { get; set; }
    }

    public class SignDocumentRequestBody
    {
        public string URInputPdf { get; set; }

        public string URIOutputPdf { get; set; }

        public string URImgSignature { get; set; }
        public string CertSubject { get; set; }

        public string SignatureReason { get; set; }


        public string SignatureLocation { get; set; }

        public string token { get; set; }
        public string uploadUrl { get; set; }
        public int pacienteId { get; set; }
        public int profissionalId { get; set; }

        public string email { get; set; }
        public string celular { get; set; }

        public bool enviarEmail { get; set; }
        public bool enviarSms { get; set; }

      
        public int ImgSignaturePositionX { get; set; }

     
        public int ImgSignaturePositionY { get; set; }

   
        public int ImgSignatureHeight { get; set; }


        public int ImgSignatureWidth { get; set; }
    }

    public class SignManager
    {
        static SignManager _instancia;

        public static SignManager Instancia
        {
            get { return _instancia ?? (_instancia = new SignManager()); }
        }

        private bool IsLocalAbsolutePath(string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
            {
                return uri.IsFile;
            }

            return false;
        }

        private bool IsRemoteAbsolutePath(string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
            {
                return !uri.IsFile;
            }

            return false;
        }

        public bool IsValidURI(string uri)
        {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                return false;
            Uri tmp;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out tmp))
                return false;
            return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
        }

        public string getExternalDoc(string path)
        {
            if (!this.IsValidURI(path))
            {
                return path;
            }

            var loc = new Uri(path);
            var fi = new FileInfo(loc.AbsolutePath);

            string filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "." + fi.Extension);

            using (WebClient myWebClient = new WebClient())
            {
                myWebClient.DownloadFile(path, filePath);
            }

            return filePath;
        }

        public List<Cert> ListCerts()
        {
            X509Store store;
            store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = store.Certificates;
            //ListaNomeCertificados.Add("Não");
            List<Cert> certList = new List<Cert>();

            foreach (X509Certificate2 x509 in certCollection)
            {
                try
                {
                    if (((RSACryptoServiceProvider)(x509.PrivateKey)) == null || ((RSACryptoServiceProvider)(x509.PrivateKey)).CspKeyContainerInfo == null)
                    {
                        continue;
                    }

                    bool isRemovable = ((RSACryptoServiceProvider)(x509.PrivateKey)).CspKeyContainerInfo.Removable;
                    bool isHardware = ((RSACryptoServiceProvider)(x509.PrivateKey)).CspKeyContainerInfo.HardwareDevice;

                    if (isRemovable && isHardware)
                    {
                        certList.Add(new Cert { Subject = x509.Subject, Description = x509.Subject.Split('=')[1].Split(',')[0] });
                    }
                }
                catch (Exception e)
                {

                }
            }

            return certList;
        }

        private SecureString GetSecurePin(string PinCode)
        {
            SecureString pwd = new SecureString();
            foreach (var c in PinCode.ToCharArray()) pwd.AppendChar(c);
            return pwd;
        }

        private X509Certificate2 getCertBySubject(string Subject)
        {
            X509Store store;
            store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = store.Certificates;

            X509Certificate2 target = null;
            foreach (X509Certificate2 x509 in certCollection)
            {
                try
                {
                    if (!x509.HasPrivateKey)
                    {
                        throw new Exception("Chave privada do certificado nao foi gerada");
                    }
                    if (x509.PrivateKey == null || ((RSACryptoServiceProvider)(x509.PrivateKey)) == null || ((RSACryptoServiceProvider)(x509.PrivateKey)).CspKeyContainerInfo == null)
                    {
                        continue;
                    }

                    bool isRemovable = ((RSACryptoServiceProvider)(x509.PrivateKey)).CspKeyContainerInfo.Removable;
                    bool isHardware = ((RSACryptoServiceProvider)(x509.PrivateKey)).CspKeyContainerInfo.HardwareDevice;

                    if (isRemovable && isHardware && x509.Subject == Subject)
                    {
                        target = x509;
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return target;
        }

        public ResponseSign SignDoc(SignDocumentRequestBody signConfig)
        {
            try
            {
                if (signConfig == null)
                {
                    throw new Exception("Defina os parametros da requisição");
                }

                if (signConfig.CertSubject == null)
                {
                    throw new Exception("Defina certSubject antes de assinar");
                }

                var x509cert = this.getCertBySubject(signConfig.CertSubject);

                if (x509cert == null)
                {
                    throw new Exception("O certificado selecionado não foi encontrado");
                }

                string PinCode = "";
                //if pin code is set then no windows form will popup to ask it
                SecureString pwd = GetSecurePin(PinCode);
                RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)x509cert.PrivateKey;
                rsa.PersistKeyInCsp = false;
                //this.selected
                CspParameters csp = new CspParameters(1,
                                                        rsa.CspKeyContainerInfo.ProviderName,
                                                        rsa.CspKeyContainerInfo.KeyContainerName,
                                                        new System.Security.AccessControl.CryptoKeySecurity(),
                                                        pwd);

                return this.SignWithThisCert(x509cert, signConfig);
                // the pin code will be cached for next access to the smart card
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UploadFileSigned(string filePath, SignDocumentRequestBody signConfig)
        {
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            // Add your parameters here  
            postParameters.Add("file", new FormUpload.FileParameter(File.ReadAllBytes(filePath), Path.GetFileName(filePath), "image/pdf"));
            postParameters.Add("pacienteId", signConfig.pacienteId);
            postParameters.Add("email", signConfig.email);
            postParameters.Add("celular", signConfig.celular);
            postParameters.Add("enviarEmail", signConfig.enviarEmail);
            postParameters.Add("enviarSms", signConfig.enviarSms);
            postParameters.Add("profissionalId", signConfig.profissionalId);

            string userAgent = "Someone";
            HttpWebResponse webResponse = FormUpload.MultipartFormPost(signConfig.uploadUrl, userAgent, postParameters, "authorization", signConfig.token);
            // Process response  
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            var returnResponseText = responseReader.ReadToEnd();
            webResponse.Close();

            Console.WriteLine(returnResponseText);

        }

        private ResponseSign SignWithThisCert(X509Certificate2 cert, SignDocumentRequestBody signConfig)
        {
            string SourcePdfFileName = this.getExternalDoc(signConfig.URInputPdf);
            string filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
            string DestPdfFileName = signConfig.URIOutputPdf;

            if (SourcePdfFileName == null)
            {
                throw new Exception("Defina o caminho do documento >> caminhoDocumento");
            }

            if (DestPdfFileName == null)
            {
                throw new Exception("Defina o destino do documento assinado >> caminhoDocumentoAssinado");
            }

            if (signConfig.URImgSignature == null)
            {
                throw new Exception("Defina a imagem da assinatura >> caminhoImgAssinatura");
            }

            var assinatura = this.getExternalDoc(signConfig.URImgSignature);

            if (!File.Exists(assinatura))
            {
                throw new Exception("Assinatura " + assinatura + " nao existe");
            }

            Org.BouncyCastle.X509.X509CertificateParser cp = new Org.BouncyCastle.X509.X509CertificateParser();
            Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[] { cp.ReadCertificate(cert.RawData) };

            IExternalSignature externalSignature = new X509Certificate2Signature(cert, "SHA-1");
            using (PdfReader pdfReader = new PdfReader(SourcePdfFileName))
            {
                using (FileStream signedPdf = new FileStream(DestPdfFileName, FileMode.Create, FileAccess.Write))
                {  //the output pdf file
                    using (PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, signedPdf, '\0'))
                    {
                        PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;
                        //here set signatureAppearance at your will
                        signatureAppearance.Reason = signConfig.SignatureReason;
                        signatureAppearance.Location = signConfig.SignatureLocation;
                        // pesquisar
                        // signature field in jasper software
                        signatureAppearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC;
                        signatureAppearance.SignatureGraphic = iTextSharp.text.Image.GetInstance(this.getExternalDoc(assinatura));
                        iTextSharp.text.Rectangle cropBox = pdfReader.GetCropBox(pdfReader.NumberOfPages);

                        iTextSharp.text.Rectangle pagesize;
                        if (pdfReader.GetPageRotation(1) == 90 || pdfReader.GetPageRotation(1) == 270)
                        {
                            pagesize = pdfReader.GetPageSizeWithRotation(1);
                        }
                        else
                        {
                            pagesize = pdfReader.GetPageSize(1);
                        }

                        float _mm2Pt = Convert.ToSingle(0.352777778);
                        int x = signConfig.ImgSignaturePositionX;
                        int y = signConfig.ImgSignaturePositionY;

                        int w = signConfig.ImgSignatureWidth;
                        int h = signConfig.ImgSignatureHeight;
                        //var currentPage = pdfReader.GetPageN[1];

                        var pHeight = pagesize.Height;
                        var bX = (x / _mm2Pt);
                        var bY = pHeight - (y / _mm2Pt) - (h / _mm2Pt);
                        var bW = bX + (w / _mm2Pt);
                        var bH = bY + (h / _mm2Pt);

                        // https://forum.aspose.com/t/what-is-llx-lly-yrx-and-ury/33291/5
                        var rect = new iTextSharp.text.Rectangle(bX, bY, bW, bH);
                        signatureAppearance.SetVisibleSignature(
                      rect,
                      pdfReader.NumberOfPages,
                      "Signature");
                        MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, null, 0, CryptoStandard.CMS);
                    }
                }
            }

            var bytes = File.ReadAllBytes(DestPdfFileName);
            String file = Convert.ToBase64String(bytes);

            if (signConfig.pacienteId != 0)
            {
                this.UploadFileSigned(DestPdfFileName, signConfig);
            }

            return new ResponseSign
            {
                Base64 = file
            };
        }
    }
}
