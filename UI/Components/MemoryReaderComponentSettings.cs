using LiveSplit.Options;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model.Input;
using System.Threading;

namespace LiveSplit.UI.Components
{
    public partial class MemoryReaderComponentSettings : UserControl
    {
        public MemoryReaderComponentSettings()
        {
            InitializeComponent();

            Hook = new CompositeHook();

            // Set default values.
            GlobalHotkeysEnabled = false;
            MemReaderFont = new Font("Segoe UI", 13, FontStyle.Regular, GraphicsUnit.Pixel);
            OverrideMemReaderFont = false;
            MemReaderTextColor = Color.FromArgb(255, 255, 255, 255);
            MemReaderValueColor = Color.FromArgb(255, 255, 255, 255);
            OverrideTextColor = false;
            BackgroundColor = Color.Transparent;
            BackgroundColor2 = Color.Transparent;
            BackgroundGradient = GradientType.Plain;
            MemReaderText = "Value:";
            MemReaderGameTitle = "Process Name";
            MemReaderAddress = "0x00112233";
            MemReaderType = 1; // Default to reading 2 bytes
            MemReaderSigned = false;

            // Set bindings.

            memReaderText.DataBindings.Add("Text", this, "MemReaderText");
            memReaderGameTitle.DataBindings.Add("Text", this, "memReaderGameTitle");
            memReaderAddress.DataBindings.Add("Text", this, "memReaderAddress");
            memReaderSigned.DataBindings.Add("Checked", this, "memReaderSigned");

            chkFont.DataBindings.Add("Checked", this, "OverrideMemReaderFont", false, DataSourceUpdateMode.OnPropertyChanged);
            lblFont.DataBindings.Add("Text", this, "MemReaderFontString", false, DataSourceUpdateMode.OnPropertyChanged);
            chkColor.DataBindings.Add("Checked", this, "OverrideTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor.DataBindings.Add("BackColor", this, "MemReaderTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor3.DataBindings.Add("BackColor", this, "MemReaderValueColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor2.DataBindings.Add("BackColor", this, "BackgroundColor2", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbGradientType.DataBindings.Add("SelectedItem", this, "GradientString", false, DataSourceUpdateMode.OnPropertyChanged);

            // Assign event handlers.
            cmbGradientType.SelectedIndexChanged += cmbGradientType_SelectedIndexChanged;
            chkFont.CheckedChanged += chkFont_CheckedChanged;
            chkColor.CheckedChanged += chkColor_CheckedChanged;

            Load += MemReaderSettings_Load;
            
        }

        public CompositeHook Hook { get; set; }

        public bool GlobalHotkeysEnabled { get; set; }

        public Color MemReaderTextColor { get; set; }
        public Color MemReaderValueColor { get; set; }
        public bool OverrideTextColor { get; set; }

        public string MemReaderFontString { get { return String.Format("{0} {1}", MemReaderFont.FontFamily.Name, MemReaderFont.Style); } }
        public Font MemReaderFont { get; set; }
        public bool OverrideMemReaderFont { get; set; }

        public Color BackgroundColor { get; set; }
        public Color BackgroundColor2 { get; set; }
        public GradientType BackgroundGradient { get; set; }
        public String GradientString
        {
            get { return BackgroundGradient.ToString(); }
            set { BackgroundGradient = (GradientType)Enum.Parse(typeof(GradientType), value); }
        }

        public string MemReaderText { get; set; }
        public string MemReaderGameTitle { get; set; }
        public string MemReaderAddress { get; set; }
        public int MemReaderType { get; set; }
        public bool MemReaderSigned { get; set; }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            GlobalHotkeysEnabled = SettingsHelper.ParseBool(element["GlobalHotkeysEnabled"]);
            MemReaderTextColor = SettingsHelper.ParseColor(element["MemReaderTextColor"]);
            MemReaderValueColor = SettingsHelper.ParseColor(element["MemReaderColor"]);
            MemReaderFont = SettingsHelper.GetFontFromElement(element["MemReaderFont"]);
            OverrideMemReaderFont = SettingsHelper.ParseBool(element["OverrideMemReaderFont"]);
            OverrideTextColor = SettingsHelper.ParseBool(element["OverrideTextColor"]);
            BackgroundColor = SettingsHelper.ParseColor(element["BackgroundColor"]);
            BackgroundColor2 = SettingsHelper.ParseColor(element["BackgroundColor2"]);
            GradientString = SettingsHelper.ParseString(element["BackgroundGradient"]);
            MemReaderText = SettingsHelper.ParseString(element["MemReaderText"]);
            MemReaderGameTitle = SettingsHelper.ParseString(element["MemReaderGameTitle"]);
            MemReaderAddress = SettingsHelper.ParseString(element["MemReaderAddress"]);
            MemReaderType = SettingsHelper.ParseInt(element["MemReaderType"]);
            MemReaderSigned = SettingsHelper.ParseBool(element["MemReaderSigned"]);

            if (MemReaderType == 0) radioByte.Checked = true;
            if (MemReaderType == 1) radioByte2.Checked = true;
            if (MemReaderType == 2) radioByte4.Checked = true;
            if (MemReaderType == 3) radioByte8.Checked = true;
            if (MemReaderType == 4) radioFloat.Checked = true;

        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            return parent;
        }

        public int GetSettingsHashCode()
        {
            return CreateSettingsNode(null, null);
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "Version", "1.0") ^
            SettingsHelper.CreateSetting(document, parent, "GlobalHotkeysEnabled", GlobalHotkeysEnabled) ^
            SettingsHelper.CreateSetting(document, parent, "OverrideMemReaderFont", OverrideMemReaderFont) ^
            SettingsHelper.CreateSetting(document, parent, "OverrideTextColor", OverrideTextColor) ^
            SettingsHelper.CreateSetting(document, parent, "MemReaderFont", MemReaderFont) ^
            SettingsHelper.CreateSetting(document, parent, "MemReaderTextColor", MemReaderTextColor) ^
            SettingsHelper.CreateSetting(document, parent, "MemReaderValueColor", MemReaderValueColor) ^
            SettingsHelper.CreateSetting(document, parent, "BackgroundColor", BackgroundColor) ^
            SettingsHelper.CreateSetting(document, parent, "BackgroundColor2", BackgroundColor2) ^
            SettingsHelper.CreateSetting(document, parent, "BackgroundGradient", BackgroundGradient) ^
            SettingsHelper.CreateSetting(document, parent, "MemReaderText", MemReaderText) ^
            SettingsHelper.CreateSetting(document, parent, "MemReaderGameTitle", MemReaderGameTitle) ^
            SettingsHelper.CreateSetting(document, parent, "MemReaderAddress", MemReaderAddress) ^
            SettingsHelper.CreateSetting(document, parent, "MemReaderType", MemReaderType) ^
            SettingsHelper.CreateSetting(document, parent, "MemReaderSigned", MemReaderSigned);
        }

        private void MemReaderSettings_Load(object sender, EventArgs e)
        {
            chkColor_CheckedChanged(null, null);
            chkFont_CheckedChanged(null, null);
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            SettingsHelper.ColorButtonClick((Button)sender, this);
        }

        private void btnFont_Click(object sender, EventArgs e)
        {
            var dialog = SettingsHelper.GetFontDialog(MemReaderFont, 7, 20);
            dialog.FontChanged += (s, ev) => MemReaderFont = ((CustomFontDialog.FontChangedEventArgs)ev).NewFont;
            dialog.ShowDialog(this);
            lblFont.Text = MemReaderFontString;
        }

        private void chkColor_CheckedChanged(object sender, EventArgs e)
        {
            label3.Enabled = btnColor.Enabled = label5.Enabled = btnColor3.Enabled = chkColor.Checked;
        }

        void chkFont_CheckedChanged(object sender, EventArgs e)
        {
            label1.Enabled = lblFont.Enabled = btnFont.Enabled = chkFont.Checked;
        }

        void cmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnColor1.Visible = cmbGradientType.SelectedItem.ToString() != "Plain";
            btnColor2.DataBindings.Clear();
            btnColor2.DataBindings.Add("BackColor", this, btnColor1.Visible ? "BackgroundColor2" : "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            GradientString = cmbGradientType.SelectedItem.ToString();
        }

        private void radioBytes_CheckedChanged(object sender, EventArgs e)
        {
            if (radioByte.Checked) MemReaderType = 0;
            if (radioByte2.Checked) MemReaderType = 1;
            if (radioByte4.Checked) MemReaderType = 2;
            if (radioByte8.Checked) MemReaderType = 3;
            if (radioFloat.Checked) MemReaderType = 4;
        }

        private void memReaderSigned_CheckedChanged(object sender, EventArgs e)
        {
            MemReaderSigned = memReaderSigned.Checked;
        }

        private void memReaderAddress_TextChanged(object sender, EventArgs e)
        {
            MemReaderAddress = memReaderAddress.Text;
        }

        private void memReaderGameTitle_TextChanged(object sender, EventArgs e)
        {
            MemReaderGameTitle = memReaderGameTitle.Text;
        }

        private void memReaderText_TextChanged(object sender, EventArgs e)
        {
            MemReaderText = memReaderText.Text;
        }
    }
}
