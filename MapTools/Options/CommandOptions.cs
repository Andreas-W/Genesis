using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapTools.Options
{
    public class CloneOptions
    {
        public bool CloneTerrain = true;
        public bool CloneBlendTiles = true;
        public bool CloneHeightmap = true;
        public bool CloneObjects = true;
        public bool CloneWaypoints = true;
        public bool CloneAreas = true;
        //TODO
    }

    public class ResizeOptions
    {
        public bool RemoveObjects = true;
        public bool RemoveWaypoints = false;
        public bool RemoveAreas = false;

        public int DefaultHeight = 16;
    }
}
