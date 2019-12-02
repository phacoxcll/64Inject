using System;
using System.IO;

namespace _64Inject
{
    public class VCN64ConfigFile
    {
        public bool IsValid
        { private set; get; }
        public ushort HashCRC16
        { private set; get; }

        public VCN64ConfigFile(string filename)
        {
            IsValid = false;
            HashCRC16 = 0;

            try
            {
                IsValid = Validate(filename);
            }
            catch
            {
                Cll.Log.WriteLine("Warning! \"VCN64Config.exe\" program not found.");
                IsValid = UTF8Validator(filename);
            }

            if (IsValid)
            {
                try
                {
                    FileStream fs = File.Open(filename, FileMode.Open);
                    HashCRC16 = Cll.Security.ComputeCRC16(fs);
                    fs.Close();
                    Cll.Log.WriteLine("The configuration file is valid and its metadata has been loaded.");
                }
                catch
                {
                    HashCRC16 = 0;
                    Cll.Log.WriteLine("Error reading \"" + filename + "\".");
                }
            }
            else
                Console.WriteLine("The file \"" + filename + "\" is invalid.");
        }

        public static bool Copy(string source, string destination)
        {
            bool valid = false;
            try
            {
                valid = Validate(source);
            }
            catch
            {
                Cll.Log.WriteLine("Warning! \"VCN64Config.exe\" program not found.");
                valid = UTF8Validator(source);
            }

            if (valid)
            {
                try
                {
                    File.Copy(source, destination);
                    return true;
                }
                catch
                {
                    Cll.Log.WriteLine("Failed to copy configuration file.");
                }
            }
            
            return false;
        }

        private static bool Validate(string filename)
        {
            return VCN64Config.Validator.Evaluate(filename);
        }

        public static bool UTF8Validator(string filename)
        {
            Cll.Log.WriteLine("The UTF8 validator will be used, it is not 100% reliable.");

            bool result = false;

            if (File.Exists(filename))
            {
                FileStream fs = null;
                try
                {
                    fs = File.Open(filename, FileMode.Open);
                    byte[] file = new byte[fs.Length];
                    fs.Read(file, 0, file.Length);
                    fs.Close();
                    if (Useful.IsUTF8(file))
                    {
                        Cll.Log.WriteLine("The file \"" + filename + "\" is valid!");
                        result = true;
                    }
                }
                catch (Exception e)
                {
                    Cll.Log.WriteLine("The file \"" + filename + "\" is invalid.\n" + e.Message);
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }
            else
                Console.WriteLine("The file \"" + filename + "\" not exists.");

            return result;
        }
    }
}
