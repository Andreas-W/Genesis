using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CkMp.Data;
using CkMp.Data.Map;

namespace MapTools.Commands
{
    public class Mirror
    {
        public static void mirror(Map map, bool vertical)
        {
            mirrorHeightMap(map, vertical);
            mirrorTiles(map, vertical);
            mirrorObjects(map, vertical);
            mirrorAreas(map, vertical);
        }

        private static void mirrorHeightMap(Map map, bool vertical)
        {
            HeightMap hm = map.HeightMap;
            HeightMap hm2 = new HeightMap(hm.Width, hm.Height, map.HeightMap.Border);


            for (int i = 0; i < hm.Width; i++)
            {
                for (int j = 0; j < hm.Height; j++)
                {
                    if (vertical) //flip y
                    {
                        hm2[i, hm.Height - 1 - j] = hm[i, j];
                    }
                    else //flip x
                    {
                        hm2[hm.Width - 1 - i, j] = hm[i, j];
                    }
                }
            }
            map.HeightMap = hm2;
        }

        private static void mirrorTiles(Map map, bool vertical)
        {
            TileData td = map.Tiles;
            TileData td2 = new TileData(td.Width, td.Height);

            //td2.Textures = td.Textures;
            //td2.NumberOfBaseTiles = td.NumberOfBaseTiles;
            td2.NumberOfBlendTiles = td.NumberOfBlendTiles;

            //Rotate left: x -> y; y -> -x

            for (int i = 0; i < td.Width; i++)
            {
                for (int j = 0; j < td.Height; j++)
                {
                    if (vertical) //flip y
                    {
                        td2[i, td.Height - 1 - j] = td[i, j];
                    }
                    else //flip x
                    {
                        td2[td.Width - 1 - i, j] = td[i, j];
                    }
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
                BlendType type = bt.BlendType;
                if (vertical) //flip top/bottom
                {
                    if (bt.BlendType == BlendType.Top) type = BlendType.Bottom;
                    else if (bt.BlendType == BlendType.Bottom) type = BlendType.Top;                 
                    else if (bt.BlendType == BlendType.TopLeftSmall) type = BlendType.BottomLeftSmall;
                    else if (bt.BlendType == BlendType.TopLeftLarge) type = BlendType.BottomLeftLarge;
                    else if (bt.BlendType == BlendType.TopRightSmall) type = BlendType.BottomRightSmall;
                    else if (bt.BlendType == BlendType.TopRightLarge) type = BlendType.BottomRightLarge;
                    else if (bt.BlendType == BlendType.BottomLeftSmall) type = BlendType.TopLeftSmall;
                    else if (bt.BlendType == BlendType.BottomLeftLarge) type = BlendType.TopLeftLarge;
                    else if (bt.BlendType == BlendType.BottomRightSmall) type = BlendType.TopRightSmall;
                    else if (bt.BlendType == BlendType.BottomRightLarge) type = BlendType.TopRightLarge;
                }
                else //flip left/right
                {
                    if (bt.BlendType == BlendType.Left) type = BlendType.Right;
                    if (bt.BlendType == BlendType.Right) type = BlendType.Left;
                    else if (bt.BlendType == BlendType.TopLeftSmall) type = BlendType.TopRightSmall;
                    else if (bt.BlendType == BlendType.TopLeftLarge) type = BlendType.TopRightLarge;
                    else if (bt.BlendType == BlendType.TopRightSmall) type = BlendType.TopLeftSmall;
                    else if (bt.BlendType == BlendType.TopRightLarge) type = BlendType.TopLeftLarge;
                    else if (bt.BlendType == BlendType.BottomLeftSmall) type = BlendType.BottomRightSmall;
                    else if (bt.BlendType == BlendType.BottomLeftLarge) type = BlendType.BottomRightLarge;
                    else if (bt.BlendType == BlendType.BottomRightSmall) type = BlendType.BottomLeftSmall;
                    else if (bt.BlendType == BlendType.BottomRightLarge) type = BlendType.BottomLeftLarge;
                }
               
                //td2.GetBlendTileIndex(bt.TileIndex, type);
                td2.BlendTiles.Add(new BlendTile() { TileIndex = bt.TileIndex, BlendType = type });
            }
            //td2.BlendTiles = td.BlendTiles;
        }

        private static void mirrorObjects(Map map, bool vertical)
        {
            //old center
            float centerX = map.HeightMap.MapWidth * 0.5f * 10.0f;
            float centerY = map.HeightMap.MapHeight * 0.5f * 10.0f;
            foreach (var obj in map.Objects)
            {
                if (vertical) //flip y
                {
                    if (!obj.IsWaypoint)
                    {
                        obj.Rotation = ((float)Math.PI * 2.0f) - obj.Rotation;
                    }
                    float dy = obj.Y - centerY;

                    obj.Y = centerY - dy;
                }else //flip x
                {
                    if (!obj.IsWaypoint)
                    {
                        obj.Rotation = ((float)Math.PI) - obj.Rotation;
                        if (obj.Rotation < 0) obj.Rotation += ((float)Math.PI * 2.0f);
                    }
                    float dx = obj.X - centerX;
                    obj.X = centerX - dx;
                }
                
            }
        }

        private static void mirrorAreas(Map map, bool vertical)
        {
            //old center
            float centerX = map.HeightMap.MapWidth * 0.5f * 10.0f;
            float centerY = map.HeightMap.MapHeight * 0.5f * 10.0f;

            foreach (var a in map.Areas)
            {
                foreach (var point in a.Points)
                {
                    if (vertical) //flip y
                    {
                        float dy = (float)point[1] - centerY;
                        point[1] = Convert.ToInt32(-dy + centerY);
                    }
                    else //flip x
                    {
                        float dx = (float)point[0] - centerX;
                        point[0] = Convert.ToInt32(-dx + centerX);
                    }
                }
            }
        }

    }
}
