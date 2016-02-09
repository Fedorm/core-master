using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ServiceModel.Web;

namespace ImageService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ImageService : IImageRequestHandler
    {
        private String scope;
        private System.Net.NetworkCredential credential;
        private static Dictionary<String, String> mimeTypes;

        public ImageService(String scope, System.Net.NetworkCredential credential)
        {
            this.credential = credential;
            this.scope = scope;

            mimeTypes = new Dictionary<string, string>();
            mimeTypes.Add("jpeg", "image/jpeg");
            mimeTypes.Add("jpg", "image/jpeg");
            mimeTypes.Add("png", "image/png");
        }

        public Stream GetImage(String catalog, String id)
        {
            MemoryStream ms = new MemoryStream();
            String rootFolder = Common.Solution.GetSolutionFolder(scope);
            String catalogFolder = String.Format(@"{0}\filesystem\images\{1}", rootFolder, catalog);
            System.IO.DirectoryInfo dir = new DirectoryInfo(catalogFolder);
            foreach (FileInfo fi in dir.EnumerateFiles(String.Format("{0}.*", id)))
            {
                String ext = fi.Extension.ToLower();
                if (ext.StartsWith("."))
                    ext = ext.Remove(0, 1);

                if (!mimeTypes.ContainsKey(ext))
                    throw new WebFaultException<String>("Unknown mime type", System.Net.HttpStatusCode.UnsupportedMediaType);

                WebOperationContext.Current.OutgoingResponse.ContentType = mimeTypes[ext];
                WebOperationContext.Current.OutgoingResponse.ContentLength = (int)fi.Length;
                using (FileStream fs = fi.OpenRead())
                {
                    fs.CopyTo(ms);
                }
                ms.Position = 0;
                return ms;
            }

            ms = Helper.ImageFromText("No image found");
            WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
            WebOperationContext.Current.OutgoingResponse.ContentLength = (int)ms.Length;
            return ms;
        }

        public System.IO.Stream UploadImage(System.IO.Stream messageBody, String catalog, string id)
        {
            Common.Logon.CheckAdminCredential(scope, credential);

            if (!FileExtensionIsCorrect(id))
                return MakeTextAnswer("Bad file extension. Only supported jpg and png formats");
            String rootFolder = Common.Solution.GetSolutionFolder(scope);
            String fileSystemFolder = String.Format(@"{0}\filesystem", rootFolder);
            if (!System.IO.Directory.Exists(fileSystemFolder))
                System.IO.Directory.CreateDirectory(fileSystemFolder);
            String imagesFolder = String.Format(@"{0}\images", fileSystemFolder);
            if (!System.IO.Directory.Exists(imagesFolder))
                System.IO.Directory.CreateDirectory(imagesFolder);
            String catalogFolder = String.Format(@"{0}\{1}", imagesFolder, catalog);
            if (!System.IO.Directory.Exists(catalogFolder))
                System.IO.Directory.CreateDirectory(catalogFolder);

            String fileName = String.Format(@"{0}\{1}", catalogFolder, id);
            if (System.IO.File.Exists(fileName))
                System.IO.File.Delete(fileName);
            using (System.IO.Stream file = System.IO.File.OpenWrite(fileName))
            {
                messageBody.CopyTo(file);
            }

            return MakeTextAnswer("ok");
        }

        public Stream List(String catalog)
        {
            Common.Logon.CheckAdminCredential(scope, credential);

            MemoryStream ms = new MemoryStream();
            StreamWriter wr = new StreamWriter(ms);
            wr.WriteLine("ok");

            String catalogFolder = String.Format(@"{0}\filesystem\images\{1}", Common.Solution.GetSolutionFolder(scope), catalog);
            if (Directory.Exists(catalogFolder))
            {
                System.IO.DirectoryInfo dir = new DirectoryInfo(catalogFolder);
                foreach (FileInfo fi in dir.EnumerateFiles())
                {
                    wr.WriteLine(String.Format("{0}\t{1}", fi.LastWriteTimeUtc.ToString("dd.MM.yyyy HH:mm:ss"), fi.Name));
                }
            }

            wr.Flush();

            WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
            WebOperationContext.Current.OutgoingResponse.ContentLength = ms.Length;
            ms.Position = 0;
            return ms;
        }

        public Stream DeleteImage(String catalog, String id)
        {
            Common.Logon.CheckAdminCredential(scope, credential);

            String fileName = String.Format(@"{0}\filesystem\images\{1}\{2}", Common.Solution.GetSolutionFolder(scope), catalog, id);
            if (File.Exists(fileName))
                File.Delete(fileName);
            return MakeTextAnswer("ok");
        }

        private bool FileExtensionIsCorrect(String fileName)
        {
            String[] arr = fileName.Split('.');
            if (arr.Length > 1)
                if (mimeTypes.ContainsKey(arr[arr.Length - 1].ToLower()))
                    return true;
            return false;
        }

        private System.IO.Stream MakeTextAnswer(String text)
        {
            MemoryStream ms = new MemoryStream();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;
            return ms;
        }

        private System.IO.Stream MakeExceptionAnswer(Exception e, String scope)
        {
            String text = e.Message;
            while (e.InnerException != null)
            {
                text = text + "; " + e.InnerException.Message;
                e = e.InnerException;
            }
            text = text + e.StackTrace;

            //Common.Solution.Log(scope, "admin", text);

            return MakeTextAnswer(text);
        }

    }
}
