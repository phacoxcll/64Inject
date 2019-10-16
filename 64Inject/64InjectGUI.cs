using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace _64Inject
{
    public partial class _64InjectGUI : Form
    {
        private _64Injector injector;

        private string BasePath;
        private string ResultPath;

        public _64InjectGUI()
        {
            Cll.Log.SaveIn("64Inject.log");
            Cll.Log.WriteLine(DateTime.Now.ToString());
            Cll.Log.WriteLine("64Inject open in GUI mode.");

            injector = new _64Injector();

            InitializeComponent();

            LoadSettings();
            this.Text = "64Inject " + _64Injector.Release;
            buttonMain.BackColor = Color.FromArgb(16, 110, 190);
            groupBoxHelp.Text = HelpString.Packing;
            labelHelpText.Text = HelpString.Enable_Packing;

            if (NusContent.GetJavaVersion() == null)
                MessageBox.Show(HelpString.Java_Warning,
                    HelpString.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                StringBuilder sb = new StringBuilder();
                bool warning = false;
                if (!File.Exists("resources\\nuspacker\\NUSPacker.jar"))
                {
                    sb.AppendLine(HelpString.NUSPacker_Warning);
                    sb.AppendLine("");
                    warning = true;
                }
                if (!File.Exists("resources\\jnustool\\JNUSTool.jar"))
                {
                    sb.AppendLine(HelpString.JNUSTool_Warning);
                    warning = true;
                }

                if (warning)
                    MessageBox.Show(sb.ToString(), HelpString.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (NusContent.CheckCommonKeyFiles())
            {
                textBoxCommonKey.BackColor = Color.FromArgb(33, 33, 33);
                textBoxCommonKey.Enabled = false;
                panelValidKey.BackgroundImage = Properties.Resources.checkmark_16;
            }
            else
            {
                textBoxCommonKey.BackColor = Color.FromArgb(51, 51, 51);
                textBoxCommonKey.Enabled = true;
                panelValidKey.BackgroundImage = Properties.Resources.x_mark_16;
            }

            labelLoadedBase.Text = "Base: " + injector.LoadedBase;
            if (injector.BaseIsLoaded)
                panelLoadedBase.BackgroundImage = Properties.Resources.checkmark_16;

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
            SaveSettings();
            Cll.Log.WriteLine("Close.");
            Cll.Log.WriteLine("----------------------------------------------------------------");
        }

        private void LoadLogFile()
        {
            try
            {
                textBoxLog.Clear();
                StreamReader sr = File.OpenText(Cll.Log.Filename);
                textBoxLog.AppendText(sr.ReadToEnd());
                sr.Close();
                //this.Update();
            }
            catch
            {
                Cll.Log.WriteLine("Error reading log file.");
            }
        }

        private void LoadSettings()
        {
            BasePath = Properties.Settings.Default.BasePath;
            ResultPath = Properties.Settings.Default.ResultPath;

            switch (Properties.Settings.Default.Language)
            {
                case "en-US":
                    comboBoxLanguage.SelectedIndex = 0;
                    break;
                case "es-MX":
                    comboBoxLanguage.SelectedIndex = 1;
                    break;
                default:
                    switch (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName)
                    {
                        case "en":
                            comboBoxLanguage.SelectedIndex = 0;
                            break;
                        case "es":
                            comboBoxLanguage.SelectedIndex = 1;
                            break;
                        default:
                            comboBoxLanguage.SelectedIndex = 0;
                            break;
                    }
                    break;
            }
            checkBoxHelp.Checked = Properties.Settings.Default.Help;

            textBoxRomPath.Text = Properties.Settings.Default.ROMPath;
            textBoxImagesPath.Text = Properties.Settings.Default.ImagePath;
            textBoxConfigFilesPath.Text = Properties.Settings.Default.ConfigFilePath;

            checkBoxRomPath.Checked = Properties.Settings.Default.ROMCheck;
            checkBoxImagesPath.Checked = Properties.Settings.Default.ImageCheck;
            checkBoxConfigFilesPath.Checked = Properties.Settings.Default.ConfigFileCheck;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.BasePath = BasePath;
            Properties.Settings.Default.ResultPath = ResultPath;

            switch (comboBoxLanguage.SelectedIndex)
            {
                case 0:
                    Properties.Settings.Default.Language = "en-US";
                    break;
                case 1:
                    Properties.Settings.Default.Language = "es-MX";
                    break;
                default:
                    Properties.Settings.Default.Language = "en-US";
                    break;
            }
            Properties.Settings.Default.Help = checkBoxHelp.Checked;

            Properties.Settings.Default.ROMPath = textBoxRomPath.Text;
            Properties.Settings.Default.ImagePath = textBoxImagesPath.Text;
            Properties.Settings.Default.ConfigFilePath = textBoxConfigFilesPath.Text;

            Properties.Settings.Default.ROMCheck = checkBoxRomPath.Checked;
            Properties.Settings.Default.ImageCheck = checkBoxImagesPath.Checked;
            Properties.Settings.Default.ConfigFileCheck = checkBoxConfigFilesPath.Checked;

            Properties.Settings.Default.Save();
        }

        private void ButtonMain_Click(object sender, EventArgs e)
        {
            buttonMain.BackColor = Color.FromArgb(16, 110, 190);
            buttonImages.BackColor = Color.FromArgb(17, 17, 17);
            buttonPacking.BackColor = Color.FromArgb(17, 17, 17);
            buttonSettings.BackColor = Color.FromArgb(17, 17, 17);
            panelMain.Visible = true;
            panelImages.Visible = false;
            panelPacking.Visible = false;
            panelSettings.Visible = false;
        }

        private void ButtonImages_Click(object sender, EventArgs e)
        {
            UpdateBootName();
            UpdateBootTvPictureBox();
            UpdateBootDrcPictureBox();

            buttonMain.BackColor = Color.FromArgb(17, 17, 17);
            buttonImages.BackColor = Color.FromArgb(16, 110, 190);
            buttonPacking.BackColor = Color.FromArgb(17, 17, 17);
            buttonSettings.BackColor = Color.FromArgb(17, 17, 17);
            panelMain.Visible = false;
            panelImages.Visible = true;
            panelPacking.Visible = false;
            panelSettings.Visible = false;
        }

        private void ButtonPacking_Click(object sender, EventArgs e)
        {
            LoadLogFile();

            buttonMain.BackColor = Color.FromArgb(17, 17, 17);
            buttonImages.BackColor = Color.FromArgb(17, 17, 17);
            buttonPacking.BackColor = Color.FromArgb(16, 110, 190);
            buttonSettings.BackColor = Color.FromArgb(17, 17, 17);
            panelMain.Visible = false;
            panelImages.Visible = false;
            panelPacking.Visible = true;
            panelSettings.Visible = false;
        }

        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            buttonMain.BackColor = Color.FromArgb(17, 17, 17);
            buttonImages.BackColor = Color.FromArgb(17, 17, 17);
            buttonPacking.BackColor = Color.FromArgb(17, 17, 17);
            buttonSettings.BackColor = Color.FromArgb(16, 110, 190);
            panelMain.Visible = false;
            panelImages.Visible = false;
            panelPacking.Visible = false;
            panelSettings.Visible = true;
        }

        #region Main

        private void buttonRom_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.FilterIndex = 0;
            openFileDialog.Filter = "N64 ROM|*.z64;*.n64;*.v64;*.u64|All files|*.*";
            if (checkBoxRomPath.Checked && Directory.Exists(textBoxRomPath.Text))
                openFileDialog.InitialDirectory = textBoxRomPath.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                injector.RomPath = openFileDialog.FileName;
                textBoxRom.Text = Path.GetFileName(injector.RomPath);

                injector.Rom = new RomN64(injector.RomPath);

                if (injector.Rom.IsValid)
                {
                    this.Text = "64Inject " + _64Injector.Release + " :: " + injector.Rom.Title;
                    labelProductCode.Text = "Product code: " + injector.Rom.ProductCode +
                        (injector.Rom.Version != 0 ? " (Rev " + injector.Rom.Revision + ")" : "");
                }
                else
                {
                    this.Text = "64Inject " + _64Injector.Release;
                    labelProductCode.Text = "Product code:";
                }

                if (injector.BaseIsLoaded && injector.Rom.IsValid)
                {
                    labelTitleId.Text = "Title ID: " + injector.TitleId;

                    if (textBoxShortName.Text.Length > 0)
                    {
                        buttonInjectPack.Enabled = true;
                        buttonInjectNotPack.Enabled = true;
                        panelPackingQuestion.Visible = false;
                    }
                    else
                    {
                        buttonInjectPack.Enabled = false;
                        buttonInjectNotPack.Enabled = false;
                        panelPackingQuestion.Visible = true;
                    }
                }
                else
                {
                    labelTitleId.Text = "Title ID:";
                    buttonInjectPack.Enabled = false;
                    buttonInjectNotPack.Enabled = false;
                    panelPackingQuestion.Visible = true;
                }
            }
        }

        private void textBoxShortName_TextChanged(object sender, EventArgs e)
        {
            if (injector.BaseIsLoaded && injector.RomIsLoaded &&
                textBoxShortName.Text.Length > 0)
            {
                buttonInjectPack.Enabled = true;
                buttonInjectNotPack.Enabled = true;
                panelPackingQuestion.Visible = false;
            }
            else
            {
                buttonInjectPack.Enabled = false;
                buttonInjectNotPack.Enabled = false;
                panelPackingQuestion.Visible = true;
            }

            //UpdateBootName();
            //UpdateBootTvPictureBox();
            //UpdateBootDrcPictureBox();
        }

        private void checkBoxLongName_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLongName.Checked)
            {
                textBoxLNLine1.BackColor = Color.FromArgb(51, 51, 51);
                textBoxLNLine2.BackColor = Color.FromArgb(51, 51, 51);
                textBoxLNLine1.Enabled = true;
                textBoxLNLine2.Enabled = true;
                injector.BootTvImg.Longname = true;
                injector.BootDrcImg.Longname = true;
            }
            else
            {
                textBoxLNLine1.Text = "";
                textBoxLNLine2.Text = "";
                textBoxLNLine1.BackColor = Color.FromArgb(33, 33, 33);
                textBoxLNLine2.BackColor = Color.FromArgb(33, 33, 33);
                textBoxLNLine1.Enabled = false;
                textBoxLNLine2.Enabled = false;
                injector.BootTvImg.Longname = false;
                injector.BootDrcImg.Longname = false;
            }

            //UpdateBootName();
            //UpdateBootTvPictureBox();
            //UpdateBootDrcPictureBox();
        }

        private void textBoxLNLine1_TextChanged(object sender, EventArgs e)
        {
            //UpdateBootName();
            //UpdateBootTvPictureBox();
            //UpdateBootDrcPictureBox();
        }

        private void textBoxLNLine2_TextChanged(object sender, EventArgs e)
        {
            //UpdateBootName();
            //UpdateBootTvPictureBox();
            //UpdateBootDrcPictureBox();
        }

        private void buttonLoadBase_Click(object sender, EventArgs e)
        {
            if ((AskBase() || injector.BaseIsLoaded) && injector.RomIsLoaded)
            {
                labelTitleId.Text = "Title ID: " + injector.TitleId;

                if (textBoxShortName.Text.Length > 0)
                {
                    buttonInjectPack.Enabled = true;
                    buttonInjectNotPack.Enabled = true;
                    panelPackingQuestion.Visible = false;
                }
                else
                {
                    buttonInjectPack.Enabled = false;
                    buttonInjectNotPack.Enabled = false;
                    panelPackingQuestion.Visible = true;
                }
            }
            else
            {
                labelTitleId.Text = "Title ID:";
                buttonInjectPack.Enabled = false;
                buttonInjectNotPack.Enabled = false;
                panelPackingQuestion.Visible = true;
            }
        }

        private bool AskBase()
        {
            folderBrowserDialog.Description = HelpString.Base_Select;

            if (Directory.Exists(BasePath))
                folderBrowserDialog.SelectedPath = BasePath;
            else
                folderBrowserDialog.SelectedPath = "";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                labelLoadedBase.Text = "Base: " + HelpString.Loading;
                panelLoadedBase.BackgroundImage = Properties.Resources.x_mark_16;
                this.Update();

                BasePath = folderBrowserDialog.SelectedPath;

                Cll.Log.WriteLine("Loading base.");
                if (injector.LoadBase(BasePath))
                {
                    labelLoadedBase.Text = "Base: " + injector.LoadedBase;
                    panelLoadedBase.BackgroundImage = Properties.Resources.checkmark_16;
                    Cll.Log.WriteLine("Base: " + injector.LoadedBase);
                    return true;
                }
                else
                {
                    labelLoadedBase.Text = HelpString.Base_Invalid;
                    panelLoadedBase.BackgroundImage = Properties.Resources.x_mark_16;
                }
            }
            return false;
        }

        private void checkBoxDarkFilter_CheckedChanged(object sender, EventArgs e)
        {
            injector.DarkFilter = checkBoxDarkFilter.Checked;

            if (injector.BaseIsLoaded &&
                injector.RomIsLoaded &&
                (injector.IniIsLoaded || injector.IniPath == null))
            {
                labelTitleId.Text = "Title ID: " + injector.TitleId;
            }
        }

        private void checkBoxWidescreen_CheckedChanged(object sender, EventArgs e)
        {
            injector.Widescreen = checkBoxWidescreen.Checked;

            if (injector.BaseIsLoaded &&
                injector.RomIsLoaded &&
                (injector.IniIsLoaded || injector.IniPath == null))
            {
                labelTitleId.Text = "Title ID: " + injector.TitleId;
            }
        }

        private void numericUpDownZoom_ValueChanged(object sender, EventArgs e)
        {
            injector.Scale = (float)(numericUpDownZoom.Value / 100.0M);

            if (injector.BaseIsLoaded &&
                injector.RomIsLoaded &&
                (injector.IniIsLoaded || injector.IniPath == null))
            {
                labelTitleId.Text = "Title ID: " + injector.TitleId;
            }
        }

        private void buttonConfigFile_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.FilterIndex = 0;
            openFileDialog.Filter = "INI file|*.ini|TXT file|*.txt|All files|*.*";
            if (checkBoxConfigFilesPath.Checked && Directory.Exists(textBoxConfigFilesPath.Text))
                openFileDialog.InitialDirectory = textBoxConfigFilesPath.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                injector.IniPath = openFileDialog.FileName;

                injector.Ini = new VCN64ConfigFile(injector.IniPath);

                if (injector.Ini.IsValid)
                    textBoxConfigFile.Text = Path.GetFileName(injector.IniPath);
                else
                    MessageBox.Show(HelpString.Config_File_Invalid,
                        "64Inject", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (injector.BaseIsLoaded && injector.RomIsLoaded)
                    labelTitleId.Text = "Title ID: " + injector.TitleId;
            }
        }

        private void buttonEditConfigFile_Click(object sender, EventArgs e)
        {
            try
            {
                StartVCN64ConfigEditor();
            }
            catch
            {
                Cll.Log.WriteLine("Error \"VCN64Config.exe\" program not found.");
                MessageBox.Show("\"VCN64Config.exe\" program not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartVCN64ConfigEditor()
        {
            if (!Directory.Exists("resources\\vcn64configs"))
                Directory.CreateDirectory("resources\\vcn64configs");

            string input = "";
            if (injector.IniIsLoaded)
                input = injector.IniPath;

            StringBuilder output = new StringBuilder("resources\\vcn64configs\\");
            if (injector.RomIsLoaded)
            {
                output.Append(injector.Rom.ProductCodeVersion);
                output.Append(" (" + injector.Rom.Title + ")");
            }
            else
                output.Append("TempConfigFile");
            output.Append(".ini");

            StringBuilder desc = new StringBuilder();
            if (textBoxShortName.Text.Length > 0)
                desc.Append(textBoxShortName.Text);
            if (desc.Length > 0)
                desc.Append(" ");
            if (injector.RomIsLoaded)
                desc.Append(injector.Rom.ProductCodeVersion);

            VCN64Config.FormEditor editor = new VCN64Config.FormEditor();
            if (editor.ShowDialog(input, output.ToString(), desc.ToString()) == DialogResult.OK)
            {
                injector.IniPath = output.ToString();

                injector.Ini = new VCN64ConfigFile(injector.IniPath);

                if (injector.Ini.IsValid)
                    textBoxConfigFile.Text = Path.GetFileName(injector.IniPath);
                else
                    MessageBox.Show(HelpString.Config_File_Invalid,
                        "64Inject", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (injector.BaseIsLoaded && injector.RomIsLoaded)
                    labelTitleId.Text = "Title ID: " + injector.TitleId;
            }
            editor.Dispose();
        }

        #endregion

        #region Images

        private void buttonBootTv_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.FilterIndex = 0;
            openFileDialog.Filter = "Image file|*.png;*.jpg;*.bmp";
            if (checkBoxImagesPath.Checked && Directory.Exists(textBoxImagesPath.Text))
                openFileDialog.InitialDirectory = textBoxImagesPath.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                injector.BootTvImg.Frame = new Bitmap(openFileDialog.FileName);
                injector.BootTvImg.IsDefault = false;
                UpdateBootTvPictureBox();

                if (injector.BaseIsLoaded &&
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
            openFileDialog.FilterIndex = 0;
            openFileDialog.Filter = "Image file|*.png;*.jpg;*.bmp";
            if (checkBoxImagesPath.Checked && Directory.Exists(textBoxImagesPath.Text))
                openFileDialog.InitialDirectory = textBoxImagesPath.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                injector.BootDrcImg.Frame = new Bitmap(openFileDialog.FileName);
                injector.BootDrcImg.IsDefault = false;
                UpdateBootDrcPictureBox();

                if (injector.BaseIsLoaded &&
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
            openFileDialog.FilterIndex = 0;
            openFileDialog.Filter = "Image file|*.png;*.jpg;*.bmp";
            if (checkBoxImagesPath.Checked && Directory.Exists(textBoxImagesPath.Text))
                openFileDialog.InitialDirectory = textBoxImagesPath.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                injector.IconImg.Frame = new Bitmap(openFileDialog.FileName);
                injector.IconImg.IsDefault = false;
                UpdateIconPictureBox();

                if (injector.BaseIsLoaded &&
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
            openFileDialog.FilterIndex = 0;
            openFileDialog.Filter = "Image file|*.png;*.jpg;*.bmp";
            if (checkBoxImagesPath.Checked && Directory.Exists(textBoxImagesPath.Text))
                openFileDialog.InitialDirectory = textBoxImagesPath.Text;
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

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

                if (injector.BaseIsLoaded &&
                    injector.RomIsLoaded &&
                    (injector.IniIsLoaded || injector.IniPath == null))
                {
                    labelTitleId.Text = "Title ID: " + injector.TitleId;
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

        #region Packing

        private void buttonInjectPack_Click(object sender, EventArgs e)
        {
            Inject(true);
        }

        private void ButtonInjectNotPack_Click(object sender, EventArgs e)
        {
            Inject(false);
        }

        private void Inject(bool pack)
        {
            UpdateBootName();
            UpdateBootTvPictureBox();
            UpdateBootDrcPictureBox();

            bool _continue = true;

            if (textBoxShortName.Text.Length != 0)
                injector.ShortName = textBoxShortName.Text;
            else
            {
                _continue = false;
                MessageBox.Show(HelpString.Game_Name_Empty,
                    HelpString.Short_Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        MessageBox.Show(HelpString.Game_Long_Name_Empty,
                            HelpString.Long_Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }                    
                }
                else
                    injector.LongName = injector.ShortName;
            }

            folderBrowserDialog.Description = HelpString.Folder_Result_Select;
            folderBrowserDialog.SelectedPath = ResultPath;
            if (_continue && folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                injector.Encrypt = pack;
                injector.OutPath = folderBrowserDialog.SelectedPath + "\\" + injector.ShortNameASCII + " [" + injector.TitleId + "]";
                ResultPath = folderBrowserDialog.SelectedPath;

                if (Directory.Exists(injector.OutPath))
                    if (Directory.GetDirectories(injector.OutPath).Length != 0 ||
                        Directory.GetFiles(injector.OutPath).Length != 0)
                    {
                        _continue = false;
                        MessageBox.Show(HelpString.Folder_Exists_1 + " \"" + injector.OutPath + "\" " + HelpString.Folder_Exists_2,
                            "64Inject", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                if (_continue)
                {
                    Cll.Log.WriteLine("Injecting - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
                    if (injector.BasePath != null) Cll.Log.WriteLine("base: " + injector.BasePath);
                    if (injector.ShortName != null) Cll.Log.WriteLine("name: " + injector.ShortName);
                    if (injector.LongName != null) Cll.Log.WriteLine("longname:\n" + injector.LongName);
                    if (injector.InPath != null) Cll.Log.WriteLine("in: " + injector.InPath);
                    if (injector.RomPath != null) Cll.Log.WriteLine("rom: " + injector.RomPath);
                    if (injector.IniPath != null) Cll.Log.WriteLine("ini: " + injector.IniPath);
                    if (injector.BootTvPath != null) Cll.Log.WriteLine("tv: " + injector.BootTvPath);
                    if (injector.BootDrcPath != null) Cll.Log.WriteLine("drc: " + injector.BootDrcPath);
                    if (injector.IconPath != null) Cll.Log.WriteLine("icon: " + injector.IconPath);
                    if (injector.OutPath != null) Cll.Log.WriteLine("out: " + injector.OutPath);
                    Cll.Log.WriteLine("encrypt: " + injector.Encrypt.ToString());
                    Cll.Log.WriteLine("Please wait...");

                    _continue = injector.Inject();

                    LoadLogFile();


                    if (_continue)
                        MessageBox.Show(HelpString.Injection_Successfully,
                            "64Inject", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show(HelpString.Injection_Failed,
                            "64Inject", MessageBoxButtons.OK, MessageBoxIcon.Error);                    
                }
            }
        }

        #endregion

        #region Settings

        private void textBoxCommonKey_TextChanged(object sender, EventArgs e)
        {
            if (NusContent.LoadKey(textBoxCommonKey.Text))
            {
                textBoxCommonKey.Text = "";
                textBoxCommonKey.BackColor = Color.FromArgb(33, 33, 33);
                textBoxCommonKey.Enabled = false;
                panelValidKey.BackgroundImage = Properties.Resources.checkmark_16;
            }
            else
            {
                textBoxCommonKey.BackColor = Color.FromArgb(51, 51, 51);
                textBoxCommonKey.Enabled = true;
                panelValidKey.BackgroundImage = Properties.Resources.x_mark_16;
            }
        }

        private void ComboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string language;

            switch (comboBoxLanguage.SelectedIndex)
            {
                case 0:
                    language = "en-US";
                    break;
                case 1:
                    language = "es-MX";
                    break;
                default:
                    language = "en-US";
                    break;
            }

            ComponentResourceManager resources = new ComponentResourceManager(typeof(_64InjectGUI));
            ChangeLanguage(resources, this.Controls, language);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
        }

        private void ChangeLanguage(ComponentResourceManager resources, Control.ControlCollection ctrls, string language)
        {
            foreach (Control c in ctrls)
            {
                if(c.Name != "labelLoadedBase")
                    resources.ApplyResources(c, c.Name, new CultureInfo(language));
                if (c is Panel)
                    ChangeLanguage(resources, c.Controls, language);
                else if (c is GroupBox)
                    ChangeLanguage(resources, c.Controls, language);
            }
        }

        private void CheckBoxHelp_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxHelp.Checked)            
                groupBoxHelp.Visible = true;
            else            
                groupBoxHelp.Visible = false;
        }

        private void buttonRomPath_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = HelpString.Choose_ROM_Collection_Description;

            if (Directory.Exists(textBoxRomPath.Text))
                folderBrowserDialog.SelectedPath = textBoxRomPath.Text;
            else
                folderBrowserDialog.SelectedPath = "";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxRomPath.Text = folderBrowserDialog.SelectedPath;
        }

        private void buttonImagesPath_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = HelpString.Choose_Image_Collection_Description;

            if (Directory.Exists(textBoxImagesPath.Text))
                folderBrowserDialog.SelectedPath = textBoxImagesPath.Text;
            else
                folderBrowserDialog.SelectedPath = "";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxImagesPath.Text = folderBrowserDialog.SelectedPath;
        }

        private void buttonConfigFilesPath_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = HelpString.Choose_Config_Files_Collection_Description;

            if (Directory.Exists(textBoxConfigFilesPath.Text))
                folderBrowserDialog.SelectedPath = textBoxConfigFilesPath.Text;
            else
                folderBrowserDialog.SelectedPath = "";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxConfigFilesPath.Text = folderBrowserDialog.SelectedPath;
        }

        #endregion

        #region Help
        
        private void LabelRom_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.ROM;
            labelHelpText.Text = HelpString.ROM_Description;
        }

        private void TextBoxRom_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.ROM;
            labelHelpText.Text = HelpString.ROM_Description;
        }

        private void ButtonRom_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Choose_ROM;
            labelHelpText.Text = HelpString.Choose_ROM_Description;
        }

        private void LabelProductCode_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Product_Code;
            labelHelpText.Text = HelpString.Product_Code_Description;
        }

        private void LabelRom_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void TextBoxRom_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void ButtonRom_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void LabelProductCode_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void LabeShortName_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Short_Name;
            labelHelpText.Text = HelpString.Short_Name_Description;
        }

        private void TextBoxShortName_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Short_Name;
            labelHelpText.Text = HelpString.Short_Name_Description;
        }

        private void LabeShortName_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void TextBoxShortName_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void CheckBoxLongName_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Long_Name;
            labelHelpText.Text = HelpString.Long_Name_Description;
        }

        private void TextBoxLNLine1_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Long_Name;
            labelHelpText.Text = HelpString.Long_Name_Boxes_Description;
        }

        private void TextBoxLNLine2_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Long_Name;
            labelHelpText.Text = HelpString.Long_Name_Boxes_Description;
        }

        private void CheckBoxLongName_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void TextBoxLNLine1_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void TextBoxLNLine2_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void ButtonLoadBase_MouseEnter(object sender, EventArgs e)
        {
            if (checkBoxHelp.Checked)
            {
                groupBoxHelp.Text = HelpString.Load_Base;
                labelHelpText.Text = HelpString.Load_Base_Description;
                if (textBoxCommonKey.Enabled)
                    labelHelpText.Text += "\n" + HelpString.Load_Base_Warning_WiiUCK;
                if (NusContent.GetJavaVersion() == null)
                    labelHelpText.Text += "\n" + HelpString.Load_Base_Warning_Java;
            }
        }
        
        private void LabelTitleId_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Title_ID;
            labelHelpText.Text = HelpString.Title_ID_Description;
        }

        private void PanelLoadedBase_MouseEnter(object sender, EventArgs e)
        {
            if (injector.BaseIsLoaded)
            {
                groupBoxHelp.Text = HelpString.Base_Checked;
                labelHelpText.Text = HelpString.Base_Checked_Description;
            }
            else
            {
                groupBoxHelp.Text = HelpString.Base_Error;
                labelHelpText.Text = HelpString.Base_Error_Description;
            }
        }

        private void LabelLoadedBase_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Base;
            labelHelpText.Text = HelpString.Base_Description;
        }

        private void ButtonLoadBase_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void LabelTitleId_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void PanelLoadedBase_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void LabelLoadedBase_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void CheckBoxDarkFilter_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Dark_Filter;
            labelHelpText.Text = HelpString.Dark_Filter_Description;
        }

        private void CheckBoxWidescreen_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Widescreen;
            labelHelpText.Text = HelpString.Widescreen_Description;
        }

        private void LabelZoom_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Zoom;
            labelHelpText.Text = HelpString.Zoom_Description;
        }

        private void CheckBoxDarkFilter_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void CheckBoxWidescreen_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void LabelZoom_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void LabelConfigFile_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Config_File;
            labelHelpText.Text = HelpString.Config_File_Description;
        }

        private void TextBoxConfigFile_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Config_File;
            labelHelpText.Text = HelpString.Config_File_Description;
        }

        private void ButtonConfigFile_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Choose_Config_File;
            labelHelpText.Text = HelpString.Choose_Config_File_Description;
        }

        private void buttonEditConfigFile_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Edit_Config_File;
            labelHelpText.Text = HelpString.Edit_Config_File_Description;
        }

        private void LabelConfigFile_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void TextBoxConfigFile_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void ButtonConfigFile_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void buttonEditConfigFile_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void ButtonIcon_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Icon;
            labelHelpText.Text = HelpString.Icon_Description;
        }

        private void ButtonBootDrc_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.GamePad;
            labelHelpText.Text = HelpString.GamePad_Description;
        }

        private void ButtonBootTv_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.TV;
            labelHelpText.Text = HelpString.TV_Description;
        }

        private void ButtonTitleScreen_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Title_Screen;
            labelHelpText.Text = HelpString.Title_Screen_Description;
        }
        private void ButtonIcon_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void ButtonBootDrc_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void ButtonBootTv_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void ButtonTitleScreen_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void LabelReleased_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Released;
            labelHelpText.Text = HelpString.Released_Description;
        }

        private void ComboBoxReleased_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Released;
            labelHelpText.Text = HelpString.Released_Description;
        }

        private void LabelPlayers_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Players;
            labelHelpText.Text = HelpString.Players_Description;
        }

        private void ComboBoxPlayers_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Players;
            labelHelpText.Text = HelpString.Players_Description;
        }

        private void CheckBoxShowName_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Show_Name;
            labelHelpText.Text = HelpString.Show_Name_Description;
        }

        private void LabelReleased_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void ComboBoxReleased_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void LabelPlayers_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void ComboBoxPlayers_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void CheckBoxShowName_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void PictureBoxIcon_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Icon_Preview;
            labelHelpText.Text = HelpString.Icon_Preview_Description;
        }

        private void PictureBoxBootDrc_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.GamePad_Preview;
            labelHelpText.Text = HelpString.GamePad_Preview_Description;
        }

        private void PictureBoxBootTv_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.TV_Preview;
            labelHelpText.Text = HelpString.TV_Preview_Description;
        }

        private void PictureBoxIcon_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void PictureBoxBootDrc_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void PictureBoxBootTv_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void ButtonInjectPack_MouseEnter(object sender, EventArgs e)
        {
            if (checkBoxHelp.Checked)
            {
                groupBoxHelp.Text = HelpString.Pack;
                labelHelpText.Text = HelpString.Pack_Description;
                if (textBoxCommonKey.Enabled)
                    labelHelpText.Text += "\n" + HelpString.Injection_Warning_WiiUCK;
                if (NusContent.GetJavaVersion() == null)
                    labelHelpText.Text += "\n" + HelpString.Injection_Warning_Java;
            }
        }

        private void ButtonInjectNotPack_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Not_Pack;
            labelHelpText.Text = HelpString.Not_Pack_Description;
        }

        private void PanelPackingQuestion_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Packing;
            labelHelpText.Text = HelpString.Enable_Packing;
        }

        private void TextBoxLog_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Log;
            labelHelpText.Text = HelpString.Log_Description;
        }

        private void ButtonInjectPack_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void ButtonInjectNotPack_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void PanelPackingQuestion_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void TextBoxLog_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void LabelCommonKey_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Wii_U_Common_Key;
            labelHelpText.Text = HelpString.Wii_U_Common_Key_Description;
        }

        private void TextBoxCommonKey_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Wii_U_Common_Key;
            labelHelpText.Text = HelpString.Wii_U_Common_Key_Box_Description;
        }

        private void PanelValidKey_MouseEnter(object sender, EventArgs e)
        {
            if (textBoxCommonKey.Enabled)
            {
                groupBoxHelp.Text = HelpString.Wii_U_Common_Key_Error;
                labelHelpText.Text = HelpString.Wii_U_Common_Key_Error_Description;
            }
            else
            {                    
                groupBoxHelp.Text = HelpString.Wii_U_Common_Key_Checked;
                labelHelpText.Text = HelpString.Wii_U_Common_Key_Checked_Description;
            }            
        }

        private void CheckBoxHelp_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Help_Box;
            labelHelpText.Text = HelpString.Help_Description;
        }

        private void LabelCommonKey_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void TextBoxCommonKey_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void PanelValidKey_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void CheckBoxHelp_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void CheckBoxRomPath_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.ROM_Collection;
            labelHelpText.Text = HelpString.ROM_Collection_Description;
        }

        private void TextBoxRomPath_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.ROM_Collection_Path;
            labelHelpText.Text = HelpString.ROM_Collection_Path_Description;
        }

        private void ButtonRomPath_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Choose_ROM_Collection;
            labelHelpText.Text = HelpString.Choose_ROM_Collection_Description;
        }

        private void CheckBoxRomPath_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void TextBoxRomPath_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void ButtonRomPath_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void CheckBoxImagesPath_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Image_Collection;
            labelHelpText.Text = HelpString.Image_Collection_Description;
        }

        private void TextBoxImagesPath_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Image_Collection_Path;
            labelHelpText.Text = HelpString.Image_Collection_Path_Description;
        }

        private void ButtonImagesPath_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Choose_Image_Collection;
            labelHelpText.Text = HelpString.Choose_Image_Collection_Description;
        }

        private void CheckBoxImagesPath_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void TextBoxImagesPath_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void ButtonImagesPath_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        //********************************************************************************
        private void CheckBoxConfigFilesPath_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Config_Files_Collection;
            labelHelpText.Text = HelpString.Config_Files_Collection_Description;
        }

        private void TextBoxConfigFilesPath_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Config_Files_Collection_Path;
            labelHelpText.Text = HelpString.Config_Files_Collection_Path_Description;
        }

        private void ButtonConfigFilesPath_MouseEnter(object sender, EventArgs e)
        {
            groupBoxHelp.Text = HelpString.Choose_Config_Files_Collection;
            labelHelpText.Text = HelpString.Choose_Config_Files_Collection_Description;
        }

        private void CheckBoxConfigFilesPath_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void TextBoxConfigFilesPath_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        private void ButtonConfigFilesPath_MouseLeave(object sender, EventArgs e)
        {
            groupBoxHelp.Text = "";
            labelHelpText.Text = "";
        }

        #endregion
    }
}
