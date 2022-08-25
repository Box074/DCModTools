using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCSkinGUI
{
    public class ClipInfo
    {
        public string Name { get; set; } = null!;
        public List<Tile> Tiles { get; set; } = null!;
        public int ModifiedLevel { get; set; } = 0;
        public int ModifiedCount { get; set; } = 0;
        public WeakReference<List<FrameInfo>> Frames { get; set; } = null!;
        public List<AdditionalFrameInfo> AppendedFrames { get; set; } = new();
        public string Background => ModifiedLevel switch
        {
            1 => "Yellow",
            2 => "Green",
            _ => "White"
        };
    }
}
