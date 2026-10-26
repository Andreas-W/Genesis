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

        /// <summary>
        /// Leaves the "Default Water" area alone: it is neither cloned nor removed.
        /// It usually covers the whole map already, so copying it just stacks duplicates.
        /// </summary>
        public bool SkipDefaultWater = true;
        //TODO
    }

    public class ResizeOptions
    {
        public bool RemoveObjects = true;
        public bool RemoveWaypoints = false;
        public bool RemoveAreas = false;

        public int DefaultHeight = 16;
    }

    public class WakOptions
    {
        /// <summary>
        /// Water level in heightmap units (same scale as the HeightMap bytes in the map file,
        /// i.e. before HeightMapScale is applied).
        /// </summary>
        public float WaterHeight = 80f;

        /// <summary>
        /// HeightMapScale from the map's map.ini: the game multiplies the stored heights by it when loading.
        /// </summary>
        public float HeightMapScale = 1f;

        /// <summary>
        /// Waves are only placed where the terrain slope is below this value (steep shores get none).
        /// </summary>
        public float GradientThreshold = 15f;

        /// <summary>
        /// Spacing between waves along the shore line, in world units.
        /// </summary>
        public float WaveDistance = 45f;

        /// <summary>
        /// Moves each wave towards the land by this many world units.
        /// </summary>
        public float InwardsShift = 0f;

        /// <summary>
        /// Smooths the shore line before the waves are placed (gaussian sigma in world units, 0 = off).
        /// When on, waves point along the normal of the smoothed shore instead of the local terrain slope.
        /// </summary>
        public float ShoreSmoothing = 0f;

        /// <summary>
        /// Water narrower than this (world units) gets no waves, e.g. rivers and channels. 0 = off.
        /// </summary>
        public float MinWaterWidth = 0f;

        /// <summary>
        /// 0 Pond, 1 Ocean, 2 CloseOcean, 3 CloseOceanDouble, 4 Radial
        /// </summary>
        public int WaveType = 3;
    }
}
