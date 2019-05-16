using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Xml;

namespace _64Inject
{
    public class _64Injector
    {
        public const string Release = "1.2 debug"; //CllVersionReplace "major.minor stability"

        public Cll.Log Log;

        public string BasePath;
        public string ShortName;
        public string LongName;
        public bool DarkFilter;
        public bool Widescreen;
        public string Zoom;
        public string InPath;
        public string RomPath;
        public string IniPath;
        public string BootTvPath;
        public string BootDrcPath;
        public string IconPath;
        public string OutPath;
        public bool Encrypt;

        private VCN64 _base;
        public RomN64 Rom;
        private byte[] _ini;
        public float Scale;
        public BootImage BootTvImg;
        public BootImage BootDrcImg;
        public IconImage IconImg;
        

        public bool BaseIsLoaded
        {
            get { return _base != null; }
        }

        public bool RomIsLoaded
        {
            get { return Rom != null && Rom.IsValid; }
        }

        public bool IniIsLoaded
        {
            get { return _ini != null; }
        }

        public string LoadedBase
        {
            get
            {
                if (_base != null)
                    return _base.ToString();
                else
                    return "";
            }
        }

        public string ShortNameASCII
        {
            get
            {
                char[] array = Useful.Windows1252ToASCII(ShortName, '_').ToCharArray();
                char[] invalid = Path.GetInvalidFileNameChars();

                for (int i = 0; i < array.Length; i++)
                {
                    foreach (char c in invalid)
                    {
                        if (array[i] == c)
                            array[i] = '_';
                    }
                }
                
                return new string(array);
            }
        }

        public string TitleId
        {
            get
            {
                ulong crc;

                if (BaseIsLoaded)
                    crc = _base.HashCRC32;
                else
                    crc = Cll.Security.ComputeCRC32(new byte[] { }, 0, 0);

                if (RomIsLoaded)
                    crc += Rom.HashCRC32;                    
                else
                    crc += Cll.Security.ComputeCRC32(new byte[] { }, 0, 0);
                crc >>= 1;

                if (IniIsLoaded)
                    crc += Cll.Security.ComputeCRC32(_ini, 0, _ini.Length);
                else
                    crc += Cll.Security.ComputeCRC32(new byte[] { }, 0, 0);
                crc >>= 1;

                byte[] b = BitConverter.GetBytes((uint)crc);
                UInt16 id = (UInt16)((((b[3] << 8) + (b[2])) + ((b[1] << 8) + (b[0]))) >> 1);

                int flags = 0;
                flags |= DarkFilter ? 0x80 : 0;
                flags |= Widescreen ? 0x40 : 0;
                flags |= Scale != 1.0F ? 0x20 : 0;
                flags |= IconImg.IsDefault ? 0 : 0x10;
                flags |= BootTvImg.IsDefault ? 0 : 8;
                flags |= BootDrcImg.IsDefault ? 0 : 4;                

                return "0005000064" + id.ToString("X4") + ((byte)(flags)).ToString("X2");
            }
        }
        

        public _64Injector()
        {
            Log = new Cll.Log();

            BasePath = null;
            ShortName = null;
            LongName = null;
            DarkFilter = true;
            Widescreen = false;
            Zoom = null;
            InPath = null;
            RomPath = null;
            IniPath = null;
            IconPath = null;
            BootTvPath = null;
            BootDrcPath = null;
            OutPath = null;
            Encrypt = true;

            _base = GetLoadedBase();
            Rom = null;
            _ini = null;
            Scale = 1.0F;
            BootTvImg = new BootImage();
            BootDrcImg = new BootImage();
            IconImg = new IconImage();
        }

        public bool Inject()
        {
            _base = GetLoadedBase();
            bool _continue = BaseIsLoaded;
            Log.WriteLine("The base is ready: " + _continue.ToString());

            if (_continue)
                _continue = InjectGameLayout();
                
            if (_continue)
                _continue = InjectImages();

            if (_continue)
                _continue = InjectMeta();

            if (_continue)
                _continue = InjectRom();

            if (_continue)
            {
                if (Encrypt)
                {
                    Log.WriteLine("Creating encrypted output.");
                    string inPath = Environment.CurrentDirectory + "\\base";
                    _continue = NusContent.Encrypt(inPath, OutPath);
                }
                else
                {
                    Log.WriteLine("Creating unencrypted output.");
                    _continue = Useful.DirectoryCopy("base", OutPath, true);
                }          
            }

            if (_continue)
                Log.WriteLine("Injection completed successfully!");
            else
                Log.WriteLine("The injection failed.");

            return _continue;
        }
        
        private bool InjectGameLayout()
        {
            FileStream fs = null;
            try
            {
                byte darkFilterB = (byte)(DarkFilter ? 1 : 0);
                byte[] widescreenB = Widescreen ?
                    new byte[] { 0x44, 0xF0, 0, 0 } :
                    new byte[] { 0x44, 0xB4, 0, 0 };
                byte[] scaleB = BitConverter.GetBytes(Scale);

                byte[] magic = new byte[4];
                uint offset = 0;
                uint size = 0;
                byte[] offsetB = new byte[4];
                byte[] sizeB = new byte[4];
                byte[] nameB = new byte[0x18];

                fs = File.Open("base\\content\\FrameLayout.arc", FileMode.Open);
                fs.Read(magic, 0, 4);

                if (magic[0] == 'S' &&
                    magic[1] == 'A' &&
                    magic[2] == 'R' &&
                    magic[3] == 'C')
                {
                    fs.Position = 0x0C;
                    fs.Read(offsetB, 0, 4);
                    offset = (uint)(offsetB[0] << 24 |
                        offsetB[1] << 16 |
                        offsetB[2] << 8 |
                        offsetB[3]);
                    fs.Position = 0x38;
                    fs.Read(offsetB, 0, 4);
                    offset += (uint)(offsetB[0] << 24 |
                        offsetB[1] << 16 |
                        offsetB[2] << 8 |
                        offsetB[3]);

                    fs.Position = offset;
                    fs.Read(magic, 0, 4);

                    if (magic[0] == 'F' &&
                        magic[1] == 'L' &&
                        magic[2] == 'Y' &&
                        magic[3] == 'T')
                    {
                        fs.Position = offset + 0x04;
                        fs.Read(offsetB, 0, 4);
                        offsetB[0] = 0;
                        offsetB[1] = 0;
                        offset += (uint)(offsetB[0] << 24 |
                            offsetB[1] << 16 |
                            offsetB[2] << 8 |
                            offsetB[3]);
                        fs.Position = offset;

                        while (true)
                        {
                            fs.Read(magic, 0, 4);
                            fs.Read(sizeB, 0, 4);
                            size = (uint)(sizeB[0] << 24 |
                                sizeB[1] << 16 |
                                sizeB[2] << 8 |
                                sizeB[3]);                                

                            if (magic[0] == 'p' &&
                                magic[1] == 'i' &&
                                magic[2] == 'c' &&
                                magic[3] == '1')
                            {
                                fs.Position = offset + 0x0C;
                                fs.Read(nameB, 0, 0x18);
                                int count = Array.IndexOf(nameB, (byte)0);
                                string name = Encoding.ASCII.GetString(nameB, 0, count);

                                if (name == "frame")
                                {
                                    fs.Position = offset + 0x44;//Scale
                                    fs.WriteByte(scaleB[3]);
                                    fs.WriteByte(scaleB[2]);
                                    fs.WriteByte(scaleB[1]);
                                    fs.WriteByte(scaleB[0]);
                                    fs.Position = offset + 0x48;//Scale
                                    fs.WriteByte(scaleB[3]);
                                    fs.WriteByte(scaleB[2]);
                                    fs.WriteByte(scaleB[1]);
                                    fs.WriteByte(scaleB[0]);
                                    fs.Position = offset + 0x4C;//Widescreen
                                    fs.Write(widescreenB, 0, 4);
                                }
                                else if (name == "frame_mask")
                                {
                                    fs.Position = offset + 0x08;//Dark filter
                                    fs.WriteByte(darkFilterB);                                        
                                }
                                else if (name == "power_save_bg")
                                {
                                    return true;
                                }

                                offset += size;
                                fs.Position = offset;
                            }
                            else if (offset + size >= fs.Length)
                            {
                            }
                            else
                            {
                                offset += size;
                                fs.Position = offset;
                            }
                        }
                    }
                }                    
                fs.Close();
            }
            catch { Log.WriteLine("Error editing the \"FrameLayout.arc\"."); }
            finally { if (fs != null) fs.Close(); }
            
            return false;
        }

        private bool InjectImages()
        {
            string currentDir = Environment.CurrentDirectory;
            Bitmap bootTvImg;
            Bitmap bootDrcImg;
            Bitmap iconImg;
            Bitmap tmp;
            Graphics g;

            try
            {
                if (BootTvPath != null)
                    tmp = new Bitmap(BootTvPath);
                else
                    tmp = BootTvImg.Create();
                bootTvImg = new Bitmap(1280, 720, PixelFormat.Format24bppRgb);
                g = Graphics.FromImage(bootTvImg);
                g.DrawImage(tmp, new Rectangle(0, 0, 1280, 720));
                g.Dispose();
                tmp.Dispose();

                if (BootDrcPath != null)
                    tmp = new Bitmap(BootDrcPath);
                else
                    tmp = BootDrcImg.Create();
                bootDrcImg = new Bitmap(854, 480, PixelFormat.Format24bppRgb);
                g = Graphics.FromImage(bootDrcImg);
                g.DrawImage(tmp, new Rectangle(0, 0, 854, 480));
                g.Dispose();
                tmp.Dispose();

                if (IconPath != null)
                    tmp = new Bitmap(IconPath);
                else
                    tmp = IconImg.Create();
                iconImg = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
                g = Graphics.FromImage(iconImg);
                g.DrawImage(tmp, new Rectangle(0, 0, 128, 128));
                g.Dispose();
                tmp.Dispose();
            }
            catch
            {
                Log.WriteLine("Error creating images.");
                return false;
            }

            if (!NusContent.ConvertToTGA(bootTvImg, currentDir + "\\base\\meta\\bootTvTex.tga"))
                return false;
            if (!NusContent.ConvertToTGA(bootDrcImg, currentDir + "\\base\\meta\\bootDrcTex.tga"))
                return false;
            if (!NusContent.ConvertToTGA(iconImg, currentDir + "\\base\\meta\\iconTex.tga"))
                return false;

            return true;
        }

        private bool InjectMeta()
        {
            string titleId = TitleId;
            byte[] id = Useful.StrHexToByteArray(titleId, "");

            XmlWriterSettings xmlSettings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace
            };

            XmlDocument xmlApp = new XmlDocument();
            XmlDocument xmlMeta = new XmlDocument();

            try
            {
                xmlApp.Load("base\\code\\app.xml");
                xmlMeta.Load("base\\meta\\meta.xml");

                XmlNode app_title_id = xmlApp.SelectSingleNode("app/title_id");
                XmlNode app_group_id = xmlApp.SelectSingleNode("app/group_id");

                XmlNode meta_product_code = xmlMeta.SelectSingleNode("menu/product_code");
                XmlNode meta_title_id = xmlMeta.SelectSingleNode("menu/title_id");
                XmlNode meta_group_id = xmlMeta.SelectSingleNode("menu/group_id");
                XmlNode meta_longname_en = xmlMeta.SelectSingleNode("menu/longname_en");
                XmlNode meta_shortname_en = xmlMeta.SelectSingleNode("menu/shortname_en");

                app_title_id.InnerText = titleId;
                app_group_id.InnerText = "0000" + id[5].ToString("X2") + id[6].ToString("X2");

                meta_product_code.InnerText = "WUP-N-" + Rom.ProductCode;
                meta_title_id.InnerText = titleId;
                meta_group_id.InnerText = "0000" + id[5].ToString("X2") + id[6].ToString("X2");
                meta_longname_en.InnerText = LongName;
                meta_shortname_en.InnerText = ShortName;

                XmlWriter app = XmlWriter.Create("base\\code\\app.xml", xmlSettings);
                XmlWriter meta = XmlWriter.Create("base\\meta\\meta.xml", xmlSettings);

                xmlApp.Save(app);
                xmlMeta.Save(meta);

                app.Close();
                meta.Close();

                return true;
            }
            catch
            {
                Log.WriteLine("Error editing the \"meta.xml\".");
            }

            return false;
        }

        private bool InjectRom()
        {
            bool injected = true;

            try
            {
                Directory.Delete("base\\content\\config", true);
                Directory.CreateDirectory("base\\content\\config");

                if (IniIsLoaded)
                {
                    FileStream fs = File.Open("base\\content\\config\\U" + Rom.ProductCodeVersion + ".z64.ini", FileMode.Create);
                    fs.Write(_ini, 0, _ini.Length);
                    fs.Close();
                }
                else
                    File.Create("base\\content\\config\\U" + Rom.ProductCodeVersion + ".z64.ini").Close();

                Directory.Delete("base\\content\\rom", true);
                Directory.CreateDirectory("base\\content\\rom");

                if (!RomN64.ToBigEndian(RomPath, "base\\content\\rom\\U" + Rom.ProductCodeVersion + ".z64"))
                    injected = false;
            }
            catch
            {
                Log.WriteLine("Error injecting ROM.");
                injected = false;
            }

            return injected;
        }


        #region Loads

        public bool LoadBase(string path)
        {
            if (Directory.Exists("base"))
                Directory.Delete("base", true);

            if (IsValidBase(path))
                Useful.DirectoryCopy(path, "base", true);

            else if (IsValidEncryptedBase(path))
                NusContent.Decrypt(path, "base");

            else
                Console.WriteLine("The \"" + path + "\" folder not contain a valid base.");

            _base = GetLoadedBase();

            return BaseIsLoaded;
        }

        public bool LoadIni(string filename)
        {
            FileStream fs = File.Open(filename, FileMode.Open);
            _ini = new byte[fs.Length];
            fs.Read(_ini, 0, _ini.Length);
            fs.Close();

            if (Useful.IsUTF8(_ini))
                return true;
            else
            {
                _ini = null;
                return false;
            }
        }

        private VCN64 GetLoadedBase()
        {
            if (IsValidBase("base"))
            {
                FileStream fs = File.Open("base\\code\\VESSEL.rpx", FileMode.Open);
                uint hash = Cll.Security.ComputeCRC32(fs);
                fs.Close();

                if (hash == VCN64.DonkeyKong64.HashCRC32)
                    return VCN64.DonkeyKong64;
                else if (hash == VCN64.Ocarina.HashCRC32)
                    return VCN64.Ocarina;
                else if (hash == VCN64.PaperMario.HashCRC32)
                    return VCN64.PaperMario;
                else if (hash == VCN64.Kirby64.HashCRC32)
                    return VCN64.Kirby64;
                else if (hash == VCN64.MarioTennis.HashCRC32)
                    return VCN64.MarioTennis;
                else if (hash == VCN64.MarioGolf.HashCRC32)
                    return VCN64.MarioGolf;
                else if (hash == VCN64.StarFox64.HashCRC32)
                    return VCN64.StarFox64;
                else if (hash == VCN64.SinAndP.HashCRC32)
                    return VCN64.SinAndP;
                else if (hash == VCN64.MarioKart64.HashCRC32)
                    return VCN64.MarioKart64;
                else if (hash == VCN64.YoshiStory.HashCRC32)
                    return VCN64.YoshiStory;
                else if (hash == VCN64.WaveRace64.HashCRC32)
                    return VCN64.WaveRace64;
                else if (hash == VCN64.Majora.HashCRC32)
                    return VCN64.Majora;
                else if (hash == VCN64.PokemonSnap.HashCRC32)
                    return VCN64.PokemonSnap;
                else if (hash == VCN64.MarioParty2.HashCRC32)
                    return VCN64.MarioParty2;
                else if (hash == VCN64.OgreBattle64.HashCRC32)
                    return VCN64.OgreBattle64;
                else if (hash == VCN64.Excitebike64.HashCRC32)
                    return VCN64.Excitebike64;
                else if (hash == VCN64.FZeroX.HashCRC32)
                    return VCN64.FZeroX;
                else
                    return new VCN64(hash, "", "**Unidentified**");
            }
            else
                return null;
        }
        
        #endregion

        #region Validations

        private bool IsValidBase(string path)
        {
            if (File.Exists(path + "\\code\\app.xml") &&
                File.Exists(path + "\\code\\cos.xml") &&
                File.Exists(path + "\\code\\VESSEL.rpx") &&
                Directory.Exists(path + "\\content\\config") &&
                Directory.Exists(path + "\\content\\rom") &&
                File.Exists(path + "\\content\\BuildInfo.txt") &&
                File.Exists(path + "\\content\\config.ini") &&
                File.Exists(path + "\\content\\FrameLayout.arc") &&
                File.Exists(path + "\\meta\\iconTex.tga") &&
                File.Exists(path + "\\meta\\bootTvTex.tga") &&
                File.Exists(path + "\\meta\\bootDrcTex.tga") &&
                File.Exists(path + "\\meta\\meta.xml"))
                return true;
            else
                return false;
        }

        private bool IsValidEncryptedBase(string path)
        {
            string titleId = NusContent.CheckEncrypted(path);
            if (titleId != null &&
                NusContent.CheckCommonKeyFiles() &&
                File.Exists("resources\\jnustool\\JNUSTool.jar"))
            {
                string name = NusContent.JNUSToolWrapper(path, 400, 32768, titleId, "/code/cos.xml");

                if (name != null && File.Exists(name + "\\code\\cos.xml"))
                {
                    XmlDocument xmlCos = new XmlDocument();
                    xmlCos.Load(name + "\\code\\cos.xml");
                    XmlNode cos_argstr = xmlCos.SelectSingleNode("app/argstr");

                    Directory.Delete(name, true);

                    if (cos_argstr.InnerText == "VESSEL.rpx")
                        return true;
                }
                else if(name != null)
                    Directory.Delete(name, true);
            }
            return false;
        }

        public bool IsValidCode(string code)
        {
            if (code.Length == 4)
            {
                foreach (char c in code)
                {
                    if ((c < 'A' || c > 'Z') && (c < '0' || c > '9'))
                        return false;
                }
            }
            else
                return false;

            return true;
        }

        #endregion
    }
}
