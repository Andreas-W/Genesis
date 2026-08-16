#region License

// Tiles.cs
// Author: Daniel Sklenitzka
//
// Copyright 2013 The CWC Team
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace CkMp.Data.Map
{
    public class TileData : Grid<Tile>
    {
        public int NumberOfBaseTiles { get; set; }
        public int NumberOfBlendTiles { get; set; }

        public List<Texture> Textures { get; set; }
        public List<BlendTile> BlendTiles { get; set; }

        /// <summary>
        /// Stores all indices in the BlendTiles collection for a given tile index.
        /// </summary>
        private IDictionary<short, IList<int>> tileBlendTypes;

        /// <summary>
        /// How many entries of BlendTiles are covered by tileBlendTypes. Used to notice that the
        /// list was filled behind the lookup's back.
        /// </summary>
        private int indexedBlendTiles;

        public TileData(int width, int height) : base(width, height)
        {
            for (int i = 0; i < width * height; i++)
                this[i] = new Tile();

            this.Textures = new List<Texture>();
            this.BlendTiles = new List<BlendTile>();
            this.tileBlendTypes = new Dictionary<short, IList<int>>();
        }

        public int GetSize(bool includingHeader)
        {
            return 4
                 + (Width * Height) * 2 * 4
                 + Height * ((Width / 8) + (Width % 8 == 0 ? 0 : 1))
                 + 4
                 + 4
                 + 4
                 + 4
                 + this.Textures.Sum(t => t.GetSize())
                 + this.BlendTiles.Sum(b => b.GetSize())
                 + 8;
        }

        public Texture AddTexture(Texture texture)
        {
            var existing = Textures.FirstOrDefault(t => t.Name == texture.Name);
            if (existing != null)
                return existing;

            texture.BlockStartIndex = NumberOfBaseTiles;
            NumberOfBaseTiles += texture.BlockCount;
            Textures.Add(texture);
            return texture;
        }

        /// <summary>
        /// Gets the index of the blend tile. If the blend tile doesn't exist yet, it is added to the list.
        /// </summary>
        /// <param name="tileIndex">The tile index (in texture space).</param>
        /// <param name="blendType">The blend type.</param>
        /// <returns></returns>
        /// <summary>
        /// Rebuilds the tile index -> blend tile lookup from the current BlendTiles collection.
        /// Call this after replacing or filling BlendTiles directly instead of going through
        /// <see cref="GetBlendTileIndex"/>; without it the lookup is empty and every following
        /// call appends a duplicate instead of reusing the blend tile that is already there.
        /// </summary>
        public void RebuildBlendTileIndex()
        {
            tileBlendTypes.Clear();

            for (int i = 0; i < BlendTiles.Count; i++)
            {
                IList<int> indices;
                if (!tileBlendTypes.TryGetValue(BlendTiles[i].TileIndex, out indices))
                {
                    indices = new List<int>();
                    tileBlendTypes.Add(BlendTiles[i].TileIndex, indices);
                }

                indices.Add(i);
            }

            indexedBlendTiles = BlendTiles.Count;
        }

        public short GetBlendTileIndex(short tileIndex, BlendType blendType)
        {
            // The Reader and the map commands fill BlendTiles directly, which leaves the lookup
            // stale. Notice that and rebuild, otherwise nothing gets deduplicated from here on.
            if (indexedBlendTiles != BlendTiles.Count)
                RebuildBlendTileIndex();

            // Get the indices of all the blend tiles already defined for this tile index.
            IList<int> indices;
            if (!tileBlendTypes.TryGetValue(tileIndex, out indices))
            {
                // No blend tiles for this tile index yet. Add a new list to the dictionary.
                indices = new List<int>();
                tileBlendTypes.Add(tileIndex, indices);
            }

            // Check if one of the existing blend tiles has the same blend type. If yes, return it.
            foreach (int i in indices)
            {
                if (BlendTiles[i].BlendType == blendType)
                    return (short)(i + 1);
            }

            // The index is stored as a signed 16 bit value in the map file, so running past that
            // silently wraps into garbage. Fail loudly instead.
            if (BlendTiles.Count >= short.MaxValue)
                throw new InvalidOperationException(
                    "Too many blend tiles: a map cannot hold more than " + short.MaxValue + ".");

            // Otherwise, create the new blend tile and add it before returning the index.
            BlendTiles.Add(new BlendTile() { TileIndex = tileIndex, BlendType = blendType });
            NumberOfBlendTiles++;
            indices.Add(BlendTiles.Count - 1);
            indexedBlendTiles = BlendTiles.Count;

            return (short)BlendTiles.Count;
        }

        public Texture GetTexture(int i, int j)
        {
            return Textures.First(t => t.BlockStartIndex * 4 <= this[i, j].BaseTexture && (t.BlockStartIndex + t.BlockCount) * 4 > this[i, j].BaseTexture);
        }
    }
}
