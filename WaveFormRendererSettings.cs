using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NAudio.WaveFormRenderer
{
    public class WaveFormRendererSettings
    {

        // for display purposes only
        public string Name { get; set; }

        public int Width { get; set; }

        public int TopHeight { get; set; }
        public int BottomHeight { get; set; }
        public int PixelsPerPeak { get; set; }
        public int SpacerPixels { get; set; }
        public int BlockWidth { get; set; }
        //public virtual Pen TopPeakPen { get; set; }
        //public virtual Pen TopSpacerPen { get; set; }
        //public virtual Pen BottomPeakPen { get; set; }
        //public virtual Pen BottomSpacerPen { get; set; }
        public bool DecibelScale { get; set; }
        public Windows.UI.Color BackgroundColor { get; set; }
        public Image BackgroundImage { get; set; }
        public Brush BackgroundBrush
        {
            get; set;
        }
        public Brush TopBarBrush
        {
            get; set;
        }
        public Brush BottomBarBrush
        {
            get; set;
        }

        //protected static Pen CreateGradientPen(int height, Color startColor, Color endColor)
        //{
        //    var brush = new LinearGradientBrush(new Point(0, 0), new Point(0, height), startColor, endColor);
        //    return new Pen(brush);
        //}
    }
}