using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Common
{
    public class Crypt
    {
        public static byte[] IV = new byte[] { 1, 45, 134, 7, 200, 88, 34, 16, 1, 45, 134, 7, 200, 88, 34, 16 };

        public static int TotalLicenses;

        public static void ReadLicenses()
        {
            String dir = GetLicensesPath();
            Trace.TraceInformation("Reading licenses from {0}", dir);
            LicenseInfo[] licenses = GetLicenses();
            int qty = 0;
            foreach (LicenseInfo li in licenses)
            {
                Trace.TraceInformation("Found license {0}", li);
                qty += li.Qty;
            }
            TotalLicenses = (qty == 0) ? 0 : qty;
            Trace.TraceInformation("Total licenses: {0}", TotalLicenses);
        }

        public static LicenseInfo[] GetLicenses()
        {
            String dir = GetLicensesPath();
            Dictionary<Guid, LicenseInfo> licenses = new Dictionary<Guid, LicenseInfo>();
            if (System.IO.Directory.Exists(dir))
            {
                foreach (String lFile in System.IO.Directory.EnumerateFiles(dir))
                {
                    try
                    {
                        LicenseInfo li = GetLicenseInfo(lFile);
                        if (!licenses.ContainsKey(li.Id))
                            licenses.Add(li.Id, li);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Exception in GetLicenses: {0}", Utils.MakeDetailedExceptionString(ex));
                    }
                }
            }
            return licenses.Values.ToArray<LicenseInfo>();
        }

        public static LicenseInfo GetLicenseInfo(String lFile)
        {
            return GetLicenseInfo(System.IO.File.OpenRead(lFile));
        }

        public static LicenseInfo GetLicenseInfo(Stream lFile)
        {
            String xml = DecryptStream(lFile);
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            XmlNode node = doc.DocumentElement;

            LicenseInfo li = new LicenseInfo();
            li.Server = node.ChildNodes[0].InnerText;
            li.Id = Guid.Parse(node.ChildNodes[1].InnerText);
            li.Name = node.ChildNodes[2].InnerText;
            li.Qty = int.Parse(node.ChildNodes[3].InnerText);
            li.ExpireDate = DateTime.Parse(node.ChildNodes[4].InnerText);

            return li;
        }

        public static String DecryptStream(Stream data)
        {
            byte[] key = GetKey();
            using (BinaryReader sr = new BinaryReader(data))
            {
                return DecryptStringFromBytes_Aes(sr.ReadBytes((int)data.Length) , key, IV);
            }
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Padding = PaddingMode.PKCS7;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        csDecrypt.Write(cipherText,0,cipherText.Length);
                    }
                    var decrypted = msDecrypt.ToArray();
                    plaintext = System.Text.Encoding.UTF8.GetString(decrypted);
                }

            }

            return plaintext;

        }

        public static byte[] GetKey()
        {
            Dictionary<string, string> ids =
            new Dictionary<string, string>();

            ManagementObjectSearcher searcher;
            

            try
            {
                //процессор
                searcher = new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_Processor");
                foreach (var queryObj in searcher.Get())
                    ids.Add("ProcessorId", queryObj["ProcessorId"].ToString());
            }
            catch (Exception ex)
            {
            }

            try
            {
                //мать
                searcher = new ManagementObjectSearcher("root\\CIMV2",
                       "SELECT * FROM CIM_Card");
                foreach (var queryObj in searcher.Get())
                    ids.Add("CardID", queryObj["SerialNumber"].ToString());
            }
            catch { }

            /*
            try
            {
                //клавиатура
                searcher = new ManagementObjectSearcher("root\\CIMV2",
                       "SELECT * FROM CIM_KeyBoard");
                foreach (ManagementObject queryObj in searcher.Get())
                    ids.Add("KeyBoardID", queryObj["DeviceId"].ToString());
            }
            catch { }
            */
            try
            {
                //ОС
                searcher = new ManagementObjectSearcher("root\\CIMV2",
                       "SELECT * FROM CIM_OperatingSystem");
                foreach (var queryObj in searcher.Get())
                    ids.Add("OSSerialNumber", queryObj["SerialNumber"].ToString());
            }
            catch { }

            /*
            try
            {
                //мышь
                searcher = new ManagementObjectSearcher("root\\CIMV2",
                       "SELECT * FROM Win32_PointingDevice");
                foreach (ManagementObject queryObj in searcher.Get())
                    ids.Add("MouseID", queryObj["DeviceID"].ToString());
            }
            catch { }

            try
            {
                //звук
                searcher = new ManagementObjectSearcher("root\\CIMV2",
                       "SELECT * FROM Win32_SoundDevice");
                foreach (ManagementObject queryObj in searcher.Get())
                    ids.Add("SoundCardID", queryObj["DeviceID"].ToString());
            }
            catch { }
            
            try
            {
                //CD-ROM
                searcher = new ManagementObjectSearcher("root\\CIMV2",
                       "SELECT * FROM Win32_CDROMDrive");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    ids.Add("CDROMID", queryObj["DeviceID"].ToString());
                    break;
                }
            }
            catch { }
            */
            try
            {
                //UUID
                searcher = new ManagementObjectSearcher("root\\CIMV2",
                       "SELECT UUID FROM Win32_ComputerSystemProduct");
                foreach (var queryObj in searcher.Get())
                    ids.Add("UUID", queryObj["UUID"].ToString());
            }
            catch { }

            String key = "";
            foreach (var x in ids)
            {
                key = key + x.Value;
                //Console.WriteLine(x.Key + ": " + x.Value);
            }

            MD5 md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(key));
        }

        public static String GetLicensesPath()
        {
            return String.Format(@"{0}\licenses", System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath);
        }
    }
}
