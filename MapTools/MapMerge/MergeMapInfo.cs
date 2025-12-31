using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CkMp.Data.Map;

namespace MapTools.MapMerge
{
    public class MergeMapInfo
    {
        public Map Map { get; set; }
        public String MapName { get; set; }
        public int[] Crop { get; set; } = new int[4];
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public byte Rotation { get; set; }
        public bool MirroredVertical { get; set; }
        public bool MirroredHorizontal { get; set; }
        public Grid<bool> CropMap { get; set;}

        //final values of prepared map
        public int startX;
        public int startY;
        public int sizeX;
        public int sizeY;
    }

    public class MergeOptions
    {

    }
}
