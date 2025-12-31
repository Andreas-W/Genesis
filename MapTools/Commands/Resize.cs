using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CkMp.Data;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using MapTools.Utils;
using MapTools.Options;

namespace MapTools.Commands
{
    public class Resize
    {

        /* Resizes the map by adding the supplied values to each size.
         * Positive values increase map size, negative values reduce it.
         * Options: Keep/Remove Objects, Areas or Waypoints outside boundaries
         * Border value is unchanged.
         * sizeDiff -> 0 Left (-x), 1 Top (-y), 2 Right (+x), 3 Bottom (+y)
         */
        public static void resize(Map map, int[] sizeDiff, ResizeOptions options)
        {
            Grid<bool> cm;
            resize(map, sizeDiff, options, out cm);
        }

        //CropMap = boolean map to show which tiles are actual map area
        public static void resize(Map map, int[] sizeDiff, ResizeOptions options, out Grid<bool> cropMap)
        {
            int width = map.HeightMap.Width + sizeDiff[0] + sizeDiff[2];
            int height = map.HeightMap.Height + sizeDiff[1] + sizeDiff[3];

            //----------------------
            //resize HeightMap/Tiles
            //----------------------
            var hmOld = map.HeightMap;
            var tdOld = map.Tiles;
            map.HeightMap = new HeightMap(width, height, map.HeightMap.Border);
            map.Tiles = new TileData(width, height);
            map.Tiles.BlendTiles = tdOld.BlendTiles;
            map.Tiles.NumberOfBlendTiles = tdOld.NumberOfBlendTiles;

            cropMap = new Grid<bool>(width, height);

            for (int x = 0; x < hmOld.Width; x++)
            {
                for (int y = 0; y < hmOld.Height; y++)
                {
                    //coordinates are within new map boundaries
                    if (x+sizeDiff[0] >= 0 && y+sizeDiff[1] >= 0 && x + sizeDiff[0] < width && y + sizeDiff[1] < height)
                    {
                        map.HeightMap[x+sizeDiff[0], y+sizeDiff[1]] = hmOld[x, y];
                        map.Tiles[x + sizeDiff[0], y + sizeDiff[1]] = tdOld[x, y];
                        cropMap[x + sizeDiff[0], y + sizeDiff[1]] = true;
                    }
                }
            }
            foreach (var tex in tdOld.Textures)
            {
                map.Tiles.AddTexture(tex);
            }
            //--------------------
            //move objects
            //--------------------
            foreach (var obj in new List<ScriptObject>(map.Objects))
            {
                obj.X += sizeDiff[0] * 10;
                obj.Y += sizeDiff[1] * 10;

                //TODO: FIX THIS! (??)
                if (obj.X < -map.HeightMap.Border*10 || obj.Y < -map.HeightMap.Border * 10 || obj.X > (width - map.HeightMap.Border) * 10.0 || obj.Y > (height-map.HeightMap.Border) * 10.0)
                {
                    if ((obj.IsWaypoint && options.RemoveWaypoints) || (!obj.IsWaypoint && options.RemoveObjects))
                    {
                        map.Objects.Remove(obj);
                    }
                }
            }
            //-----------------
            //move areas
            //-----------------
            foreach (var a in map.Areas)
            {
                foreach (var point in a.Points)
                {
                    point[0] += sizeDiff[0] * 10;
                    point[1] += sizeDiff[1] * 10;
                }
            }
        }
    }
}
