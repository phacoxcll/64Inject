using System;
using System.IO;
using System.Text;

namespace _64Inject
{
    public class RomN64 : IDisposable
    {
        public enum Endian
        {
            BigEndian,
            LittleEndian,
            ByteSwapped,
            Indeterminate
        }

        private bool disposed = false;

        public int Size
        { private set; get; }
        public Endian Endianness
        { private set; get; }
        public string Title
        { private set; get; }
        public char FormatCode
        { private set; get; }
        public string Id
        { private set; get; }
        public char ContryCode
        { private set; get; }
        public byte Version
        { private set; get; }

        public char Revision
        { get { return Version == 0? ' ' :(char)(Version + 0x40); } }
        public string ProductCode
        { get { return (FormatCode + Id + ContryCode).ToUpper(); } }
        public string ProductCodeVersion
        { get { return ProductCode + Version.ToString("X1"); } }
        public bool IsValid
        { private set; get; }
        public uint HashCRC32
        { private set; get; }

        public RomN64(string filename)
        {
            Size = 0;
            Title = "";
            FormatCode = '?';
            Id = "??";
            ContryCode = '?';
            Version = 0;

            try
            {
                byte[] header = new byte[0x40];
                FileStream fs = File.Open(filename, FileMode.Open);
                Size = (int)fs.Length;
                fs.Read(header, 0, 0x40);
                fs.Close();

                if (Size > 0xFFF)
                {
                    if (header[0] == 0x80 &&
                        header[1] == 0x37 &&
                        header[2] == 0x12 &&
                        header[3] == 0x40)
                        Endianness = Endian.BigEndian;
                    else if (header[0] == 0x37 &&
                        header[1] == 0x80 &&
                        header[2] == 0x40 &&
                        header[3] == 0x12)
                        Endianness = Endian.ByteSwapped;
                    else if (header[0] == 0x40 &&
                        header[1] == 0x12 &&
                        header[2] == 0x37 &&
                        header[3] == 0x80)
                        Endianness = Endian.LittleEndian;
                    else
                        Endianness = Endian.Indeterminate;

                    if (Endianness != Endian.Indeterminate && Size % 4 == 0)
                    {
                        IsValid = true;

                        byte formatCode;
                        byte[] id = new byte[2];
                        byte contryCode;

                        header = ToBigEndian(header, Endianness);

                        formatCode = header[0x3B];
                        id[0] = header[0x3C];
                        id[1] = header[0x3D];
                        contryCode = header[0x3E];
                        Version = header[0x3F];
                        FormatCode = (char)formatCode;
                        Id = Encoding.ASCII.GetString(id);
                        ContryCode = (char)contryCode;

                        byte[] titleBytes = new byte[20];
                        Array.Copy(header, 0x20, titleBytes, 0, 20);
                        int count = 20;
                        while (titleBytes[--count] == 0x20 && count > 0) ;
                        Title = Encoding.ASCII.GetString(titleBytes, 0, count + 1);

                        fs = File.Open(filename, FileMode.Open);
                        HashCRC32 = Cll.Security.ComputeCRC32(fs);
                        fs.Close();
                    }
                }
                else
                    Size = 0;
            }
            catch { }
        }

        ~RomN64()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                }
                disposed = true;
            }
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

        private static byte[] ToBigEndian(byte[] array, Endian endianness)
        {
            if (array.Length % 4 != 0)
                throw new Exception("ToBigEndian: Invalid array length.");

            byte[] bigEndian = new byte[4];

            if (endianness == Endian.ByteSwapped)
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
            else if (endianness == Endian.LittleEndian)
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
