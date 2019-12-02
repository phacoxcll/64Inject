using System;
using System.IO;
using System.Text;

namespace _64Inject
{
    public class RomN64
    {
        public enum Subformat
        {
            BigEndian,
            LittleEndian,
            ByteSwapped,
            Indeterminate
        }

        public Subformat Endianness
        { private set; get; }

        public int Size
        { private set; get; }
        public string Title
        { private set; get; }
        public char FormatCode
        { private set; get; }
        public string ShortId
        { private set; get; }
        public char RegionCode
        { private set; get; }
        public byte Version
        { private set; get; }

        public char Revision
        { get { return Version == 0? ' ' :(char)(Version + 0x40); } }
        public string ProductCode
        { get { return (FormatCode + ShortId + RegionCode).ToUpper(); } }
        public string ProductCodeVersion
        { get { return ProductCode + Version.ToString("X"); } }
        public bool IsValid
        { private set; get; }
        public ushort HashCRC16
        { private set; get; }

        public RomN64(string filename)
        {
            Size = 0;
            Title = "";
            FormatCode = '?';
            ShortId = "??";
            RegionCode = '?';
            Version = 0;
            IsValid = false;
            HashCRC16 = 0;

            try
            {
                Cll.Log.WriteLine("Validating N64 ROM.");

                byte[] header = new byte[0x40];
                FileStream fs = File.Open(filename, FileMode.Open);
                Size = (int)fs.Length;
                fs.Read(header, 0, 0x40);
                fs.Close();

                Endianness = GetFormat(header);

                if (Endianness != Subformat.Indeterminate && Size % 4 == 0)
                {
                    byte uniqueCode;
                    byte[] shortTitle = new byte[2];
                    byte region;

                    header = ToBigEndian(header, Endianness);

                    uniqueCode = header[0x3B];
                    shortTitle[0] = header[0x3C];
                    shortTitle[1] = header[0x3D];
                    region = header[0x3E];
                    Version = header[0x3F];
                    FormatCode = (char)uniqueCode;
                    ShortId = Encoding.ASCII.GetString(shortTitle);
                    RegionCode = (char)region;

                    byte[] titleBytes = new byte[20];
                    Array.Copy(header, 0x20, titleBytes, 0, 20);
                    int count = 20;
                    while (titleBytes[--count] == 0x20 && count > 0) ;
                    Title = Encoding.ASCII.GetString(titleBytes, 0, count + 1);

                    fs = File.Open(filename, FileMode.Open);
                    //Size = (int)fs.Length;
                    HashCRC16 = Cll.Security.ComputeCRC16(fs);
                    fs.Close();

                    IsValid = true;
                    Cll.Log.WriteLine("The N64 ROM is valid and its metadata has been loaded.");
                }
                else
                {
                    Size = 0;
                    Cll.Log.WriteLine("It was not possible to determine the N64 ROM format.");
                }
            }
            catch
            {
                Cll.Log.WriteLine("Error reading N64 ROM.");
            }
        }

        private static Subformat GetFormat(byte[] header)
        {
            Subformat format = Subformat.Indeterminate;

            if (header[0] == 0x80 &&
                header[1] == 0x37 &&
                header[2] == 0x12 &&
                header[3] == 0x40)
                format = Subformat.BigEndian;
            else if (header[0] == 0x37 &&
                header[1] == 0x80 &&
                header[2] == 0x40 &&
                header[3] == 0x12)
                format = Subformat.ByteSwapped;
            else if (header[0] == 0x40 &&
                header[1] == 0x12 &&
                header[2] == 0x37 &&
                header[3] == 0x80)
                format = Subformat.LittleEndian;

            return format;
        }
        
        public static bool ToBigEndian(string source, string destination)
        {
            RomN64 rom = new RomN64(source);

            if (rom.IsValid)
            {
                try
                {
                    FileStream fs = File.Open(source, FileMode.Open);
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    fs.Close();

                    data = ToBigEndian(data, rom.Endianness);
                    fs = File.Open(destination, FileMode.Create);
                    fs.Write(data, 0, data.Length);
                    fs.Close();

                    return true;
                }
                catch { }
            }
            return false;
        }

        private static byte[] ToBigEndian(byte[] array, Subformat endianness)
        {
            if (array.Length % 4 != 0)
                throw new Exception("ToBigEndian: Invalid array length.");

            byte[] bigEndian = new byte[4];

            if (endianness == Subformat.ByteSwapped)
            {
                for (int i = 0; i < array.Length / 4; i++)
                {
                    bigEndian[0] = array[(i * 4) + 1];
                    bigEndian[1] = array[(i * 4) + 0];
                    bigEndian[2] = array[(i * 4) + 3];
                    bigEndian[3] = array[(i * 4) + 2];

                    array[(i * 4) + 0] = bigEndian[0];
                    array[(i * 4) + 1] = bigEndian[1];
                    array[(i * 4) + 2] = bigEndian[2];
                    array[(i * 4) + 3] = bigEndian[3];
                }
            }
            else if (endianness == Subformat.LittleEndian)
            {
                for (int i = 0; i < array.Length / 4; i++)
                {
                    bigEndian[0] = array[(i * 4) + 3];
                    bigEndian[1] = array[(i * 4) + 2];
                    bigEndian[2] = array[(i * 4) + 1];
                    bigEndian[3] = array[(i * 4) + 0];

                    array[(i * 4) + 0] = bigEndian[0];
                    array[(i * 4) + 1] = bigEndian[1];
                    array[(i * 4) + 2] = bigEndian[2];
                    array[(i * 4) + 3] = bigEndian[3];
                }
            }

            return array;
        }
    }
}
