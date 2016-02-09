using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace AdminService
{
    class Zip
    {
        public static System.IO.Stream UnzipStream(System.IO.Stream input, bool zippedStream = true, String contentEncoding = null, String fileName = null)
        {
            System.IO.Stream tempStream = null;

            if (contentEncoding == null)
                contentEncoding = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.ContentEncoding];

            if (String.IsNullOrEmpty(contentEncoding) || !zippedStream)
            {
                if (!String.IsNullOrEmpty(fileName))
                    return System.IO.File.OpenRead(fileName);
                else
                    return input;
            }
            else
            {
                switch (contentEncoding.ToLower())
                {
                    case "gzip":
                        if (!String.IsNullOrEmpty(fileName))
                            tempStream = System.IO.File.OpenRead(fileName);

                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(tempStream != null ? tempStream : input, System.IO.Compression.CompressionMode.Decompress, true))
                        {
                            gzip.CopyTo(ms);
                        }
                        ms.Position = 0;

                        return ms;

                    case "deflate":
                        String tempFileName = null;

                        if (String.IsNullOrEmpty(fileName))
                        {
                            tempFileName = Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
                            using (FileStream fs = System.IO.File.OpenWrite(tempFileName))
                            {
                                input.CopyTo(fs);
                                fs.Flush();
                            }
                        }

                        try
                        {
                            using (ZipArchive a = ZipFile.OpenRead(String.IsNullOrEmpty(fileName) ? tempFileName : fileName))
                            {
                                ZipArchiveEntry entry = a.Entries[0];
                                return entry.Open();
                            }
                        }
                        finally
                        {
                            if (!String.IsNullOrEmpty(tempFileName))
                                System.IO.File.Delete(tempFileName);
                        }

                    default:
                        throw new Exception("Unsupported content type");
                }
            }

        }

        /*
        public static System.IO.Stream UnzipStream(System.IO.Stream input, String contentEncoding = null, String fileName = null)
        {
            if (contentEncoding == null)
                contentEncoding = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.ContentEncoding];
            if (!String.IsNullOrEmpty(contentEncoding))
            {
                System.IO.Stream ms = null;
                switch (contentEncoding.ToLower())
                {
                    case "gzip":
                        if (!String.IsNullOrEmpty(fileName))
                            input = System.IO.File.OpenRead(fileName);

                        ms = new System.IO.MemoryStream();
                        using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress, true))
                        {
                            gzip.CopyTo(ms);
                        }
                        ms.Position = 0;

                        if (!String.IsNullOrEmpty(fileName))
                            input.Dispose();

                        return ms;

                    case "deflate":
                        if (!String.IsNullOrEmpty(fileName))
                        {

                        }


                        ms = new System.IO.MemoryStream();
                        using (System.IO.Compression.DeflateStream deflate = new System.IO.Compression.DeflateStream(input, System.IO.Compression.CompressionMode.Decompress, true))
                        {
                            deflate.CopyTo(ms);
                        }
                        ms.Position = 0;
                        return ms;
                    default:
                        throw new Exception("Unsupported content encofing !");
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(fileName))
                {
                    return System.IO.File.OpenRead(fileName);
                }
                else
                    return input;
            }
        }
        */
        public static System.IO.Stream ZipStream(System.IO.Stream input)
        {
            String acceptEncoding = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.AcceptEncoding];
            if (!String.IsNullOrEmpty(acceptEncoding))
            {
                System.IO.Stream ms = null;
                String[] acceptEncodings = acceptEncoding.Split(',');
                switch (acceptEncodings[0].Trim().ToLower())
                {
                    case "gzip":
                        ms = new System.IO.MemoryStream();
                        using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true))
                        {
                            input.CopyTo(gzip);
                        }
                        ms.Position = 0;
                        WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
                        return ms;

                    case "deflate":

                        throw new NotImplementedException();
                        /*
                        ms = new System.IO.MemoryStream();
                        using (System.IO.Compression.DeflateStream deflate = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress, true))
                        {
                            input.CopyTo(deflate);
                        }
                        ms.Position = 0;
                        WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.ContentEncoding] = "deflate";
                        return ms;
                        */
                }
            }

            return input;
        }

    }
}
