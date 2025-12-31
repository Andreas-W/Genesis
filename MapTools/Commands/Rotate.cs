using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CkMp.Data;
using CkMp.Data.Map;

namespace MapTools.Commands
{
    public class Rotate
    {
        public static void rotateMap(Map map, byte rcount)
        {
            for (int i = 0; i < rcount; i++)
            {
                rotateHeightMap(map);
                rotateTiles(map);
                rotateObjects(map);
                rotateAreas(map);
            }
        }

        private static void rotateHeightMap(Map map)
        {
            HeightMap hm = map.HeightMap;
            HeightMap hm2 = new HeightMap( hm.Height, hm.Width, map.HeightMap.Border);
            
            //Rotate left: x -> y; y -> -x

            for (int i = 0; i < hm.Width; i++)
            {
                for (int j = 0; j < hm.Height; j++)
                {
                    hm2[hm.Height-1-j, i] = hm[i, j];
                }
            }
            map.HeightMap = hm2;
        }

        private static void rotateTiles(Map map)
        {
            TileData td = map.Tiles;
            TileData td2 = new TileData(td.Height, td.Width);

            //td2.Textures = td.Textures;
            //td2.NumberOfBaseTiles = td.NumberOfBaseTiles;
            td2.NumberOfBlendTiles = td.NumberOfBlendTiles;
            

            //Rotate left: x -> y; y -> -x

            for (int i = 0; i < td.Width; i++)
            {
                for (int j = 0; j < td.Height; j++)
                {
                    td2[td.Height - 1 - j, i] = td[i, j];
                }
            }
            map.Tiles = td2;

            foreach (var tex in td.Textures)
            {
                td2.AddTexture(tex);
            }

            //Rotate BlendTiles
            foreach (var bt in td.BlendTiles)
            {
                BlendType type = BlendType.Top;
                if (bt.BlendType == BlendType.Top) type = BlendType.Left;
                else if (bt.BlendType == BlendType.Bottom) type = BlendType.Right;
                else if (bt.BlendType == BlendType.Left) type = BlendType.Bottom;
                else if (bt.BlendType == BlendType.Right) type = BlendType.Top;
                else if (bt.BlendType == BlendType.TopLeftSmall) type = BlendType.BottomLeftSmall;
                else if (bt.BlendType == BlendType.TopLeftLarge) type = BlendType.BottomLeftLarge;
                else if (bt.BlendType == BlendType.TopRightSmall) type = BlendType.TopLeftSmall;
                else if (bt.BlendType == BlendType.TopRightLarge) type = BlendType.TopLeftLarge;
                else if (bt.BlendType == BlendType.BottomLeftSmall) type = BlendType.BottomRightSmall;
                else if (bt.BlendType == BlendType.BottomLeftLarge) type = BlendType.BottomRightLarge;
                else if (bt.BlendType == BlendType.BottomRightSmall) type = BlendType.TopRightSmall;
                else if (bt.BlendType == BlendType.BottomRightLarge) type = BlendType.TopRightLarge;
                //td2.GetBlendTileIndex(bt.TileIndex, type);
                td2.BlendTiles.Add(new BlendTile() { TileIndex = bt.TileIndex, BlendType = type });
            }
            //td2.BlendTiles = td.BlendTiles;
        }

        private static void rotateObjects(Map map)
        {
            //old center
            float centerX = map.HeightMap.MapHeight * 0.5f * 10.0f;
            float centerY = map.HeightMap.MapWidth * 0.5f * 10.0f;
            foreach (var obj in map.Objects)
            {
                if (!obj.IsWaypoint)
                {
                    obj.Rotation = (obj.Rotation + (float)Math.PI * 0.5f) % ((float)Math.PI * 2.0f);
                }
                float dx = obj.X - centerX;
                float dy = obj.Y - centerY; 

                float x = dx;
                obj.X = -dy + centerY;
                obj.Y = x + centerX;
            }
        }

        private static void rotateAreas(Map map)
        {
            //old center
            float centerX = map.HeightMap.MapHeight * 0.5f * 10.0f;
            float centerY = map.HeightMap.MapWidth * 0.5f * 10.0f;

            foreach (var a in map.Areas)
            {
                foreach (var point in a.Points)
                {
                    float dx = (float)point[0] - centerX;
                    float dy = (float)point[1] - centerY;

                    float x = dx;
                    point[0] = Convert.ToInt32(-dy + centerY);
                    point[1] = Convert.ToInt32(x + centerX);
                }
            }
        }

    }
}
