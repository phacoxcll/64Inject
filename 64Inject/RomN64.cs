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

        private byte[] _data;
        private int _size;
        private Endian _endianness;
        private string _name;
        private char _formatCode;
        private string _id;
        private char _contryCode;
        private byte _version;

        public int Size
        { get { return _size; } }
        public Endian Endianness
        { get { return _endianness; } }
        public string Name
        { get { return _name; } }
        public char FormatCode
        { get { return _formatCode; } }
        public string Id
        { get { return _id; } }
        public char ContryCode
        { get { return _contryCode; } }
        public byte Version
        { get { return _version; } }

        public char Revision
        { get { return _version == 0? ' ' :(char)(_version + 0x40); } }
        public string ProductCode
        { get { return (_formatCode + _id + _contryCode).ToUpper(); } }
        public string ProductCodeVersion
        { get { return ProductCode + _version.ToString(); } }
        public bool IsValid
        { get { return _endianness != Endian.Indeterminate && _size > 0xFFF; } }

        public RomN64(string filename)
        {
            _data = null;
            _size = 0;
            _name = "";
            _formatCode = '?';
            _id = "??";
            _contryCode = '?';
            _version = 0;

            byte[] indicator = new byte[4];
            FileStream fs = File.Open(filename, FileMode.Open);
            _size = (int)fs.Length; 
            fs.Read(indicator, 0, 4);
            fs.Close();
            _endianness = DetermineEndian(indicator);

            if (IsValid)
            {
                byte[] name = new byte[20];
                byte formatCode;
                byte[] id = new byte[2];
                byte contryCode;

                _data = new byte[_size];
                fs = File.Open(filename, FileMode.Open);
                fs.Read(_data, 0, _size);
                fs.Close();
                _data = ToBigEndian(_data, _endianness);

                Array.Copy(_data, 0x20, name, 0, 20);
                formatCode = _data[0x3B];
                id[0] = _data[0x3C];
                id[1] = _data[0x3D];
                contryCode = _data[0x3E];
                _version = _data[0x3F];

                int count = 20;
                while (name[--count] == 0x20 && count > 0) ;

                _name = Encoding.ASCII.GetString(name, 0, count + 1);
                _formatCode = (char)formatCode;
                _id = Encoding.ASCII.GetString(id);
                _contryCode = (char)contryCode;         
            }
            else
                _size = 0;
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
                _data = null;
                disposed = true;
            }
        }

        public bool Save(string filename)
        {
            if (IsValid)
            {
                try
                {
                    FileStream fs = File.Open(filename, FileMode.Create);
                    fs.Write(_data, 0, _data.Length);
                    fs.Close();
                    return true;
                }
                catch { }
            }
            return false;
        }

        public uint GetHashCRC32(uint crc)
        {
            return CRC32.Compute(_data, 0, _data.Length, crc);
        }

        private Endian DetermineEndian(byte[] indicator)
        {
            if (indicator != null && indicator.Length >= 4)
            {
                if (indicator[0] == 0x80 &&
                    indicator[1] == 0x37 &&
                    indicator[2] == 0x12 &&
                    indicator[3] == 0x40)
                    return Endian.BigEndian;
                else if (indicator[0] == 0x37 &&
                    indicator[1] == 0x80 &&
                    indicator[2] == 0x40 &&
                    indicator[3] == 0x12)
                    return Endian.ByteSwapped;
                else if (indicator[0] == 0x40 &&
                    indicator[1] == 0x12 &&
                    indicator[2] == 0x37 &&
                    indicator[3] == 0x80)
                    return Endian.LittleEndian;
            }
            return Endian.Indeterminate;
        }

        private byte[] ToBigEndian(byte[] array, Endian endianness)
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
