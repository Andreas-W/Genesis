using CkMp.Data;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapTools.MapMerge
{
    public class MapMerger
    {
        public MergeMapInfo Map1;
        public MergeMapInfo Map2;

        Grid<Texture> textureMap1;
        Grid<Texture> textureMap2;

        public int MergedWidth;
        public int MergedHeight;
        public int MergedBorder;

        public MergeMapInfo openMap()
        {
            var openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "map files (*.map)|*.map|All files (*.*)|*.*";

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;
                var reader = new Reader();
                reader.ReadFile(filename);
                MergeMapInfo mapInfo = new MergeMapInfo()
                {
                    Map = reader.Map,
                    MapName = Path.GetFileName(filename)
                };
                return mapInfo;
            }
            return null;
        }

        public MergeMapInfo openMap(string filename)
        {
            var reader = new Reader();
            reader.ReadFile(filename);
            MergeMapInfo mapInfo = new MergeMapInfo()
            {
                Map = reader.Map,
                MapName = Path.GetFileName(filename)
            };
            return mapInfo;
        }


        public Map MergeMaps()
        {
            Map result = new Map(MergedWidth, MergedHeight, MergedBorder);

            //-- Prepare maps --
            PrepareMap(Map1);
            textureMap1 = getMapTextures(Map1);
            PrepareMap(Map2);
            textureMap2 = getMapTextures(Map2);

            saveTempMap(Map1.Map, "temp_mergeMap1.map");
            saveTempMap(Map2.Map, "temp_mergeMap2.map");

            //- Merge --

            //default mode: first map 1, then map 2 (overwrite)

            //1. Merge Height
            MergeHeightMap(result);
            //2. Merge Tiles
            MergeTiles(result);
            //2. Merge Objects
            MergeObjects(result);
            //3. Merge Areas
            MergeAreas(result);

            result.GlobalLightOptions = Map1.Map.GlobalLightOptions;
            //result.Players.Add(Map1.Map.Players);
            //result.PlayerScripts = Map1.Map.PlayerScripts;
            result.Properties = Map1.Map.Properties;

            return result;
        }

        private Grid<Texture> getMapTextures(MergeMapInfo map)
        {
            Grid<Texture> textures = new Grid<Texture>(map.Map.HeightMap.Width, map.Map.HeightMap.Height);
            for (int i = 0; i < map.Map.HeightMap.Width; i++)
            {
                for (int j = 0; j < map.Map.HeightMap.Height; j++)
                {
                    textures[i,j] = map.Map.Tiles.GetTexture(i, j);
                }
            }
            return textures;
        }

        private void PrepareMap(MergeMapInfo map)
        {
            //clear blend tiles
            map.Map.Tiles.BlendTiles.Clear();
            map.Map.Tiles.NumberOfBlendTiles = 1;
            for (int i = 0; i < map.Map.Tiles.Width * map.Map.Tiles.Height; i++)
            {
                map.Map.Tiles[i].BlendTexture1 = 0;
                map.Map.Tiles[i].BlendTexture2 = 0;
                map.Map.Tiles[i].BlendTexture3 = 0;
            }

            int xOffset = 0, yOffset = 0;
            int startX = map.Crop[map.Rotation % 4];
            int startY = map.Crop[(map.Rotation + 1) % 4];

            //TODO: Pre-Crop map
            //Actually, no need to crop, instead just set the crop map
            //Note: This happens first, so we need to also flip/rotate the crop map next
            Grid<bool> cropMap = Crop(map);
            if (map.Rotation > 0)
            {
                Commands.Rotate.rotateMap(map.Map, map.Rotation);
                if (map.Rotation % 2 >= 0)
                {
                    xOffset = ((map.Map.HeightMap.Height / 2) - (map.Map.HeightMap.Width / 2));
                    yOffset = ((map.Map.HeightMap.Width / 2) - (map.Map.HeightMap.Height / 2));
                }
                cropMap = cropMap.RotateLeft(map.Rotation);
            }
            if (map.MirroredHorizontal)
            {
                Commands.Mirror.mirror(map.Map, false);
                cropMap = cropMap.FlipHorizontal();
                startX = map.Crop[(3-map.Rotation) % 4];

            }
            if (map.MirroredVertical)
            {
                Commands.Mirror.mirror(map.Map, true);
                cropMap = cropMap.FlipVertical();
                startY = map.Crop[(3 - map.Rotation+1) % 4];
            }

            xOffset += map.OffsetX;
            yOffset += map.OffsetY;

            int x2 = MergedWidth - xOffset - map.Map.HeightMap.Width;
            int y2 = MergedHeight - yOffset - map.Map.HeightMap.Height;
            var options = new Options.ResizeOptions() { RemoveObjects = false };
            Grid<bool> cropMap2;
            Commands.Resize.resize(map.Map, new int[] {xOffset, yOffset, x2, y2}, options, out cropMap2);
            MergeCropMaps(cropMap2, cropMap, xOffset, yOffset);
            map.CropMap = cropMap2;

            map.startX= startX+ xOffset;
            map.startY= startY+ yOffset;
            //map.sizeX = map.Map.HeightMap.Width; //TODO
            //map.sizeY = map.Map.HeightMap.Height; //TODO
            computeCroppedMapSize(map);


            //TODO: FIX CROP MAP AFTER ROTATION (RIGHT)
            //TODO: ROTATION IS WEIRD (rotates in wrong dir?)
        }

        private Grid<bool> Crop(MergeMapInfo map)
        {
            //We resize twice and use the second cropmap
            var options = new Options.ResizeOptions();
            options.RemoveObjects = false;
            Grid<bool> cropMap;
            Commands.Resize.resize(map.Map, new int[] { -map.Crop[0], -map.Crop[1], -map.Crop[2], -map.Crop[3] }, options, out cropMap);
            Commands.Resize.resize(map.Map, map.Crop, options, out cropMap);
            return cropMap;
        }

        //map1 = larger map
        //apply AND operation starting from offset
        private void MergeCropMaps(Grid<bool> map1, Grid<bool> map2, int xoffset, int yoffset)
        {
            for (int x = 0; x < map2.Width; x++)
            {
                for (int y = 0; y < map2.Height; y++)
                {
                    if (x + xoffset >= 0 && y + yoffset >= 0 && x + xoffset < MergedWidth && y + yoffset < MergedHeight)
                    {
                        map1[x + xoffset, y + yoffset] = map1[x + xoffset, y + yoffset] && map2[x, y];
                    }
                }
            }

        }

        private void MergeHeightMap(Map result)
        {
            //Heightmap
            for (int x = 0; x <MergedWidth; x++)
            {
                for (int y = 0; y < MergedHeight; y++)
                {
                    if (Map2.CropMap[x,y])
                    {
                        result.HeightMap[x, y] = Map2.Map.HeightMap[x, y];
                    }
                    else if (Map1.CropMap[x,y])
                    {
                        result.HeightMap[x, y] = Map1.Map.HeightMap[x, y];
                    }
                }
            }
        }

        private void MergeTiles(Map result)
        {
            result.Tiles = new TileData(MergedWidth, MergedHeight);

            //Heightmap
            for (int x = 0; x < MergedWidth; x++)
            {
                for (int y = 0; y < MergedHeight; y++)
                {
                    if (Map2.CropMap[x, y])
                    {
                        addTile(result, Map2.Map, x, y, textureMap2);
                    }
                    else if (Map1.CropMap[x, y])
                    {
                        addTile(result, Map1.Map, x, y, textureMap1);
                    }
                }
            }
        }

        private void MergeObjects(Map result)
        {
            //TODO: Fix objects when mirrored??

            //1. Add all objects from Map2
            //2. Add all objects from Map1 that are not on Map2 tiles
            bool skipNext = false;
            foreach (var o in Map2.Map.Objects)
            {
                if (skipNext && !o.RoadOptions.HasFlag(RoadOptions.End))
                {
                    Console.Error.WriteLine("Error! Road End expected!");
                }
                else if(!skipNext && o.RoadOptions.HasFlag(RoadOptions.End))
                {
                    continue;
                }

                float d = (-result.HeightMap.Border + Map2.Map.HeightMap.Border) * 10f;

                if (skipNext || !(o.X + Map2.Map.HeightMap.Border * 10 < Map2.startX * 10
                     || o.Y + Map2.Map.HeightMap.Border * 10 < Map2.startY * 10
                     || o.X + Map2.Map.HeightMap.Border * 10 > (Map2.startX + Map2.sizeX) * 10
                     || o.Y + Map2.Map.HeightMap.Border * 10 > (Map2.startY + Map2.sizeY) * 10))
                {
                    o.X += d;
                    o.Y += d;
                    result.Objects.Add(o);

                    skipNext = false;
                    if (o.RoadOptions.HasFlag(RoadOptions.Start))
                    {
                        skipNext = true;
                    }

                }
            }
            skipNext = false;
            foreach (var o in Map1.Map.Objects)
            {
                if (skipNext && !o.RoadOptions.HasFlag(RoadOptions.End))
                {
                    Console.Error.WriteLine("Error! Road End expected!");
                }
                else if (!skipNext && o.RoadOptions.HasFlag(RoadOptions.End))
                {
                    continue;
                }

                float d = (-result.HeightMap.Border + Map1.Map.HeightMap.Border) * 10f;

                if (skipNext || o.X + Map1.Map.HeightMap.Border * 10 < Map2.startX * 10
                     || o.Y + Map1.Map.HeightMap.Border * 10 < Map2.startY * 10
                     || o.X + Map1.Map.HeightMap.Border * 10 > (Map2.startX + Map2.sizeX) * 10
                     || o.Y + Map1.Map.HeightMap.Border * 10 > (Map2.startY + Map2.sizeY) * 10)
                {

                    o.X += d;
                    o.Y += d;
                    result.Objects.Add(o);

                    skipNext = false;
                    if (o.RoadOptions.HasFlag(RoadOptions.Start))
                    {
                        skipNext = true;
                    }
                }
            }

        }

        private void MergeAreas(Map result)
        {
            //1. Add all areas from Map2
            //2. Add all areas from Map1 that are not on Map2 tiles

            float d = (-result.HeightMap.Border + Map2.Map.HeightMap.Border) * 10f;
            foreach (var a in Map2.Map.Areas)
            {
                bool add = false;
                foreach (var p in a.Points) {

                    if (!(p[0] + Map2.Map.HeightMap.Border * 10 < Map2.startX * 10
                         || p[1] + Map2.Map.HeightMap.Border * 10 < Map2.startY * 10
                         || p[0] + Map2.Map.HeightMap.Border * 10 > (Map2.startX + Map2.sizeX) * 10
                         || p[1]+ Map2.Map.HeightMap.Border * 10 > (Map2.startY + Map2.sizeY) * 10))
                    {
                        add = true; //add area if at least one point is in correct area
                    }
                    p[0] += Convert.ToInt32(d);
                    p[1] += Convert.ToInt32(d);   
                }
                if (add)
                {
                    result.Areas.Add(a);
                }
            }

            d = (-result.HeightMap.Border + Map1.Map.HeightMap.Border) * 10f;
            foreach (var a in Map1.Map.Areas)
            {
                bool add = false;
                foreach (var p in a.Points)
                {

                    if (p[0] + Map2.Map.HeightMap.Border * 10 < Map2.startX * 10
                         || p[1] + Map2.Map.HeightMap.Border * 10 < Map2.startY * 10
                         || p[0] + Map2.Map.HeightMap.Border * 10 > (Map2.startX + Map2.sizeX) * 10
                         || p[1] + Map2.Map.HeightMap.Border * 10 > (Map2.startY + Map2.sizeY) * 10)
                    {
                        add = true; //add area if at least one point is in correct area
                    }
                    p[0] += Convert.ToInt32(d);
                    p[1] += Convert.ToInt32(d);
                }
                if (add)
                {
                    result.Areas.Add(a);
                }
            }

        }

        private void addTile(Map result, Map sourceMap, int x, int y, Grid<Texture> srcTextures)
        {
            short texindex = 0;


            Texture tex = srcTextures[x, y];//sourceMap.Tiles.GetTexture(x, y);

            tex = result.Tiles.AddTexture(tex);
            texindex = tex.GetTileIndex(x, y);
          

            result.Tiles[x, y] = new Tile()
            {
                BaseTexture = texindex,
                //Blend textures are ignored for now
                Impassable = Map2.Map.Tiles[x, y].Impassable
            };
        }

        private void computeCroppedMapSize(MergeMapInfo mapInfo)
        {
            bool b = false;
            for (int i = mapInfo.startX; i < mapInfo.Map.HeightMap.Width; i++)
            {
                if (!mapInfo.CropMap[i, mapInfo.startY])
                {
                    mapInfo.sizeX = i - mapInfo.startX;
                    b = true;
                    break;
                }
            }
            if (!b) mapInfo.sizeX = mapInfo.Map.HeightMap.Width - mapInfo.startX;

            b = false;
            for (int i = mapInfo.startY; i < mapInfo.Map.HeightMap.Height; i++)
            {
                if (!mapInfo.CropMap[mapInfo.startX, i])
                {
                    mapInfo.sizeY = i - mapInfo.startY;
                    b = true;
                    break;
                }
            }
            if (!b) mapInfo.sizeY = mapInfo.Map.HeightMap.Height - mapInfo.startY;
        }

        private void saveTempMap(Map map, string filename)
        {
            string PATH = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "..\\..\\..\\..\\Test\\MapMerge\\";
            var writer = new Writer();
            writer.Map = map;
            writer.WriteFile(PATH+filename);
        }
    }

}
