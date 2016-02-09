using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO.Compression;
using System.Security.AccessControl;
using System.IO;

using System.Globalization;
namespace FileHelperForCloud
{
    public static class FileHelper
    {
        public static string CreateZipArchieve(string path)
        {
            string returnedString = string.Empty;
            string zippingFilePath = System.IO.Path.GetTempPath() + "ResourceFolder.zip";

            try
            {
                if (System.IO.File.Exists(zippingFilePath)) System.IO.File.Delete(zippingFilePath);
                ZipFile.CreateFromDirectory(path, zippingFilePath, CompressionLevel.Optimal, false, Encoding.UTF8);
            }
            catch (Exception e)
            {
                throw e;
            }
            return zippingFilePath;
        }

        public static Stream StreamFromZipFile(string path)
        {
            try
            {
                using (FileStream fsSource = new FileStream(path,
                FileMode.Open, FileAccess.Read))
                {

                    // Read the source file into a byte array.
                    byte[] bytes = new byte[fsSource.Length];
                    int numBytesToRead = (int)fsSource.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        // Read may return anything from 0 to numBytesToRead.
                        int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                        // Break when the end of the file is reached.
                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                    numBytesToRead = bytes.Length;

                    MemoryStream stream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(System.Text.Encoding.ASCII.GetString(bytes));
                    writer.Flush();
                    stream.Position = 0;
                    return stream;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static void DeleteDirectory(string deletingDirectory)
        {
            try
            {
                string[] files = Directory.GetFiles(deletingDirectory);
                string[] dirs = Directory.GetDirectories(deletingDirectory);

                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string dir in dirs)
                {
                    DeleteDirectory(dir);
                }

                Directory.Delete(deletingDirectory, false);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string CreateUnzipFolder(string path, string destinationPath, string subfolder = "resource")
        {
            try
            {
                string returnedString = string.Empty;
                string fullDestinationPath = Path.Combine(destinationPath, subfolder);
                if (System.IO.Directory.Exists(fullDestinationPath))
                {
                    DeleteDirectory(fullDestinationPath);
                }
                System.IO.Directory.CreateDirectory(fullDestinationPath);

                using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Update, Encoding.UTF8))
                {
                    archive.ExtractToDirectory(fullDestinationPath);
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return destinationPath;
        }
        public static string CreateZipArchieve(string path, string zippingFilePath)
        {
            string returnedString = string.Empty;

            try
            {
                if (System.IO.File.Exists(zippingFilePath))
                    System.IO.File.Delete(zippingFilePath);
                ZipFile.CreateFromDirectory(path, zippingFilePath, CompressionLevel.Optimal, false, Encoding.UTF8);
            }
            catch (Exception e)
            {
                throw e;
            }
            return zippingFilePath;
        }

        public static byte[] GetBytesFromFile(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                return bytes;
            }

        }
    }
}
