using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;

namespace _64Inject
{
    public static class NusContent
    {
        public const uint CommonKeyHashCRC32 = 0xB92B703D;

        public static string GetJavaVersion()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("java", "-version");
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                Process java = Process.Start(startInfo);
                java.Start();
                java.WaitForExit();
                string version = java.StandardError.ReadLine();
                java.Dispose();
                //version = version.Split(' ')[2].Replace("\"", "");                
                return version;
            }
            catch
            {
                return null;
            }

            /*object vesion = null;
            RegistryKey javaRE = Registry.LocalMachine.OpenSubKey("SOFTWARE\\JavaSoft\\Java Runtime Environment");

            if (javaRE != null)
                version = javaRE.GetValue("CurrentVersion");

            if (version != null)
                return version.ToString();
            else
                return null;*/
        }

        public struct JNUSToolConfig
        {
            public const string UpdateServer = "http://ccs.cdn.wup.shop.nintendo.net/ccs/download";
            public const string TAGAYA1 = "https://tagaya.wup.shop.nintendo.net/tagaya/versionlist/EUR/EU/latest_version";
            public const string TAGAYA2 = "https://tagaya-wup.cdn.nintendo.net/tagaya/versionlist/EUR/EU/list/%d.versionlist";
        }

        public static string Check(string path)
        {
            if (File.Exists(path + "\\code\\app.xml") &&
                Directory.Exists(path + "\\content") &&
                File.Exists(path + "\\meta\\meta.xml"))
            {
                XmlDocument xmlMeta = new XmlDocument();
                xmlMeta.Load(path + "\\meta\\meta.xml");
                XmlNode meta_title_id = xmlMeta.SelectSingleNode("menu/title_id");
                return meta_title_id.InnerText;
            }
            return null;
        }

        public static string CheckEncrypted(string path)
        {
            if (File.Exists(path + "\\title.tmd") &&
                File.Exists(path + "\\title.tik") &&
                File.Exists(path + "\\title.cert"))
            {
                byte[] id = new byte[8];
                FileStream fs = File.OpenRead(path + "\\title.tmd");
                fs.Position = 0x18C;
                fs.Read(id, 0, 8);
                fs.Close();
                return BitConverter.ToString(id).Replace("-", "");
            }
            return null;
        }
        
        public static void CheckBatchFiles()
        {
            if (!Directory.Exists("resources"))
                Directory.CreateDirectory("resources");

            if (!File.Exists("resources\\pack.bat"))
            {
                StreamWriter sw = File.CreateText("resources\\pack.bat");
                sw.WriteLine("@echo off");
                sw.WriteLine("cd resources\\nuspacker");
                sw.Write("java -jar NUSPacker.jar -in %1 -out %2");
                sw.Close();
            }

            if (!File.Exists("resources\\unpack.bat"))
            {
                StreamWriter sw = File.CreateText("resources\\unpack.bat");
                sw.WriteLine("@echo off");
                sw.WriteLine("cd resources\\jnustool");
                sw.Write("java -jar JNUSTool.jar %1 -file %2");
                sw.Close();
            }
        }

        private static bool IsValidCommonKey(string key)
        {
            byte[] array = Encoding.ASCII.GetBytes(key.ToUpper());
            uint hash = Cll.Security.ComputeCRC32(array, 0, array.Length);
            if (hash == CommonKeyHashCRC32)
                return true;
            return false;
        }
                
        public static bool LoadKey(string key)
        {
            if (NusContent.IsValidCommonKey(key))
            {
                try
                {
                    if (!Directory.Exists("resources\\nuspacker"))
                        Directory.CreateDirectory("resources\\nuspacker");
                    if (!Directory.Exists("resources\\jnustool"))
                        Directory.CreateDirectory("resources\\jnustool");

                    StreamWriter sw = File.CreateText("resources\\nuspacker\\encryptKeyWith");
                    sw.WriteLine(key.ToUpper());
                    sw.Close();
                    sw = File.CreateText("resources\\jnustool\\config");
                    sw.WriteLine(NusContent.JNUSToolConfig.UpdateServer);
                    sw.WriteLine(key.ToUpper());
                    sw.WriteLine("updatetitles.csv");
                    sw.WriteLine(NusContent.JNUSToolConfig.TAGAYA1);
                    sw.Write(NusContent.JNUSToolConfig.TAGAYA2);
                    sw.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public static bool CheckCommonKeyFiles()
        {
            StreamReader sr;
            StreamWriter sw;
            string jnustoolKey = "";
            string nuspackerKey = "";
            bool jnustoolKeyValid = false;
            bool nuspackerKeyValid = false;

            if (File.Exists("resources\\jnustool\\config"))
            {
                sr = File.OpenText("resources\\jnustool\\config");
                sr.ReadLine();
                jnustoolKey = sr.ReadLine();
                sr.Close();
                jnustoolKeyValid = IsValidCommonKey(jnustoolKey);
            }

            if (File.Exists("resources\\nuspacker\\encryptKeyWith"))
            {
                sr = File.OpenText("resources\\nuspacker\\encryptKeyWith");
                nuspackerKey = sr.ReadLine();
                sr.Close();
                nuspackerKeyValid = IsValidCommonKey(nuspackerKey);
            }

            if (jnustoolKeyValid && nuspackerKeyValid)
                return true;
            else if (jnustoolKeyValid && !nuspackerKeyValid)
            {
                try
                {
                    sw = File.CreateText("resources\\nuspacker\\encryptKeyWith");
                    sw.WriteLine(jnustoolKey);
                    sw.Close();
                    return true;
                }
                catch
                {
                    Cll.Log.WriteLine("Error restoring Wii U common key in nuspacker.");
                    return false;
                }
            }
            else if (!jnustoolKeyValid && nuspackerKeyValid)
            {
                try
                {
                    sw = File.CreateText("resources\\jnustool\\config");
                    sw.WriteLine(JNUSToolConfig.UpdateServer);
                    sw.WriteLine(nuspackerKey);
                    sw.WriteLine("updatetitles.csv");
                    sw.WriteLine(JNUSToolConfig.TAGAYA1);
                    sw.Write(JNUSToolConfig.TAGAYA2);
                    sw.Close();
                    return true;
                }
                catch
                {
                    Cll.Log.WriteLine("Error restoring Wii U common key in jnustool.");
                    return false;
                }
            }
            else
            {
                Cll.Log.WriteLine("You have not entered the Wii U common key.");
                return false;
            }
        }
        
        public static bool Encrypt(string inputPath, string outputPath)
        {
            string titleId = Check(inputPath);
            if (titleId != null &&
                CheckCommonKeyFiles() &&
                GetJavaVersion() != null &&
                File.Exists("resources\\nuspacker\\NUSPacker.jar"))
            {
                NusContent.CheckBatchFiles();
                Process encrypt = Process.Start("resources\\pack.bat", "\"" + inputPath + "\" \"" + outputPath + "\"");
                encrypt.WaitForExit();
                encrypt.Dispose();
                return true;
            }
            return false;
        }

        public static bool Decrypt(string inputPath, string outputPath)
        {
            string titleId = CheckEncrypted(inputPath);
            if (titleId != null &&
                CheckCommonKeyFiles() &&
                GetJavaVersion() != null &&
                File.Exists("resources\\jnustool\\JNUSTool.jar"))
            {
                string name = JNUSToolWrapper(inputPath, 0, Int32.MaxValue, titleId, ".*");
                if (name != null)
                {
                    Useful.DirectoryCopy(name, outputPath, true);
                    Directory.Delete(name, true);
                    return true;
                }
            }
            return false;
        }

        public static string JNUSToolWrapper(string inputPath, int minFileLength, int maxFileLength, string titleId, string filter)
        {
            if (inputPath == null)
                throw new Exception("JNUSToolWrapper: Input path is null.");
            if (minFileLength < 0)
                throw new Exception("JNUSToolWrapper: Min file length < zero.");
            if (minFileLength > maxFileLength)
                throw new Exception("JNUSToolWrapper: Min file length > Max file length.");
            if (titleId == null)
                throw new Exception("JNUSToolWrapper: Title Id is null.");

            try
            {
                string[] directories = Directory.GetDirectories("resources\\jnustool");
                foreach (string dir in directories)
                    Directory.Delete(dir, true);

                Directory.CreateDirectory("resources\\jnustool\\tmp_" + titleId);

                string[] file = Directory.GetFiles(inputPath);
                FileInfo[] fileInfo = new FileInfo[file.Length];

                for (int i = 0; i < file.Length; i++)
                {
                    fileInfo[i] = new FileInfo(file[i]);
                    if (fileInfo[i].Length >= minFileLength && fileInfo[i].Length <= maxFileLength)
                        File.Copy(inputPath + "\\" + fileInfo[i].Name, "resources\\jnustool\\tmp_" + titleId + "\\" + fileInfo[i].Name, true);
                }

                NusContent.CheckBatchFiles();
                Process decrypt = Process.Start("resources\\unpack.bat", titleId + " " + filter);
                decrypt.WaitForExit();
                decrypt.Dispose();

                Directory.Delete("resources\\jnustool\\tmp_" + titleId, true);
                string[] result = Directory.GetDirectories("resources\\jnustool");

                if (result.Length == 1)
                    return result[0];
            }
            catch
            {
                Cll.Log.WriteLine("JNUSToolWrapper: Failed.");
            }
            return null;
        }

        public static bool ConvertToTGA(Bitmap image, string outputFileName)
        {
            if (image.Width > 0xFFFF || image.Height > 0xFFFF)
                return false;

            byte[] header = {
                    0x00, 0x00, 0x02, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    (byte)(image.Width & 0xFF),
                    (byte)((image.Width >> 8) & 0xFF),
                    (byte)(image.Height & 0xFF),
                    (byte)((image.Height >> 8) & 0xFF),
                    0x00, 0x00 };

            byte[] footer = {
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x54, 0x52, 0x55, 0x45,
                    0x56, 0x49, 0x53, 0x49,
                    0x4F, 0x4E, 0x2D, 0x58,
                    0x46, 0x49, 0x4C, 0x45,
                    0x2E, 0x00 };

            int bytesPerPixel = 0;
            if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                header[0x10] = 0x20;
                header[0x11] = 0x08;
                bytesPerPixel = 4;
            }
            else if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                header[0x10] = 0x18;
                bytesPerPixel = 3;
            }
            else
                return false;

            Bitmap bmp = new Bitmap(image.Width, image.Height, image.PixelFormat);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height));
            g.Dispose();

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            int length = data.Width * data.Height * bytesPerPixel;
            byte[] bmpBytes = new byte[length];
            for (int y = 0; y < data.Height; ++y)
            {
                IntPtr ptr = (IntPtr)((long)data.Scan0 + y * data.Stride);
                System.Runtime.InteropServices.Marshal.Copy(
                    ptr, bmpBytes, y * data.Width * bytesPerPixel, data.Width * bytesPerPixel);
            }
            bmp.UnlockBits(data);
            bmp.Dispose();

            FileStream fs = File.Open(outputFileName, FileMode.Create);
            fs.Write(header, 0, header.Length);
            fs.Write(bmpBytes, 0, bmpBytes.Length);
            fs.Write(footer, 0, footer.Length);
            fs.Close();

            return true;
        }
    }
}
