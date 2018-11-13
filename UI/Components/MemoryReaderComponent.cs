using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public class MemoryReaderComponent : IComponent
    {
        public MemoryReaderComponent(LiveSplitState state)
        {
            VerticalHeight = 10;
            Settings = new MemoryReaderComponentSettings();
            Cache = new GraphicsCache();
            MemoryReaderTextLabel = new SimpleLabel();
            MemoryReader = new MemoryReader();

            this.state = state;
        }

        public MemoryReader MemoryReader { get; set; }
        public MemoryReaderComponentSettings Settings { get; set; }

        public GraphicsCache Cache { get; set; }

        public float VerticalHeight { get; set; }

        public float MinimumHeight { get; set; }

        public float MinimumWidth 
        { 
            get
            {
                return MemoryReaderTextLabel.X + MemoryReaderValueLabel.ActualWidth;
            } 
        }

        public float HorizontalWidth { get; set; }

        public IDictionary<string, Action> ContextMenuControls
        {
            get { return null; }
        }

        public float PaddingTop { get; set; }
        public float PaddingLeft { get { return 7f; } }
        public float PaddingBottom { get; set; }
        public float PaddingRight { get { return 7f; } }

        protected SimpleLabel MemoryReaderTextLabel = new SimpleLabel();
        protected SimpleLabel MemoryReaderValueLabel = new SimpleLabel();

        protected Font MemoryReaderFont { get; set; }

        private LiveSplitState state;
        


        private void DrawGeneral(Graphics g, Model.LiveSplitState state, float width, float height, LayoutMode mode)
        {
            // Set Background colour.
            if (Settings.BackgroundColor.A > 0
                || Settings.BackgroundGradient != GradientType.Plain
                && Settings.BackgroundColor2.A > 0)
            {
                var gradientBrush = new LinearGradientBrush(
                            new PointF(0, 0),
                            Settings.BackgroundGradient == GradientType.Horizontal
                            ? new PointF(width, 0)
                            : new PointF(0, height),
                            Settings.BackgroundColor,
                            Settings.BackgroundGradient == GradientType.Plain
                            ? Settings.BackgroundColor
                            : Settings.BackgroundColor2);

                g.FillRectangle(gradientBrush, 0, 0, width, height);
            }

            // Set Font.
            MemoryReaderFont = Settings.OverrideMemReaderFont ? Settings.MemReaderFont : state.LayoutSettings.TextFont;

            // Calculate Height from Font.
            var textHeight = g.MeasureString("A", MemoryReaderFont).Height;
            VerticalHeight = 1.2f * textHeight;
            MinimumHeight = MinimumHeight;

            PaddingTop = Math.Max(0, ((VerticalHeight - 0.75f * textHeight) / 2f));
            PaddingBottom = PaddingTop;

            // Assume four digit size for memory value
            float fourCharWidth = g.MeasureString("1000", MemoryReaderFont).Width;
            HorizontalWidth = MemoryReaderTextLabel.X + MemoryReaderTextLabel.ActualWidth + (fourCharWidth > MemoryReaderValueLabel.ActualWidth ? fourCharWidth : MemoryReaderValueLabel.ActualWidth) + 5;

            // Set Memory Reader Text Label
            MemoryReaderTextLabel.HorizontalAlignment = mode == LayoutMode.Horizontal ? StringAlignment.Near : StringAlignment.Near;
            MemoryReaderTextLabel.VerticalAlignment = StringAlignment.Center;
            MemoryReaderTextLabel.X = 5;
            MemoryReaderTextLabel.Y = 0;
            MemoryReaderTextLabel.Width = (width - fourCharWidth - 5);
            MemoryReaderTextLabel.Height = height;
            MemoryReaderTextLabel.Font = MemoryReaderFont;
            MemoryReaderTextLabel.Brush = new SolidBrush(Settings.OverrideTextColor ? Settings.MemReaderTextColor : state.LayoutSettings.TextColor);
            MemoryReaderTextLabel.HasShadow = state.LayoutSettings.DropShadows;
            MemoryReaderTextLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
            MemoryReaderTextLabel.OutlineColor = state.LayoutSettings.TextOutlineColor;
            MemoryReaderTextLabel.Draw(g);

            // Set Memory Reader Value Label.
            MemoryReaderValueLabel.HorizontalAlignment = mode == LayoutMode.Horizontal ? StringAlignment.Far : StringAlignment.Far;
            MemoryReaderValueLabel.VerticalAlignment = StringAlignment.Center;
            MemoryReaderValueLabel.X = 5;
            MemoryReaderValueLabel.Y = 0;
            MemoryReaderValueLabel.Width = (width - 10);
            MemoryReaderValueLabel.Height = height;
            MemoryReaderValueLabel.Font = MemoryReaderFont;
            MemoryReaderValueLabel.Brush = new SolidBrush(Settings.OverrideTextColor ? Settings.MemReaderValueColor : state.LayoutSettings.TextColor);
            MemoryReaderValueLabel.HasShadow = state.LayoutSettings.DropShadows;
            MemoryReaderValueLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
            MemoryReaderValueLabel.OutlineColor = state.LayoutSettings.TextOutlineColor;
            MemoryReaderValueLabel.Draw(g);
            
        }

        public void DrawHorizontal(Graphics g, Model.LiveSplitState state, float height, Region clipRegion)
        {
            DrawGeneral(g, state, HorizontalWidth, height, LayoutMode.Horizontal);
        }

        public void DrawVertical(System.Drawing.Graphics g, Model.LiveSplitState state, float width, Region clipRegion)
        {
            DrawGeneral(g, state, width, VerticalHeight, LayoutMode.Vertical);
        }

        public string ComponentName
        {
            get { return "Memory Reader"; }
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return Settings;
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
            
        }

        public void Update(IInvalidator invalidator, Model.LiveSplitState state, float width, float height, LayoutMode mode)
        {
            try
            {
                if (Settings.Hook != null)
                    Settings.Hook.Poll();
            }
            catch { }

            this.state = state;
            
            // Set memory value label
            MemoryReaderTextLabel.Text = Settings.MemReaderText;

            // Read memory and convert
            byte[] mem = MemoryReader.ReadMemory(Settings.MemReaderGameTitle, Settings.MemReaderPointer, true);

            if (mem != null) MemoryReaderValueLabel.Text = ConvertMemory(mem);
            else MemoryReaderValueLabel.Text = "-";

            Cache.Restart();
            Cache["MemoryReaderTextLabel"] = MemoryReaderTextLabel.Text;
            Cache["MemoryReaderValueLabel"] = MemoryReaderValueLabel.Text;

            if (invalidator != null && Cache.HasChanged)
            {
                invalidator.Invalidate(0, 0, width, height);
            }

        }



        public int GetSettingsHashCode()
        {
            return Settings.GetSettingsHashCode();
        }

        public void Dispose()
        {

        }

        private string ConvertMemory(Byte[] memory)
        {
            int x = -123;
            uint xx = 123;
            long z = -123;
            ulong zz = 123;
            float f = -123;

            if (Settings.MemReaderType == 4) { f = BitConverter.ToSingle(memory, 0); }
            else if (Settings.MemReaderSigned)
            {
                if (Settings.MemReaderType == 1) x = BitConverter.ToInt16(memory, 0);
                if (Settings.MemReaderType == 2) x = BitConverter.ToInt32(memory, 0);
                if (Settings.MemReaderType == 3) z = BitConverter.ToInt64(memory, 0);
            } else
            {
                if (Settings.MemReaderType == 1) xx = BitConverter.ToUInt16(memory, 0);
                if (Settings.MemReaderType == 2) xx = BitConverter.ToUInt32(memory, 0);
                if (Settings.MemReaderType == 3) zz = BitConverter.ToUInt64(memory, 0);
            }

            if (Settings.MemReaderType == 0 && Settings.MemReaderSigned) return ((sbyte)(memory[0] - 128)).ToString();
            else if (Settings.MemReaderType == 0 && !Settings.MemReaderSigned) return memory[0].ToString();
            else if (x != -123) return x.ToString();
            else if (xx != 123) return xx.ToString();
            else if (z != -123) return z.ToString();
            else if (zz != 123) return zz.ToString();
            else if (f != -123) return f.ToString();
            else return "Error";

        }

        
    }
}
