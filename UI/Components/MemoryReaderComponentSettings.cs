using LiveSplit.Options;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model.Input;
using System.Collections.Generic;

namespace LiveSplit.UI.Components
{
    public partial class MemoryReaderComponentSettings : UserControl
    {
        public MemoryReaderComponentSettings()
        {
            InitializeComponent();

            Hook = new CompositeHook();

            // Set default values.
            MemReaderFont = new Font("Segoe UI", 13, FontStyle.Regular, GraphicsUnit.Pixel);
            OverrideMemReaderFont = false;
            MemReaderTextColor = Color.FromArgb(255, 255, 255, 255);
            MemReaderValueColor = Color.FromArgb(255, 255, 255, 255);
            OverrideTextColor = false;
            BackgroundColor = Color.Transparent;
            BackgroundColor2 = Color.Transparent;
            BackgroundGradient = GradientType.Plain;
            MemReaderText = "Mem Value";
            MemReaderGameTitle = "Process Name (no .exe)";
            MemReaderAddress = "0x00112233";
            MemReaderType = 1; // Default to reading 2 bytes
            MemReaderSigned = false;
            MemReaderAddressList = new List<Game>();

            // Set bindings.

            memReaderText.DataBindings.Add("Text", this, "MemReaderText");
            memReaderGameTitle.DataBindings.Add("Text", this, "memReaderGameTitle");
            memReaderAddress.DataBindings.Add("Text", this, "memReaderAddress");
            memReaderSigned.DataBindings.Add("Checked", this, "memReaderSigned");
            memReaderHide.DataBindings.Add("Checked", this, "memReaderHide");

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
        public bool MemReaderHide { get; set; }
        public IList<Game> MemReaderAddressList { get; set; }
        public IntPtr[] MemReaderPointer { get; set; }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;

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
            MemReaderHide = SettingsHelper.ParseBool(element["MemReaderHide"]);
            

            ReadAddressList(); // Read from XML
            
            PopulateAddressList(); // Update UI

            IntPtr[] ptr = MemoryReader.ConstructPointer(MemReaderAddress);
            if (ptr != null)
                MemReaderPointer = MemoryReader.ConstructPointer(MemReaderAddress);


            if (MemReaderType == 0) radioByte.Checked = true;
            if (MemReaderType == 1) radioByte2.Checked = true;
            if (MemReaderType == 2) radioByte4.Checked = true;
            if (MemReaderType == 3) radioByte8.Checked = true;
            if (MemReaderType == 4) radioFloat.Checked = true;

        }

        private void ReadAddressList()
        {
            MemReaderAddressList.Clear();
            
            var addressListPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Components\\MemReader.AddressList.xml";

            if (File.Exists(addressListPath)) {

                XmlDocument xdc = new XmlDocument();
                xdc.Load(addressListPath);
                XmlNodeList gameNodes = xdc.SelectNodes("/AddressList/Game");
            
                foreach (XmlNode gameNode in gameNodes)
                {
                    var game = Game.FromXml((XmlNode)gameNode);
                    MemReaderAddressList.Add(game);
                }

            }

            
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
            SettingsHelper.CreateSetting(document, parent, "MemReaderSigned", MemReaderSigned) ^
            SettingsHelper.CreateSetting(document, parent, "MemReaderHide", MemReaderSigned);
           
        }


        public void PopulateAddressList()
        {

            memReaderAddress.Items.Clear();

            bool match = false;

            foreach (Game g in MemReaderAddressList)
            {
                if (MemReaderGameTitle == g.Name)
                {
                    match = true;

                    foreach ( string s in g.AddressList )
                    {
                        memReaderAddress.Items.Add(s.Replace(",", ", "));
                    }
                }
            }

            memReaderAddress.ResetText();

            if (match) memReaderAddress.SelectedIndex = memReaderAddress.Items.Count-1;
            else memReaderAddress.SelectedIndex = -1;
            
        }


        private void MemReaderSettings_Load(object sender, EventArgs e)
        {
            ReadAddressList();
            PopulateAddressList();

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

            IntPtr[] ptr = MemoryReader.ConstructPointer(MemReaderAddress);
            if (ptr != null)
                MemReaderPointer = MemoryReader.ConstructPointer(MemReaderAddress);
        }

        private void memReaderGameTitle_TextChanged(object sender, EventArgs e)
        {
            MemReaderGameTitle = memReaderGameTitle.Text;
            PopulateAddressList();
        }

        private void memReaderText_TextChanged(object sender, EventArgs e)
        {
            MemReaderText = memReaderText.Text;
        }

        private void memReaderAddressAdd_Click(object sender, EventArgs e)
        {
            Game game = null;
            foreach (Game g in MemReaderAddressList)
            {
                if (g.Name == memReaderGameTitle.Text) game = g;
            }

            if (game == null)
            {
                game = new Game(memReaderGameTitle.Text);
                game.Add(MemReaderAddress);
                MemReaderAddressList.Add(game);
            }
            else game.Add(memReaderAddress.Text);

            // Save addresslist to XML

            SaveAddressListXML();
            PopulateAddressList();
            
        }

        private void memReaderAddressRemove_Click(object sender, EventArgs e)
        {
            foreach (Game g in MemReaderAddressList)
            {
                if (g.Name == MemReaderGameTitle) g.Remove(memReaderAddress.Text);
            }
            SaveAddressListXML();
            PopulateAddressList();
        }

        private void SaveAddressListXML()
        {
            try
            {
                var addressListPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Components\\MemReader.AddressList.xml";
                if (!File.Exists(addressListPath))
                    File.Create(addressListPath).Close();

                using (var memoryStream = new MemoryStream())
                {
                    XMLAddressListSaver.Save(MemReaderAddressList, memoryStream);

                    using (var stream = File.Open(addressListPath, FileMode.Create, FileAccess.Write))
                    {
                        var buffer = memoryStream.GetBuffer();
                        stream.Write(buffer, 0, (int)memoryStream.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Address list could not be saved!", "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex);
            }
        }

        private void memReaderAddressHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "The address list is stored in Livesplit\\Components\\MemReader.AddressList.xml" +
                "\n" +
                "\n" +
                "When you change the Game Title and all pointers disappear from the list they're not actually gone, just type in the same Game Title again and they'll pop back in." +
                "\n" +
                "\n" +
                "Multi-level pointers are accepted, see working examples below:" +
                "\n" +
                "\n" +
                "       0x2AB, 0xCDE, 0x123" +
                "\n" +
                "       2AB, CDE, 123" +
                "\n" +
                "\n" +
                "Labelling pointers will be added soon!",
                "Address Help",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void memReaderHide_CheckedChanged(object sender, EventArgs e)
        {
            MemReaderHide = memReaderHide.Checked;
        }
    }
}
