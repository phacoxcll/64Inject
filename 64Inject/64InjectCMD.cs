using System;
using System.IO;

namespace _64Inject
{
    public class _64InjectCMD
    {
        private _64Injector injector;

        public _64InjectCMD()
        {
            injector = new _64Injector();
        }

        public void Run(string[] args)
        {
            injector.Log.WriteLine("64Inject " + _64Injector.Release + " - by phacox.cll");
            injector.Log.WriteLine("");

            if (NusContent.GetJavaVersion() == null)
            {
                injector.Log.WriteLine("Warning! Apparently the system does not have Java installed!");
                injector.Log.WriteLine("Java is needed to NUSPacker (encrypt) and JNUSTool (decrypt).");
                injector.Log.WriteLine("");
            }
            else
            {
                if (!File.Exists("resources\\nuspacker\\NUSPacker.jar"))
                {
                    injector.Log.WriteLine("Warning! \"resources\\nuspacker\\NUSPacker.jar\" not found!");
                    injector.Log.WriteLine("NUSPacker allows you to encrypt the result for WUPInstaller.");
                    injector.Log.WriteLine("");
                }
                if (!File.Exists("resources\\jnustool\\JNUSTool.jar"))
                {
                    injector.Log.WriteLine("Warning! \"resources\\jnustool\\JNUSTool.jar\" not found!");
                    injector.Log.WriteLine("JNUSTool allows you to decrypt Wii U VC games to use as a base.");
                    injector.Log.WriteLine("");
                }
            }            

            if (NusContent.CheckCommonKeyFiles())
                injector.Log.WriteLine("Common Key found successfully.");
            else
                injector.Log.WriteLine("Common Key not found. Use load-key option.");

            if (injector.BaseIsLoaded)
                injector.Log.WriteLine("Base loaded: " + injector.LoadedBase);
            else
                injector.Log.WriteLine("Base not found. Use load-base option or specify one using the -base option.");
            injector.Log.WriteLine("");

            if (args.Length == 0)
            {
                ConsoleHelp();
            }
            else if (args.Length == 1 && args[0] == "help")
            {
                ConsoleHelp();
            }
            else if (args.Length == 2 && args[0] == "load-key")
            {
                injector.Log.WriteLine("load-key: " + args[1]);

                if (NusContent.LoadKey(args[1]))
                    injector.Log.WriteLine("Common Key loaded successfully.");
                else
                    injector.Log.WriteLine("The load of the key failed. Is a valid Common Key?");
            }
            else if (args.Length == 2 && args[0] == "load-base")
            {
                injector.Log.WriteLine("load-base: " + args[1]);
                injector.Log.WriteLine("Loading base, please wait...");

                if (injector.LoadBase(args[1]))
                    injector.Log.WriteLine("Base loaded: " + injector.LoadedBase);
                else
                    injector.Log.WriteLine("The base was not loaded correctly.");
            }
            else if (args.Length >= 1 && args[0] == "create-image")
            {
                if (CreateImage(args))
                    injector.Log.WriteLine("Image created.");
                else
                    injector.Log.WriteLine("Image not created.");
            }
            else if (args.Length >= 1 && args[0] == "create-icon")
            {
                if (CreateIcon(args))
                    injector.Log.WriteLine("Icon created.");
                else
                    injector.Log.WriteLine("Icon not created.");
            }
            else if ((args.Length == 3 || args.Length == 5) &&
                (args[0] == "pack" || args[0] == "unpack"))
            {
                PackUnpack(args);
            }
            else
            {
                bool _continue = LoadArgs(args);

                if (_continue)
                    _continue = ValidArgs();      

                if (_continue)
                    _continue = Inject();
            }
            //Console.ReadLine();
        }

        private void ConsoleHelp()
        {
            injector.Log.WriteText(global::_64Inject.Properties.Resources.Help, 80, 0, Cll.Log.TabMode.All);
        }

        private bool CreateImage(string[] args)
        {
            BootImage bootImg = new BootImage();
            string framePath = null;
            string titlePath = null;            
            string released = null;
            string players = null;
            string outPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "create-image":
                        break;
                    case "-frame":
                        if (i + 1 < args.Length && framePath == null)
                        {
                            framePath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-frame\" option.");
                            return false;
                        }
                    case "-title":
                        if (i + 1 < args.Length && titlePath == null)
                        {
                            titlePath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-title\" option.");
                            return false;
                        }
                    case "-name":
                        if (i + 1 < args.Length && bootImg.NameLine1 == null)
                        {
                            bootImg.NameLine1 = args[++i];
                            bootImg.NameLine2 = "";
                            bootImg.Longname = false;
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-name\" option.");
                            return false;
                        }
                    case "-longname":
                        if (i + 2 < args.Length && bootImg.NameLine1 == null)
                        {
                            bootImg.NameLine1 = args[++i];
                            bootImg.NameLine2 = args[++i];
                            bootImg.Longname = true;
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-longname\" option.");
                            return false;
                        }
                    case "-r":
                        if (i + 1 < args.Length && released == null)
                        {
                            released = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-r\" option.");
                            return false;
                        }
                    case "-p":
                        if (i + 1 < args.Length && players == null)
                        {
                            players = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-p\" option.");
                            return false;
                        }
                    case "-out":
                        if (i + 1 < args.Length && outPath == null)
                        {
                            outPath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-out\" option.");
                            return false;
                        }
                    default:
                        injector.Log.WriteLine("Invalid option \"" + args[i] + "\".");
                        return false;
                }
            }

            if (framePath != null && !File.Exists(framePath))
            {
                injector.Log.WriteLine("The frame image \"" + framePath + "\" not exists.");
                return false;
            }
            if (titlePath != null && !File.Exists(titlePath))
            {
                injector.Log.WriteLine("The title screen \"" + titlePath + "\" not exists.");
                return false;
            }
            if (released != null)
            {
                try
                {
                    bootImg.Released = Convert.ToInt32(released);
                    if (bootImg.Released < 1996)
                    {
                        injector.Log.WriteLine("The year of release is less than 1996.");
                        return false;
                    }
                }
                catch
                {
                    injector.Log.WriteLine("The year of release is not an integer.");
                    return false;
                }
            }
            if (players != null)
            {
                try
                {
                    bootImg.Players = Convert.ToInt32(players);
                    if (bootImg.Players < 1 || bootImg.Players > 4)
                    {
                        injector.Log.WriteLine("The number of players must be between 1 and 4.");
                        return false;
                    }
                }
                catch
                {
                    injector.Log.WriteLine("The number of players is not an integer.");
                    return false;
                }
            }
            if (outPath != null)
            {
                if (!Directory.Exists(outPath))
                {
                    injector.Log.WriteLine("The \"" + outPath + "\" folder not exist.");
                    return false;
                }
            }
            else
                outPath = Environment.CurrentDirectory;

            outPath += "\\image.png";
            injector.Log.WriteLine("Creating image ----------------------------------------------------------------");
            if (titlePath != null) injector.Log.WriteLine("title: " + titlePath);
            if (framePath != null) injector.Log.WriteLine("frame: " + framePath);
            if (bootImg.NameLine1 != "")
            {
                if (bootImg.NameLine2 == "") injector.Log.WriteLine("name: " + bootImg.NameLine1);
                else injector.Log.WriteLine("longname:\n" + bootImg.NameLine1 + "\n" + bootImg.NameLine2);
            }
            if (released != null) injector.Log.WriteLine("released: " + released);
            if (players != null) injector.Log.WriteLine("players: " + players);
            if (outPath != null) injector.Log.WriteLine("out: " + outPath);

            System.Drawing.Bitmap image = null;
            try
            {
                if (framePath != null)
                    bootImg.Frame = new System.Drawing.Bitmap(framePath);
                if (titlePath != null)
                    bootImg.TitleScreen = new System.Drawing.Bitmap(titlePath);
                image = bootImg.Create();
                image.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
                image.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                bootImg.Dispose();
                if (image != null)
                    image.Dispose();
            }
        }

        private bool CreateIcon(string[] args)
        {
            IconImage iconImg = new IconImage();
            string framePath = null;
            string titlePath = null;
            string outPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "create-icon":
                        break;
                    case "-frame":
                        if (i + 1 < args.Length && framePath == null)
                        {
                            framePath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-frame\" option.");
                            return false;
                        }
                    case "-title":
                        if (i + 1 < args.Length && titlePath == null)
                        {
                            titlePath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-title\" option.");
                            return false;
                        }
                    case "-out":
                        if (i + 1 < args.Length && outPath == null)
                        {
                            outPath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-out\" option.");
                            return false;
                        }
                    default:
                        injector.Log.WriteLine("Invalid option \"" + args[i] + "\".");
                        return false;
                }
            }

            if (framePath != null && !File.Exists(framePath))
            {
                injector.Log.WriteLine("The frame image \"" + framePath + "\" not exists.");
                return false;
            }
            if (titlePath != null && !File.Exists(titlePath))
            {
                injector.Log.WriteLine("The title screen \"" + titlePath + "\" not exists.");
                return false;
            }
            if (outPath != null)
            {
                if (!Directory.Exists(outPath))
                {
                    injector.Log.WriteLine("The \"" + outPath + "\" folder not exist.");
                    return false;
                }
            }
            else
                outPath = Environment.CurrentDirectory;

            outPath += "\\icon.png";
            injector.Log.WriteLine("Creating icon -----------------------------------------------------------------");
            if (titlePath != null) injector.Log.WriteLine("title: " + titlePath);
            if (framePath != null) injector.Log.WriteLine("frame: " + framePath);
            if (outPath != null) injector.Log.WriteLine("out: " + outPath);

            System.Drawing.Bitmap icon = null;
            try
            {
                if (framePath != null)
                    iconImg.Frame = new System.Drawing.Bitmap(framePath);
                if (titlePath != null)
                    iconImg.TitleScreen = new System.Drawing.Bitmap(titlePath);
                icon = iconImg.Create();
                icon.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
                icon.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                iconImg.Dispose();
                if (icon != null)
                    icon.Dispose();
            }
        }

        private void PackUnpack(string[] args)
        {
            bool _continue = true;
            if (args.Length == 3 && args[1] == "-in")
            {
                injector.InPath = args[2];
                injector.OutPath = Environment.CurrentDirectory + "\\" + args[0] + "_result";

                if (Directory.Exists(injector.OutPath))
                    Directory.Delete(injector.OutPath, true);
                Directory.CreateDirectory(injector.OutPath);
            }
            else if (args.Length == 5 && args[1] == "-in" && args[3] == "-out")
            {
                injector.InPath = args[2];
                injector.OutPath = args[4];
            }
            else
            {
                injector.Log.WriteLine("Incorrect " + args[0] + " syntax.");
                injector.Log.WriteLine("");
                _continue = false;
            }

            if (_continue)
            {
                if (!Directory.Exists(injector.InPath) || !Path.IsPathRooted(injector.InPath))
                {
                    injector.Log.WriteLine("The input folder \"" + injector.InPath + "\" not exist.");
                    injector.Log.WriteLine("Do not use relative paths.");
                    injector.Log.WriteLine("");
                    _continue = false;
                }
                if (!Directory.Exists(injector.OutPath) || !Path.IsPathRooted(injector.OutPath))
                {
                    injector.Log.WriteLine("The output folder \"" + injector.OutPath + "\" not exist.");
                    injector.Log.WriteLine("Do not use relative paths.");
                    injector.Log.WriteLine("");
                    _continue = false;
                }
            }

            if (_continue && Directory.Exists(injector.OutPath))
                if (Directory.GetDirectories(injector.OutPath).Length != 0 ||
                    Directory.GetFiles(injector.OutPath).Length != 0)
                {
                    injector.Log.WriteLine("The \"" + injector.OutPath + "\" folder exist and not empty.");
                    injector.Log.WriteLine("");
                    _continue = false;
                }

            if (_continue)
            {
                injector.Log.WriteLine("in: " + injector.InPath);
                injector.Log.WriteLine("out: " + injector.OutPath);
                injector.Log.WriteLine("");

                if (args[0] == "pack")
                {
                    if (NusContent.Encrypt(injector.InPath, injector.OutPath))
                        injector.Log.WriteLine("The encryption was successful.");
                    else
                        injector.Log.WriteLine("The encryption failed.");
                }
                else if (args[0] == "unpack")
                {
                    if (NusContent.Decrypt(injector.InPath, injector.OutPath))
                        injector.Log.WriteLine("The decryption was successful.");
                    else
                        injector.Log.WriteLine("The decryption failed.");
                }
            }
            else if (args[0] == "pack")
                injector.Log.WriteLine("The encryption failed.");
            else
                injector.Log.WriteLine("The decryption failed.");
        }

        private bool Inject()
        {
            injector.Log.WriteLine("Injecting ---------------------------------------------------------------------");
            if (injector.BasePath != null) injector.Log.WriteLine("base: " + injector.BasePath);
            if (injector.ShortName != null) injector.Log.WriteLine("name: " + injector.ShortName);
            if (injector.LongName != null) injector.Log.WriteLine("longname:\n" + injector.LongName);
            if (injector.InPath != null) injector.Log.WriteLine("in: " + injector.InPath);
            if (injector.RomPath != null) injector.Log.WriteLine("rom: " + injector.RomPath);
            if (injector.IniPath != null) injector.Log.WriteLine("ini: " + injector.IniPath);
            if (injector.BootTvPath != null) injector.Log.WriteLine("tv: " + injector.BootTvPath);
            if (injector.BootDrcPath != null) injector.Log.WriteLine("drc: " + injector.BootDrcPath);
            if (injector.IconPath != null) injector.Log.WriteLine("icon: " + injector.IconPath);
            if (injector.OutPath != null) injector.Log.WriteLine("out: " + injector.OutPath);
            injector.Log.WriteLine("encrypt: " + injector.Encrypt.ToString());
            injector.Log.WriteLine("Please wait...");

            return injector.Inject();
        }

        private bool LoadArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-base":
                        if (i + 1 < args.Length && injector.BasePath == null)
                        { 
                            injector.BasePath = args[++i];
                            break;
                        }
                        else
                        { 
                            injector.Log.WriteLine("Error in the \"-base\" option.");
                            return false;
                        }
                    case "-name":
                        if (i + 1 < args.Length && injector.ShortName == null)
                        {
                            injector.ShortName = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-name\" option.");
                            return false;
                        }
                    case "-longname":
                        if (i + 2 < args.Length && injector.LongName == null)
                        {
                            injector.LongName = args[++i] + "\n" + args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-longname\" option.");
                            return false;
                        }
                    case "no-darkfilter":
                        injector.DarkFilter = false; 
                        break;
                    case "widescreen":
                        injector.Widescreen = true;                        
                        break;
                    case "-zoom":
                        if (i + 1 < args.Length && injector.Zoom == null)
                        {
                            injector.Zoom = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-zoom\" option.");
                            return false;
                        }
                    case "-in":
                        if (i + 1 < args.Length && injector.InPath == null)
                        {
                            injector.InPath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-in\" option.");
                            return false;
                        }
                    case "-rom":
                        if (i + 1 < args.Length && injector.RomPath == null)
                        {
                            injector.RomPath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-rom\" option.");
                            return false;
                        }
                    case "-ini":
                        if (i + 1 < args.Length && injector.IniPath == null)
                        {
                            injector.IniPath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-ini\" option.");
                            return false;
                        }
                    case "-icon":
                        if (i + 1 < args.Length && injector.IconPath == null)
                        {
                            injector.IconPath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-icon\" option.");
                            return false;
                        }
                    case "-tv":
                        if (i + 1 < args.Length && injector.BootTvPath == null)
                        {
                            injector.BootTvPath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-tv\" option.");
                            return false;
                        }
                    case "-drc":
                        if (i + 1 < args.Length && injector.BootDrcPath == null)
                        {
                            injector.BootDrcPath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-drc\" option.");
                            return false;
                        }
                    case "-out":
                        if (i + 1 < args.Length && injector.OutPath == null)
                        {
                            injector.OutPath = args[++i];
                            break;
                        }
                        else
                        {
                            injector.Log.WriteLine("Error in the \"-out\" option.");
                            return false;
                        }
                    case "not-encrypt":
                        injector.Encrypt = false;
                        break;
                    default:
                        injector.Log.WriteLine("Invalid option \"" + args[i] + "\".");
                        return false;
                }
            }
            return true;
        }

        #region Validations

        private bool ValidArgs()
        {
            if (!ValidBasePath())
                return false;

            if (!ValidInPath())
                return false;

            if (!ValidRomPath())
                return false;

            if (!ValidIniPath())
                return false;

            if (!ValidShortName())
                return false;

            if (!ValidLongName())
                return false;

            if (!ValidZoom())
                return false;

            if (!ValidBootTvPath())
                return false;

            if (!ValidBootDrcPath())
                return false;

            if (!ValidIconPath())
                return false;

            if (!ValidOutPath())
                return false;

            return true;
        }

        private bool ValidBasePath()
        {
            if (injector.BasePath != null)
            {
                injector.Log.WriteLine("Loading base, please wait...");

                if (injector.LoadBase(injector.BasePath))
                {
                    injector.Log.WriteLine("Base loaded: " + injector.LoadedBase);
                    injector.Log.WriteLine("");
                }
                else
                {
                    injector.Log.WriteLine("The base was not loaded correctly.");
                    injector.Log.WriteLine("");
                    return false;
                }
            }
            else if (!injector.BaseIsLoaded)
            {
                injector.Log.WriteLine("There is not a loaded base.");
                injector.Log.WriteLine("");
                return false;
            }
            return true;
        }

        private bool ValidShortName()
        {
            if (injector.ShortName != null)
            {
                if (injector.ShortName.Length == 0)
                {
                    injector.Log.WriteLine("The name of the game is empty.");
                    return false;
                }
                else if (injector.ShortName.Length > 256)
                {
                    injector.Log.WriteLine("The name is not valid, it has more than 256 characters.");
                    return false;
                }
            }
            else
            {
                injector.ShortName = injector.Rom.Title;
            }
            return true;
        }

        private bool ValidLongName()
        {
            if (injector.LongName != null)
            {
                if (injector.ShortName.Length == 0)
                {
                    injector.Log.WriteLine("The long name of the game is empty.");
                    return false;
                }
                else if (injector.LongName.Length > 512)
                {
                    injector.Log.WriteLine("The long name is not valid, it has more than 512 characters.");
                    return false;
                }
            }
            else
            {
                injector.LongName = injector.ShortName;
            }
            return true;
        }
        
        private bool ValidZoom()
        {
            if (injector.Zoom != null)
            {
                try
                {
                    injector.Scale = Convert.ToSingle(injector.Zoom);
                    if (injector.Scale < 50.0F || injector.Scale > 200.0F)
                    {
                        injector.Log.WriteLine("The zoom value must be between 50 and 200.");
                        return false;
                    }
                }
                catch
                {
                    injector.Log.WriteLine("The zoom value is not a number.");
                    return false;
                }

                if (injector.ShortName.Length == 0)
                {
                    injector.Log.WriteLine("The name of the game is empty.");
                    return false;
                }
                else if (injector.ShortName.Length > 256)
                {
                    injector.Log.WriteLine("The name is not valid, it has more than 256 characters.");
                    return false;
                }
            }
            else
            {
                injector.Scale = 100.0F;
            }
            return true;
        }
        
        private bool ValidInPath()
        {
            if (injector.InPath != null)
            {
                if (!Directory.Exists(injector.InPath))
                {
                    injector.Log.WriteLine("The input folder \"" + injector.InPath + "\" not exists.");
                    return false;
                }
            }
            else
                injector.Log.WriteLine("The input folder was not specified.");
            return true;
        }

        private bool ValidRomPath()
        {
            if (injector.RomPath != null)
            {
                if (File.Exists(injector.RomPath))
                {
                    injector.Rom = new RomN64(injector.RomPath);
                    if (!injector.Rom.IsValid)
                    {
                        injector.Log.WriteLine("The ROM file is not valid.");
                        return false;
                    }
                }
                else
                {
                    injector.Log.WriteLine("The ROM file \"" + injector.RomPath + "\" not exists.");
                    return false;
                }
            }
            else
            {
                if (injector.InPath != null)
                {
                    injector.RomPath = injector.InPath + "\\rom.z64";
                    if (File.Exists(injector.RomPath))
                    {
                        injector.Rom = new RomN64(injector.RomPath);
                        if (!injector.Rom.IsValid)
                        {
                            injector.Log.WriteLine("The ROM file is not valid.");
                            return false;
                        }
                    }
                    else
                    {
                        injector.Log.WriteLine("The ROM file \"" + injector.RomPath + "\" not exists.");
                        return false;
                    }
                }
                else
                {
                    //injector.Log.WriteLine("The input folder was not specified.");
                    injector.Log.WriteLine("ROM not found.");
                    return false;
                }
            }
            return true;
        }

        private bool ValidIniPath()
        {
            if (injector.IniPath != null)
            {
                if (File.Exists(injector.IniPath))
                {
                    if (!injector.LoadIni(injector.IniPath))
                    {
                        injector.Log.WriteLine("The INI file is not valid.");
                        return false;
                    }
                }
                else
                {
                    injector.Log.WriteLine("The INI file \"" + injector.IniPath + "\" not exists.");
                    return false;
                }
            }
            else
            {
                if (injector.InPath != null)
                {
                    injector.IniPath = injector.InPath + "\\vc.ini";
                    if (File.Exists(injector.IniPath))
                    {
                        if (!injector.LoadIni(injector.IniPath))
                        {
                            injector.Log.WriteLine("The INI file is not valid.");
                            return false;
                        }
                    }
                    else
                    {
                        injector.IniPath = null;
                        injector.Log.WriteLine("The INI file \"" + injector.IniPath + "\" not exists.");
                        injector.Log.WriteLine("An empty INI file will be used.");
                    }
                }
                else
                {
                    //injector.Log.WriteLine("The input folder was not specified.");
                    injector.Log.WriteLine("An empty INI file will be used.");
                }
            }
            return true;
        }

        private bool ValidBootTvPath()
        {
            if (injector.BootTvPath != null)
            {
                if (!File.Exists(injector.BootTvPath))
                {
                    injector.Log.WriteLine("The boot tv image file \"" + injector.BootTvPath + "\" not exists.");
                    return false;
                }
                else
                    injector.BootTvImg.IsDefault = false;
            }
            else
            {
                if (injector.InPath != null)
                {
                    injector.BootTvPath = injector.InPath + "\\tv.png";
                    if (!File.Exists(injector.BootTvPath))
                    {
                        injector.Log.WriteLine("The boot tv image file \"" + injector.BootTvPath + "\" not exists.");
                        if (File.Exists("resources\\boot.png"))
                            injector.BootTvPath = "resources\\boot.png";
                        else
                            injector.BootTvPath = null;
                        injector.Log.WriteLine("A default image will be used as boot tv.");
                    }
                    else
                        injector.BootTvImg.IsDefault = false;
                }
                else
                {
                    //injector.Log.WriteLine("The input folder was not specified.");
                    if (File.Exists("resources\\boot.png"))
                        injector.BootTvPath = "resources\\boot.png";
                    else
                        injector.BootTvPath = null;
                    injector.Log.WriteLine("A default image will be used as boot tv.");
                }
            }
            return true;
        }

        private bool ValidBootDrcPath()
        {
            if (injector.BootDrcPath != null)
            {
                if (!File.Exists(injector.BootDrcPath))
                {
                    injector.Log.WriteLine("The boot drc image file \"" + injector.BootDrcPath + "\" not exists.");
                    return false;
                }
                else
                    injector.BootDrcImg.IsDefault = false;
            }
            else
            {
                if (injector.InPath != null)
                {
                    injector.BootDrcPath = injector.InPath + "\\drc.png";
                    if (!File.Exists(injector.BootDrcPath))
                    {
                        injector.Log.WriteLine("The boot drc image file \"" + injector.BootDrcPath + "\" not exists.");
                        injector.BootDrcPath = injector.BootTvPath;
                        if (!File.Exists(injector.BootDrcPath))
                        {
                            if (File.Exists("resources\\boot.png"))
                                injector.BootDrcPath = "resources\\boot.png";
                            else
                                injector.BootDrcPath = null;
                            injector.Log.WriteLine("A default image will be used as boot drc.");
                        }
                        else
                            injector.Log.WriteLine("The boot tv image will be used as boot drc.");
                    }
                    else
                        injector.BootDrcImg.IsDefault = false;
                }
                else
                {
                    //injector.Log.WriteLine("The input folder was not specified.");
                    injector.BootDrcPath = injector.BootTvPath;
                    if (!File.Exists(injector.BootDrcPath))
                    {
                        if (File.Exists("resources\\boot.png"))
                            injector.BootDrcPath = "resources\\boot.png";
                        else
                            injector.BootDrcPath = null;
                        injector.Log.WriteLine("A default image will be used as boot drc.");
                    }
                    else
                        injector.Log.WriteLine("The boot tv image will be used as boot drc.");
                }
            }
            return true;
        }

        private bool ValidIconPath()
        {
            if (injector.IconPath != null)
            {
                if (!File.Exists(injector.IconPath))
                {
                    injector.Log.WriteLine("The icon file \"" + injector.IconPath + "\" not exists.");
                    return false;
                }
                else
                    injector.IconImg.IsDefault = false;
            }
            else
            {
                if (injector.InPath != null)
                {
                    injector.IconPath = injector.InPath + "\\icon.png";
                    if (!File.Exists(injector.IconPath))
                    {
                        injector.Log.WriteLine("The icon file \"" + injector.IconPath + "\" not exists.");
                        if (File.Exists("resources\\icon.png"))
                            injector.IconPath = "resources\\icon.png";                        
                        else
                            injector.IconPath = null;  
                        injector.Log.WriteLine("A default image will be used as icon.");
                    }
                    else
                        injector.IconImg.IsDefault = false;
                }
                else
                {
                    //injector.Log.WriteLine("The input folder was not specified.");
                    if (File.Exists("resources\\icon.png"))
                        injector.IconPath = "resources\\icon.png";
                    else
                        injector.IconPath = null;
                    injector.Log.WriteLine("A default image will be used as icon.");
                }
            }
            return true;
        }

        private bool ValidOutPath()
        {
            if (injector.OutPath != null)
            {
                if (!Directory.Exists(injector.OutPath))
                {
                    injector.Log.WriteLine("The \"" + injector.OutPath + "\" folder not exist.");
                    return false;
                }
            }
            else
            {
                injector.Log.WriteLine("The output folder was not specified.");
                injector.OutPath = Environment.CurrentDirectory;
                injector.Log.WriteLine("The root folder of 64Inject will be used as the output folder.");
            }

            injector.OutPath += "\\" + injector.ShortNameASCII + " [" + injector.TitleId + "]";

            if (Directory.Exists(injector.OutPath))
                if (Directory.GetDirectories(injector.OutPath).Length != 0 ||
                    Directory.GetFiles(injector.OutPath).Length != 0)
                {
                    injector.Log.WriteLine("The \"" + injector.OutPath + "\" folder exist and not empty.");
                    return false;
                }

            return true;
        }
        
        #endregion
    }
}
