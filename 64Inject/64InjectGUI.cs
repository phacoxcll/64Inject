using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Drawing.Drawing2D;

namespace _64Inject
{
    public partial class _64InjectGUI : Form
    {
        private _64Injector injector;
        
        private XmlDocument XmlConfig;
        private XmlWriterSettings XmlSettings;

        public _64InjectGUI()
        {
            injector = new _64Injector();

            InitializeComponent();

            this.Text = "64Inject " + _64Injector.Release;

            XmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            XmlConfig = new XmlDocument();

            LoadXmlConfig();

            if (NusContent.GetJavaVersion() == null)
                MessageBox.Show("Warning! Apparently the system does not have Java installed!\n" +
                    "Java is needed to NUSPacker (encrypt) and JNUSTool (decrypt).",
                    "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                StringBuilder sb = new StringBuilder();
                bool warning = false;
                if (!File.Exists("resources\\nuspacker\\NUSPacker.jar"))
                {
                    sb.AppendLine("Warning! \"resources\\nuspacker\\NUSPacker.jar\" not found!");
                    sb.AppendLine("NUSPacker allows you to encrypt the result for WUPInstaller.");
                    sb.AppendLine("");
                    warning = true;
                }
                if (!File.Exists("resources\\jnustool\\JNUSTool.jar"))
                {
                    sb.AppendLine("Warning! \"resources\\jnustool\\JNUSTool.jar\" not found!");
                    sb.AppendLine("JNUSTool allows you to decrypt Wii U VC games to use as a base.");
                    warning = true;
                }

                if (warning)
                    MessageBox.Show(sb.ToString(), "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (NusContent.CheckCommonKeyFiles())
            {
                textBoxCommonKey.Enabled = false;
                panelValidKey.BackgroundImage = Properties.Resources.checkmark_16;
            }
            else
            {
                textBoxCommonKey.Enabled = true;
                panelValidKey.BackgroundImage = Properties.Resources.x_mark_16;
            }

            labelLoadedBase.Text = "Base loaded: " + injector.LoadedBase;
            if (injector.BaseIsLoaded)
                panelLoadedBase.BackgroundImage = global::_64Inject.Properties.Resources.checkmark_16;         
   
            if (File.Exists("resources\\boot.png"))
            {
                injector.BootTvImg.Frame = new Bitmap("resources\\boot.png");
                injector.BootDrcImg.Frame = new Bitmap("resources\\boot.png");
            }

            if (File.Exists("resources\\icon.png"))
                injector.IconImg.Frame = new Bitmap("resources\\icon.png");

            if (File.Exists("resources\\title_screen.png"))
            {
                injector.BootTvImg.TitleScreen = new Bitmap("resources\\title_screen.png");
                injector.BootDrcImg.TitleScreen = new Bitmap("resources\\title_screen.png");
                injector.IconImg.TitleScreen = new Bitmap("resources\\title_screen.png");
            }

            UpdateBootTvPictureBox();
            UpdateBootDrcPictureBox();
            UpdateIconPictureBox();
        }

        private void _64InjectGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveXmlConfig();
        }

        #region Config

        private Stream CreateXmlConfig()
        {
            Stream xmlStream = new MemoryStream();
            XmlWriter config = XmlWriter.Create(xmlStream, XmlSettings);

            config.WriteStartElement("Config");

            config.WriteStartElement("ROMDir");
            config.WriteAttributeString("type", "String");
            config.WriteValue("");
            config.WriteEndElement();
            config.WriteStartElement("INIDir");
            config.WriteAttributeString("type", "String");
            config.WriteValue("");
            config.WriteEndElement();
            config.WriteStartElement("ImagesDir");
            config.WriteAttributeString("type", "String");
            config.WriteValue("");
            config.WriteEndElement();

            config.WriteStartElement("ROMDirCheck");
            config.WriteAttributeString("type", "Boolean");
            config.WriteValue("True");
            config.WriteEndElement();
            config.WriteStartElement("INIDirCheck");
            config.WriteAttributeString("type", "Boolean");
            config.WriteValue("True");
            config.WriteEndElement();
            config.WriteStartElement("ImagesDirCheck");
            config.WriteAttributeString("type", "Boolean");
            config.WriteValue("True");
            config.WriteEndElement();

            config.WriteStartElement("BaseFrom");
            config.WriteAttributeString("type", "String");
            config.WriteValue("");
            config.WriteEndElement();
            config.WriteStartElement("BaseAsk");
            config.WriteAttributeString("type", "Boolean");
            config.WriteValue("False");
            config.WriteEndElement();
            config.WriteStartElement("PackUpResult");
            config.WriteAttributeString("type", "Boolean");
            config.WriteValue("False");
            config.WriteEndElement();

            config.WriteEndElement();

            config.Close();
            xmlStream.Position = 0;

            return xmlStream;
        }

        private void LoadXmlConfig()
        {
            if (File.Exists("resources\\gui_config"))
            {
                try
                {
                    XmlConfig.Load("resources\\gui_config");

                    XmlNode nodeROMDir = XmlConfig.SelectSingleNode("Config/ROMDir");
                    XmlNode nodeINIDir = XmlConfig.SelectSingleNode("Config/INIDir");
                    XmlNode nodeImagesDir = XmlConfig.SelectSingleNode("Config/ImagesDir");

                    XmlNode nodeROMDirCheck = XmlConfig.SelectSingleNode("Config/ROMDirCheck");
                    XmlNode nodeINIDirCheck = XmlConfig.SelectSingleNode("Config/INIDirCheck");
                    XmlNode nodeImagesDirCheck = XmlConfig.SelectSingleNode("Config/ImagesDirCheck");

                    XmlNode nodeBaseFrom = XmlConfig.SelectSingleNode("Config/BaseFrom");
                    XmlNode nodeBaseAsk = XmlConfig.SelectSingleNode("Config/BaseAsk");
                    XmlNode nodePackUpResult = XmlConfig.SelectSingleNode("Config/PackUpResult");

                    textBoxRomDir.Text = nodeROMDir.InnerText;
                    textBoxIniDir.Text = nodeINIDir.InnerText;
                    textBoxImagesDir.Text = nodeImagesDir.InnerText;

                    checkBoxRomDir.Checked = Convert.ToBoolean(nodeROMDirCheck.InnerText);
                    checkBoxIniDir.Checked = Convert.ToBoolean(nodeINIDirCheck.InnerText);
                    checkBoxImagesDir.Checked = Convert.ToBoolean(nodeImagesDirCheck.InnerText);

                    textBoxBaseFrom.Text = nodeBaseFrom.InnerText;
                    checkBoxAskBase.Checked = Convert.ToBoolean(nodeBaseAsk.InnerText);
                    checkBoxPackUpResult.Checked = Convert.ToBoolean(nodePackUpResult.InnerText);
                }
                catch
                {
                    XmlConfig.Load(CreateXmlConfig());
                }
            }
            else
            {
                XmlConfig.Load(CreateXmlConfig());
            }
        }

        private void SaveXmlConfig()
        {
            if (!Directory.Exists("resources"))
                Directory.CreateDirectory("resources");

            XmlNode nodeROMDir = XmlConfig.SelectSingleNode("Config/ROMDir");
            XmlNode nodeINIDir = XmlConfig.SelectSingleNode("Config/INIDir");
            XmlNode nodeImagesDir = XmlConfig.SelectSingleNode("Config/ImagesDir");

            XmlNode nodeROMDirCheck = XmlConfig.SelectSingleNode("Config/ROMDirCheck");
            XmlNode nodeINIDirCheck = XmlConfig.SelectSingleNode("Config/INIDirCheck");
            XmlNode nodeImagesDirCheck = XmlConfig.SelectSingleNode("Config/ImagesDirCheck");

            XmlNode nodeBaseFrom = XmlConfig.SelectSingleNode("Config/BaseFrom");
            XmlNode nodeBaseAsk = XmlConfig.SelectSingleNode("Config/BaseAsk");
            XmlNode nodePackUpResult = XmlConfig.SelectSingleNode("Config/PackUpResult");

            nodeROMDir.InnerText = textBoxRomDir.Text;
            nodeINIDir.InnerText = textBoxIniDir.Text;
            nodeImagesDir.InnerText = textBoxImagesDir.Text;

            nodeROMDirCheck.InnerText = checkBoxRomDir.Checked.ToString();
            nodeINIDirCheck.InnerText = checkBoxIniDir.Checked.ToString();
            nodeImagesDirCheck.InnerText = checkBoxImagesDir.Checked.ToString();

            nodeBaseFrom.InnerText = textBoxBaseFrom.Text;
            nodeBaseAsk.InnerText = checkBoxAskBase.Checked.ToString();
            nodePackUpResult.InnerText = checkBoxPackUpResult.Checked.ToString();

            XmlWriter config = XmlWriter.Create("resources\\gui_config", XmlSettings);
            XmlConfig.Save(config);
            config.Close();
        }

        #endregion

        #region Main tab

        private void buttonRom_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.Filter = "N64 ROM|*.z64;*.n64;*.v64;*.u64|All files|*.*";
            if (checkBoxRomDir.Checked)
                openFileDialog.InitialDirectory = textBoxRomDir.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                
                injector.RomPath = openFileDialog.FileName;
                textBoxRom.Text = injector.RomPath;

                if (injector.Rom != null)
                    injector.Rom.Dispose();

                injector.Rom = new RomN64(injector.RomPath);

                this.Text = "64Inject " + _64Injector.Release + " :: " + injector.Rom.Name;
                labelProductCode.Text = "Product code: " + injector.Rom.ProductCode +
                    (injector.Rom.Version != 0 ? " (Rev " + injector.Rom.Revision + ")" : "");

                if ((injector.BaseIsLoaded || checkBoxAskBase.Checked) &&
                    injector.Rom.IsValid &&
                    (injector.IniIsLoaded || injector.IniPath == null))
                {
                    labelTitleId.Text = "Title ID: " + injector.TitleId;

                    if (textBoxShortName.Text.Length > 0)
                        buttonInject.Enabled = true;
                    else
                        buttonInject.Enabled = false;
                }
                else
                {
                    labelTitleId.Text = "Title ID:";
                    buttonInject.Enabled = false;
                }
            }
        }

        private void buttonIni_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.Filter = "INI file|*.ini|All files|*.*";
            if (checkBoxIniDir.Checked)
                openFileDialog.InitialDirectory = textBoxIniDir.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                injector.IniPath = openFileDialog.FileName;
                textBoxIni.Text = injector.IniPath;

                if (!injector.LoadIni(injector.IniPath))
                {
                    buttonInject.Enabled = false;
                    labelTitleId.Text = "Title ID:";
                    MessageBox.Show("The INI file is not valid.",
                        "64Inject", MessageBoxButtons.OK, MessageBoxIcon.Error);                    
                }
                else if ((injector.BaseIsLoaded || checkBoxAskBase.Checked) &&
                    injector.RomIsLoaded)
                {
                    labelTitleId.Text = "Title ID: " + injector.TitleId;
                    if (textBoxShortName.Text.Length > 0)
                        buttonInject.Enabled = true;
                    else
                        buttonInject.Enabled = false;
                }
            }
        }
        
        private void textBoxShortName_TextChanged(object sender, EventArgs e)
        {
            if ((injector.BaseIsLoaded || checkBoxAskBase.Checked) &&
                injector.RomIsLoaded &&
                (injector.IniIsLoaded || injector.IniPath == null) &&
                textBoxShortName.Text.Length > 0)
                buttonInject.Enabled = true;
            else
                buttonInject.Enabled = false;

            UpdateBootName();
            UpdateBootTvPictureBox();
            UpdateBootDrcPictureBox();
        }

        private void checkBoxLongName_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLongName.Checked)
            {
                textBoxLNLine1.Enabled = true;
                textBoxLNLine2.Enabled = true;
                injector.BootTvImg.Longname = true;
                injector.BootDrcImg.Longname = true;
            }
            else
            {
                textBoxLNLine1.Text = "";
                textBoxLNLine2.Text = "";
                textBoxLNLine1.Enabled = false;
                textBoxLNLine2.Enabled = false;
                injector.BootTvImg.Longname = false;
                injector.BootDrcImg.Longname = false;
            }

            UpdateBootName();
            UpdateBootTvPictureBox();
            UpdateBootDrcPictureBox();
        }

        private void textBoxLNLine1_TextChanged(object sender, EventArgs e)
        {
            UpdateBootName();
            UpdateBootTvPictureBox();
            UpdateBootDrcPictureBox();
        }

        private void textBoxLNLine2_TextChanged(object sender, EventArgs e)
        {
            UpdateBootName();
            UpdateBootTvPictureBox();
            UpdateBootDrcPictureBox();
        }

        private void checkBoxDarkFilter_CheckedChanged(object sender, EventArgs e)
        {
            injector.DarkFilter = checkBoxDarkFilter.Checked;

            if ((injector.BaseIsLoaded || checkBoxAskBase.Checked) &&
                    injector.RomIsLoaded &&
                    (injector.IniIsLoaded || injector.IniPath == null))
            {
                labelTitleId.Text = "Title ID: " + injector.TitleId;
            }
        }

        private void checkBoxWidescreen_CheckedChanged(object sender, EventArgs e)
        {
            injector.Widescreen = checkBoxWidescreen.Checked;

            if ((injector.BaseIsLoaded || checkBoxAskBase.Checked) &&
                    injector.RomIsLoaded &&
                    (injector.IniIsLoaded || injector.IniPath == null))
            {
                labelTitleId.Text = "Title ID: " + injector.TitleId;
            }
        }

        private void numericUpDownZoom_ValueChanged(object sender, EventArgs e)
        {
            injector.Scale = (float)(numericUpDownZoom.Value / 100.0M);

            if ((injector.BaseIsLoaded || checkBoxAskBase.Checked) &&
                    injector.RomIsLoaded &&
                    (injector.IniIsLoaded || injector.IniPath == null))
            {
                labelTitleId.Text = "Title ID: " + injector.TitleId;
            }
        }

        private void buttonBootTv_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.Filter = "Image file|*.png;*.jpg;*.bmp";
            if (checkBoxImagesDir.Checked)
                openFileDialog.InitialDirectory = textBoxImagesDir.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                injector.BootTvImg.Frame = new Bitmap(openFileDialog.FileName);
                injector.BootTvImg.IsDefault = false;
                UpdateBootTvPictureBox();

                if ((injector.BaseIsLoaded || checkBoxAskBase.Checked) &&
                    injector.RomIsLoaded &&
                    (injector.IniIsLoaded || injector.IniPath == null))
                {
                    labelTitleId.Text = "Title ID: " + injector.TitleId;
                }
            }
        }

        private void buttonBootDrc_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.Filter = "Image file|*.png;*.jpg;*.bmp";
            if (checkBoxImagesDir.Checked)
                openFileDialog.InitialDirectory = textBoxImagesDir.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                injector.BootDrcImg.Frame = new Bitmap(openFileDialog.FileName);
                injector.BootDrcImg.IsDefault = false;
                UpdateBootDrcPictureBox();

                if ((injector.BaseIsLoaded || checkBoxAskBase.Checked) &&
                    injector.RomIsLoaded &&
                    (injector.IniIsLoaded || injector.IniPath == null))
                {
                    labelTitleId.Text = "Title ID: " + injector.TitleId;
                }
            }
        }

        private void buttonIcon_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.Filter = "Image file|*.png;*.jpg;*.bmp";
            if (checkBoxImagesDir.Checked)
                openFileDialog.InitialDirectory = textBoxImagesDir.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                injector.IconImg.Frame = new Bitmap(openFileDialog.FileName);
                injector.IconImg.IsDefault = false;
                UpdateIconPictureBox();

                if ((injector.BaseIsLoaded || checkBoxAskBase.Checked) &&
                    injector.RomIsLoaded &&
                    (injector.IniIsLoaded || injector.IniPath == null))
                {
                    labelTitleId.Text = "Title ID: " + injector.TitleId;
                }
            }
        }

        private void comboBoxReleased_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxReleased.SelectedItem.ToString())
            {
                case "1996":
                    injector.BootTvImg.Released = 1996;
                    injector.BootDrcImg.Released = 1996;
                    break;
                case "1997":
                    injector.BootTvImg.Released = 1997;
                    injector.BootDrcImg.Released = 1997;
                    break;
                case "1998":
                    injector.BootTvImg.Released = 1998;
                    injector.BootDrcImg.Released = 1998;
                    break;
                case "1999":
                    injector.BootTvImg.Released = 1999;
                    injector.BootDrcImg.Released = 1999;
                    break;
                case "2000":
                    injector.BootTvImg.Released = 2000;
                    injector.BootDrcImg.Released = 2000;
                    break;
                case "2001":
                    injector.BootTvImg.Released = 2001;
                    injector.BootDrcImg.Released = 2001;
                    break;
                case "2002":
                    injector.BootTvImg.Released = 2002;
                    injector.BootDrcImg.Released = 2002;
                    break;
                case "2018":
                    injector.BootTvImg.Released = 2018;
                    injector.BootDrcImg.Released = 2018;
                    break;
                default:
                    injector.BootTvImg.Released = 0;
                    injector.BootDrcImg.Released = 0;
                    break;
            }
            UpdateBootTvPictureBox();
            UpdateBootDrcPictureBox();
        }

        private void comboBoxPlayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxPlayers.SelectedItem.ToString())
            {
                case "1":
                    injector.BootTvImg.Players = 1;
                    injector.BootDrcImg.Players = 1;
                    break;
                case "2":
                    injector.BootTvImg.Players = 2;
                    injector.BootDrcImg.Players = 2;
                    break;
                case "3":
                    injector.BootTvImg.Players = 3;
                    injector.BootDrcImg.Players = 3;
                    break;
                case "4":
                    injector.BootTvImg.Players = 4;
                    injector.BootDrcImg.Players = 4;
                    break;
                default:
                    injector.BootTvImg.Players = 0;
                    injector.BootDrcImg.Players = 0;
                    break;
            }
            UpdateBootTvPictureBox();
            UpdateBootDrcPictureBox();
        }

        private void checkBoxShowName_CheckedChanged(object sender, EventArgs e)
        {
            UpdateBootName();
            UpdateBootTvPictureBox();
            UpdateBootDrcPictureBox();
        }

        private void buttonTitleScreen_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.Filter = "Image file|*.png;*.jpg;*.bmp";
            if (checkBoxImagesDir.Checked)
                openFileDialog.InitialDirectory = textBoxImagesDir.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                injector.BootTvImg.TitleScreen = new Bitmap(openFileDialog.FileName);
                injector.BootDrcImg.TitleScreen = new Bitmap(openFileDialog.FileName);
                injector.IconImg.TitleScreen = new Bitmap(openFileDialog.FileName);

                injector.BootTvImg.IsDefault = false;
                injector.BootDrcImg.IsDefault = false;
                injector.IconImg.IsDefault = false;

                UpdateBootTvPictureBox();
                UpdateBootDrcPictureBox();
                UpdateIconPictureBox();

                if ((injector.BaseIsLoaded || checkBoxAskBase.Checked) &&
                    injector.RomIsLoaded &&
                    (injector.IniIsLoaded || injector.IniPath == null))
                {
                    labelTitleId.Text = "Title ID: " + injector.TitleId;
                }
            }
        }

        private void buttonInject_Click(object sender, EventArgs e)
        {
            bool _continue = true;

            if (textBoxShortName.Text.Length != 0)
                injector.ShortName = textBoxShortName.Text;
            else
            {
                _continue = false;
                MessageBox.Show("The name of the game is empty.",
                    "Short name", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (_continue)
            {
                if (checkBoxLongName.Checked)
                {
                    if (textBoxLNLine1.Text.Length != 0 && textBoxLNLine2.Text.Length != 0)
                        injector.LongName = textBoxLNLine1.Text + "\n" + textBoxLNLine2.Text;
                    else
                    {
                        _continue = false;
                        MessageBox.Show("The long name of the game is empty.",
                            "Long name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }                    
                }
                else
                    injector.LongName = injector.ShortName;
            }

            if (_continue && checkBoxAskBase.Checked)
            {
                _continue = AskBase();
                if (_continue)
                {
                    labelTitleId.Text = "Title ID: " + injector.TitleId;
                    this.Update();
                }
                else
                {
                    labelTitleId.Text = "Title ID:";
                    MessageBox.Show("The base was not loaded correctly.",
                        "64Inject", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            folderBrowserDialog.Description = "Select the folder where the result will be saved.";
            folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            if (_continue && folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                injector.Encrypt = checkBoxPackUpResult.Checked;
                injector.OutPath = folderBrowserDialog.SelectedPath + "\\" + injector.ShortNameASCII + " [" + injector.TitleId + "]";

                if (Directory.Exists(injector.OutPath))
                    if (Directory.GetDirectories(injector.OutPath).Length != 0 ||
                        Directory.GetFiles(injector.OutPath).Length != 0)
                {
                    _continue = false;
                    MessageBox.Show("The \"" + injector.OutPath + "\" folder exist and not empty.",
                        "64Inject", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (_continue)
                {
                    if (injector.Inject())
                        MessageBox.Show("Injection completed successfully!",
                            "64Inject", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("The injection failed.",
                            "64Inject", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateBootName()
        {
            if (checkBoxShowName.Checked)
            {
                if (checkBoxLongName.Checked)
                {
                    injector.BootTvImg.NameLine1 = textBoxLNLine1.Text;
                    injector.BootTvImg.NameLine2 = textBoxLNLine2.Text;
                    injector.BootDrcImg.NameLine1 = textBoxLNLine1.Text;
                    injector.BootDrcImg.NameLine2 = textBoxLNLine2.Text;
                }
                else
                {
                    injector.BootTvImg.NameLine1 = textBoxShortName.Text;
                    injector.BootTvImg.NameLine2 = "";
                    injector.BootDrcImg.NameLine1 = textBoxShortName.Text;
                    injector.BootDrcImg.NameLine2 = "";
                }
            }
            else
            {
                injector.BootTvImg.NameLine1 = "";
                injector.BootTvImg.NameLine2 = "";
                injector.BootDrcImg.NameLine1 = "";
                injector.BootDrcImg.NameLine2 = "";
            }
        }

        private void UpdateBootTvPictureBox()
        {
            if (pictureBoxBootTv.Image != null)
                pictureBoxBootTv.Image.Dispose();

            pictureBoxBootTv.Image = injector.BootTvImg.Create();
        }

        private void UpdateBootDrcPictureBox()
        {
            if (pictureBoxBootDrc.Image != null)
                pictureBoxBootDrc.Image.Dispose();

            pictureBoxBootDrc.Image = injector.BootDrcImg.Create();
        }

        private void UpdateIconPictureBox()
        {
            if (pictureBoxIcon.Image != null)
                pictureBoxIcon.Image.Dispose();

            pictureBoxIcon.Image = injector.IconImg.Create();
        }

        #endregion

        #region Config tab

        private void buttonRomDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Select the folder where your N64 ROM collection is located.";

            if (Directory.Exists(textBoxRomDir.Text))
                folderBrowserDialog.SelectedPath = textBoxRomDir.Text;
            else
                folderBrowserDialog.SelectedPath =
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxRomDir.Text = folderBrowserDialog.SelectedPath;
        }
        
        private void buttonIniDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Select the folder where your INI file collection is located.";

            if (Directory.Exists(textBoxIniDir.Text))
                folderBrowserDialog.SelectedPath = textBoxIniDir.Text;
            else
                folderBrowserDialog.SelectedPath =
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxIniDir.Text = folderBrowserDialog.SelectedPath;
        }

        private void buttonImagesDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Select the folder where your image collection is located.";

            if (Directory.Exists(textBoxImagesDir.Text))
                folderBrowserDialog.SelectedPath = textBoxImagesDir.Text;
            else
                folderBrowserDialog.SelectedPath =
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxImagesDir.Text = folderBrowserDialog.SelectedPath;
        }

        private void textBoxCommonKey_TextChanged(object sender, EventArgs e)
        {
            if (NusContent.LoadKey(textBoxCommonKey.Text))
            {
                textBoxCommonKey.Text = "";
                textBoxCommonKey.Enabled = false;
                panelValidKey.BackgroundImage = Properties.Resources.checkmark_16;
            }
            else
            {
                textBoxCommonKey.Enabled = true;
                panelValidKey.BackgroundImage = Properties.Resources.x_mark_16;
            }
        }

        private void buttonBaseFrom_Click(object sender, EventArgs e)
        {
            if ((AskBase() || injector.BaseIsLoaded || checkBoxAskBase.Checked) &&
                injector.RomIsLoaded &&
                (injector.IniIsLoaded || injector.IniPath == null))
            {
                labelTitleId.Text = "Title ID: " + injector.TitleId;

                if (textBoxShortName.Text.Length > 0)
                    buttonInject.Enabled = true;
                else
                    buttonInject.Enabled = false;
            }
            else
            {
                labelTitleId.Text = "Title ID:";
                buttonInject.Enabled = false;
            }
        }
        
        private void checkBoxAskBase_CheckedChanged(object sender, EventArgs e)
        {
            if (injector.RomIsLoaded &&
                (injector.BaseIsLoaded || checkBoxAskBase.Checked))
            {
                labelTitleId.Text = "Title ID: " + injector.TitleId;
                if (textBoxShortName.Text.Length > 0)
                    buttonInject.Enabled = true;
                else
                    buttonInject.Enabled = false;
            }
            else
            {
                labelTitleId.Text = "Title ID:";
                buttonInject.Enabled = false;
            }
        }

        private bool AskBase()
        {
            folderBrowserDialog.Description = "Select the folder of the N64 VC game you want to use as a base.";

            if (Directory.Exists(textBoxBaseFrom.Text))
                folderBrowserDialog.SelectedPath = textBoxBaseFrom.Text;
            else
                folderBrowserDialog.SelectedPath =
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop); ;

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                labelLoadedBase.Text = "Base loaded: Wait...";
                panelLoadedBase.BackgroundImage = global::_64Inject.Properties.Resources.x_mark_16;
                this.Update();

                textBoxBaseFrom.Text = folderBrowserDialog.SelectedPath;

                if (injector.LoadBase(textBoxBaseFrom.Text))
                {
                    labelLoadedBase.Text = "Base loaded: " + injector.LoadedBase;
                    panelLoadedBase.BackgroundImage = global::_64Inject.Properties.Resources.checkmark_16;
                    return true;
                }
                else
                {
                    labelLoadedBase.Text = "The base is not valid.";
                    panelLoadedBase.BackgroundImage = global::_64Inject.Properties.Resources.x_mark_16;
                }
            }
            return false;
        }

        #endregion
    }
}
