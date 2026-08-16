using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CkMp.Data;
using CkMp.Data.Map;
using MapTools.Utils;

namespace MapTools.Commands
{
    public class Rotate
    {
        /// <summary>
        /// Rotates the whole map left by 90 degrees, <paramref name="rcount"/> times.
        /// </summary>
        /// <param name="relayTextures">
        /// Off by default, which keeps every tile's texture index as it is. Tile.BaseTexture and
        /// BlendTile.TileIndex encode the position inside the tiled texture image, so carrying them
        /// over leaves the texture pattern misaligned with the rotated grid. Turn this on to
        /// recompute both for the target position, the way the Clone commands do.
        /// </param>
        public static void rotateMap(Map map, byte rcount, bool relayTextures = false)
        {
            for (int i = 0; i < rcount; i++)
            {
                rotateHeightMap(map);
                rotateTiles(map, relayTextures);
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

        private static void rotateTiles(Map map, bool relayTextures)
        {
            TileData td = map.Tiles;
            TileData td2 = new TileData(td.Height, td.Width);

            foreach (var tex in td.Textures)
            {
                td2.AddTexture(tex);
            }

            if (relayTextures)
            {
                rotateTilesRelayed(td, td2);
                map.Tiles = td2;
                return;
            }

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

            //Rotate BlendTiles
            foreach (var bt in td.BlendTiles)
            {
                td2.BlendTiles.Add(new BlendTile()
                {
                    TileIndex = bt.TileIndex,
                    BlendType = TileUtils.getBlendTypeRotated(bt.BlendType, 1)
                });
            }

            // The list was filled directly, so the deduplication lookup has to catch up before
            // anything else calls GetBlendTileIndex on this TileData.
            td2.RebuildBlendTileIndex();
        }

        /// <summary>
        /// Rotates the tiles and recomputes every texture index for its new position, so the
        /// texture pattern stays aligned with the grid.
        /// </summary>
        private static void rotateTilesRelayed(TileData td, TileData td2)
        {
            // NumberOfBlendTiles is always one more than the number of entries in the list.
            td2.NumberOfBlendTiles = 1;

            Func<BlendType, BlendType> rotate = t => TileUtils.getBlendTypeRotated(t, 1);

            for (int i = 0; i < td.Width; i++)
            {
                for (int j = 0; j < td.Height; j++)
                {
                    int x = td.Height - 1 - j;
                    int y = i;

                    var source = td[i, j];
                    var tile = new Tile()
                    {
                        BaseTexture = td.GetTexture(i, j).GetTileIndex(x, y),
                        Impassable = source.Impassable
                    };

                    tile.BlendTexture1 = TileUtils.remapBlendTile(td, td2, source.BlendTexture1, i, j, x, y, rotate);
                    if (tile.BlendTexture1 > 0)
                    {
                        tile.BlendTexture2 = TileUtils.remapBlendTile(td, td2, source.BlendTexture2, i, j, x, y, rotate);
                        if (tile.BlendTexture2 > 0)
                            tile.BlendTexture3 = TileUtils.remapBlendTile(td, td2, source.BlendTexture3, i, j, x, y, rotate);
                    }

                    td2[x, y] = tile;
                }
            }
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
