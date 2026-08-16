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
    public class Mirror
    {
        /// <summary>
        /// Mirrors the whole map along one axis.
        /// </summary>
        /// <param name="relayTextures">
        /// Off by default, which keeps every tile's texture index as it is. See
        /// <see cref="Rotate.rotateMap"/> for what turning it on does.
        /// </param>
        public static void mirror(Map map, bool vertical, bool relayTextures = false)
        {
            mirrorHeightMap(map, vertical);
            mirrorTiles(map, vertical, relayTextures);
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

        private static void mirrorTiles(Map map, bool vertical, bool relayTextures)
        {
            TileData td = map.Tiles;
            TileData td2 = new TileData(td.Width, td.Height);

            foreach (var tex in td.Textures)
            {
                td2.AddTexture(tex);
            }

            if (relayTextures)
            {
                mirrorTilesRelayed(td, td2, vertical);
                map.Tiles = td2;
                return;
            }

            //td2.Textures = td.Textures;
            //td2.NumberOfBaseTiles = td.NumberOfBaseTiles;
            td2.NumberOfBlendTiles = td.NumberOfBlendTiles;

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

            //Mirror BlendTiles
            foreach (var bt in td.BlendTiles)
            {
                td2.BlendTiles.Add(new BlendTile()
                {
                    TileIndex = bt.TileIndex,
                    BlendType = TileUtils.getBlendTypeMirrored(bt.BlendType, vertical)
                });
            }

            // The list was filled directly, so the deduplication lookup has to catch up before
            // anything else calls GetBlendTileIndex on this TileData.
            td2.RebuildBlendTileIndex();
        }

        /// <summary>
        /// Mirrors the tiles and recomputes every texture index for its new position, so the
        /// texture pattern stays aligned with the grid.
        /// </summary>
        private static void mirrorTilesRelayed(TileData td, TileData td2, bool vertical)
        {
            // NumberOfBlendTiles is always one more than the number of entries in the list.
            td2.NumberOfBlendTiles = 1;

            Func<BlendType, BlendType> mirror = t => TileUtils.getBlendTypeMirrored(t, vertical);

            for (int i = 0; i < td.Width; i++)
            {
                for (int j = 0; j < td.Height; j++)
                {
                    int x = vertical ? i : td.Width - 1 - i;
                    int y = vertical ? td.Height - 1 - j : j;

                    var source = td[i, j];
                    var tile = new Tile()
                    {
                        BaseTexture = td.GetTexture(i, j).GetTileIndex(x, y),
                        Impassable = source.Impassable
                    };

                    tile.BlendTexture1 = TileUtils.remapBlendTile(td, td2, source.BlendTexture1, i, j, x, y, mirror);
                    if (tile.BlendTexture1 > 0)
                    {
                        tile.BlendTexture2 = TileUtils.remapBlendTile(td, td2, source.BlendTexture2, i, j, x, y, mirror);
                        if (tile.BlendTexture2 > 0)
                            tile.BlendTexture3 = TileUtils.remapBlendTile(td, td2, source.BlendTexture3, i, j, x, y, mirror);
                    }

                    td2[x, y] = tile;
                }
            }
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
