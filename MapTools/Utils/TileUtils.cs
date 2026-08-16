using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CkMp.Data.Map;

namespace MapTools.Utils
{
    public class TileUtils
    {
        public static BlendType getBlendTypeMirrored(BlendType type, bool vertical)
        {
            if (vertical) //flip top/bottom
            {
                if (type == BlendType.Top) type = BlendType.Bottom;
                else if (type == BlendType.Bottom) type = BlendType.Top;
                else if (type == BlendType.TopLeftSmall) type = BlendType.BottomLeftSmall;
                else if (type == BlendType.TopLeftLarge) type = BlendType.BottomLeftLarge;
                else if (type == BlendType.TopRightSmall) type = BlendType.BottomRightSmall;
                else if (type == BlendType.TopRightLarge) type = BlendType.BottomRightLarge;
                else if (type == BlendType.BottomLeftSmall) type = BlendType.TopLeftSmall;
                else if (type == BlendType.BottomLeftLarge) type = BlendType.TopLeftLarge;
                else if (type == BlendType.BottomRightSmall) type = BlendType.TopRightSmall;
                else if (type == BlendType.BottomRightLarge) type = BlendType.TopRightLarge;
            }
            else //flip left/right
            {
                if (type == BlendType.Left) type = BlendType.Right;
                else if (type == BlendType.Right) type = BlendType.Left;
                else if (type == BlendType.TopLeftSmall) type = BlendType.TopRightSmall;
                else if (type == BlendType.TopLeftLarge) type = BlendType.TopRightLarge;
                else if (type == BlendType.TopRightSmall) type = BlendType.TopLeftSmall;
                else if (type == BlendType.TopRightLarge) type = BlendType.TopLeftLarge;
                else if (type == BlendType.BottomLeftSmall) type = BlendType.BottomRightSmall;
                else if (type == BlendType.BottomLeftLarge) type = BlendType.BottomRightLarge;
                else if (type == BlendType.BottomRightSmall) type = BlendType.BottomLeftSmall;
                else if (type == BlendType.BottomRightLarge) type = BlendType.BottomLeftLarge;
            }
            return type;
        }

        public static BlendType getBlendTypeRotated(BlendType type, byte rotations)
        {
            for (int i = 0; i < rotations; i++)
            {
                if (type == BlendType.Top) type = BlendType.Left;
                else if (type == BlendType.Bottom) type = BlendType.Right;
                else if (type == BlendType.Left) type = BlendType.Bottom;
                else if (type == BlendType.Right) type = BlendType.Top;
                else if (type == BlendType.TopLeftSmall) type = BlendType.BottomLeftSmall;
                else if (type == BlendType.TopLeftLarge) type = BlendType.BottomLeftLarge;
                else if (type == BlendType.TopRightSmall) type = BlendType.TopLeftSmall;
                else if (type == BlendType.TopRightLarge) type = BlendType.TopLeftLarge;
                else if (type == BlendType.BottomLeftSmall) type = BlendType.BottomRightSmall;
                else if (type == BlendType.BottomLeftLarge) type = BlendType.BottomRightLarge;
                else if (type == BlendType.BottomRightSmall) type = BlendType.TopRightSmall;
                else if (type == BlendType.BottomRightLarge) type = BlendType.TopRightLarge;
            }
            return type;
        }

        /// <summary>
        /// Moves a blend tile reference from (sourceX, sourceY) to (x, y).
        ///
        /// Both BlendTile.TileIndex and Tile.BaseTexture encode the position inside the tiled
        /// texture image, so a blend tile cannot simply be carried over: its tile index has to be
        /// recomputed for the target position, and its blend type run through
        /// <paramref name="transform"/> for the mirror or rotation being applied.
        ///
        /// Returns 0 (no blend) for an empty or out of range reference.
        /// </summary>
        public static short remapBlendTile(TileData source, TileData target, short blendIndex,
            int sourceX, int sourceY, int x, int y, Func<BlendType, BlendType> transform)
        {
            if (blendIndex <= 0 || blendIndex > source.BlendTiles.Count)
                return 0;

            BlendTile blendTile = source.BlendTiles[blendIndex - 1];

            // No texture owns this index at the source position. That happens on maps whose tiles
            // were moved without updating their indices, which is what Rotate and Mirror do by
            // default. Keep the index unchanged rather than dropping the blend tile: the map is
            // already misaligned, losing the blend on top of that would be worse.
            var texture = target.Textures.Find(t => t.GetTileIndex(sourceX, sourceY) == blendTile.TileIndex);
            short tileIndex = texture == null ? blendTile.TileIndex : texture.GetTileIndex(x, y);

            return target.GetBlendTileIndex(tileIndex, transform(blendTile.BlendType));
        }

        public static Grid<Texture> getTextureData(Map map)
        {
            var width = map.HeightMap.Width;
            var height = map.HeightMap.Height;
            var textureData = new Grid<Texture>(width + 1, height  + 1);

            for (int i = 0; i < width; i++ )
            {
                for (int j = 0; j < height; j++)
                {
                    var tex = map.Tiles.GetTexture(i, j);
                    textureData[i, j] = tex;

                    if (i == width - 1) textureData[i + 1, j] = tex;
                    if (j == height - 1) textureData[i, j + 1] = tex;
                    if (i == width - 1 && j == height - 1) textureData[i + 1, j + 1] = tex;
                }
            }

            return textureData;
        }
    }
}
