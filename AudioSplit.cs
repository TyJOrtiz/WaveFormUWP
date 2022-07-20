using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace TabbedConcept
{
    public class AudioSplit
    {
        public double Start { get; set; }
        public double End { get; set; }
        public string EndText { get; set; }
        public string StartText { get; set; }
        public MediaPlaybackItem PlayBackItem { get; set; }
        public object TrimVisual { get; set; }
        public double StartDouble { get; set; }
        public double EndDouble { get; set; }
        public double Width { get; set; }
    }
}
