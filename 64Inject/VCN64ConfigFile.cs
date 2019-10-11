using System.IO;

namespace _64Inject
{
    public class VCN64ConfigFile
    {
        public bool IsValid
        { private set; get; }
        public uint HashCRC32
        { private set; get; }

        public VCN64ConfigFile(string filename)
        {
            IsValid = VCN64Config.Validator.Evaluate(filename);

            if (IsValid)
            {
                FileStream fs = File.Open(filename, FileMode.Open);
                HashCRC32 = Cll.Security.ComputeCRC32(fs);
                fs.Close();
            }
            else
                HashCRC32 = 0;
        }

        public static bool Copy(string source, string destination)
        {
            if (VCN64Config.Validator.Evaluate(source))
            {
                try
                {
                    File.Copy(source, destination);
                    return true;
                }
                catch { }
            }
            return false;
        }
    }
}
