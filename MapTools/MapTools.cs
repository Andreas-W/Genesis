using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

using CkMp.Data;
using CkMp.Data.Compression;
using CkMp.Data.Map;

using Genesis.Processors;
using Genesis.Core;
using Genesis.Settings;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using MapTools.Utils;

namespace MapTools
{
    public class MapTools
    {


        static MapLayout defaultLayout;
        static Scenery defaultScenery;

        //static List<MapLayout> mapLayouts;
        //static List<Scenery> sceneries;

        public static Map map = null;
        public static MapInfo info = null;

        public static string mapName = "";

        /// <summary>
        /// Compression of the currently loaded map, also used as the default when saving.
        /// </summary>
        public static MapCompressionType compression = MapCompressionType.None;

        [STAThread]
        static void Main(string[] args)
        {

            //string testdir = "../../../Test/";
            //string mapname = "test3";
            ////string fileName = "../../../Test/tournament desert op_fs/tournament desert op_fs.map";
            ////var reader = new Reader();
            ////reader.ReadFile(fileName);

            //openMap();


            //return;

            //Random r = new Random();

            //var map = new Map(256, 256, 16);

            //for (int j = 0; j < map.HeightMap.Height; j++)
            //{
            //    for (int i = 0; i < map.HeightMap.Width; i++)
            //    {
            //        map.HeightMap[i, j] = (byte)r.Next(256);
            //    }
            //}
            //var openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Filter = "BMP|*.bmp|PNG|*.png|All files (*.*)|*.*";

            //DialogResult result = openFileDialog1.ShowDialog();
            //if (result == DialogResult.OK) // Test result.
            //{
            //    int borderWidth = 0;
            //    string promptValue = Prompt.ShowDialog("Enter map border Width:", "Border Width");
            //    if (!Int32.TryParse(promptValue, out borderWidth)) borderWidth = 16;

            //    mapFromHeightmapFile(openFileDialog1.FileName, borderWidth);

            //    saveMap();
            //}

        

        }


        public static void initialize()
        {
            defaultLayout = new MapLayout();
            defaultLayout.Name = "Default";
            defaultLayout.Height = 40;
            defaultLayout.TerrainLayers = new List<TerrainLayer>();
            defaultLayout.TerrainLayers.Add(new TerrainLayer() { Name = "Hills", Height = 200, Elevation = 1.3f, Jaggedness = 0.005f, Ruggedness = 0.25f, LevelOfDetail = 3 });
            defaultLayout.TerrainLayers.Add(new TerrainLayer() { Name = "Depressions", Height = -40, Elevation = 1.22f, Jaggedness = 0.014f, Ruggedness = 0.7f, LevelOfDetail = 2});
            defaultLayout.TerrainLayers.Add(new TerrainLayer() { Name = "Bumps", Height = 13, Elevation = 0.79f, Jaggedness = 0.038f, Ruggedness = 0.8f, LevelOfDetail = 3, IsDetail = true });

            //<MapLayout Name="Default" Height="40">
            //<TerrainLayers>
            //  <TerrainLayer Name="Hills" Height="200" Elevation="1.3" Jaggedness="0.005" Ruggedness="0.25" LevelOfDetail="3"/>
            //  <TerrainLayer Name="Depressions" Height="-40" Elevation="1.22" Jaggedness="0.014" Ruggedness="0.7" LevelOfDetail="2"/>
            //  <TerrainLayer Name="Bumps" Height="13" Elevation="0.79" Jaggedness="0.038" Ruggedness="0.8" LevelOfDetail="3" IsDetail="true"/>


            //Grassy
            //defaultScenery = new Scenery();
            //defaultScenery.BaseTextureData = new TextureData(){Name="GrassLargeType1", Size=384};
            //defaultScenery.OverlayTextures = new List<TextureData>();
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "GrassMediumType22", Size = 256, Granularity = 0.05f });
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "GrassMediumType6", Size = 256, Granularity = 0.07f });
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "GrassMediumType5", Size = 256, Granularity = 0.1f });
            //defaultScenery.CliffTextureData = new TextureData() { Name = "CliffMediumType15b", Size = 256 };
            //defaultScenery.CliffOverlayTextures = new List<TextureData>();
            //defaultScenery.CliffOverlayTextures.Add(new TextureData() {Name = "CliffMediumType17", Size=256, Granularity=0.23f });

            //RockyCliff Grassy Island
            //defaultScenery = new Scenery();
            //defaultScenery.BaseTextureData = new TextureData() { Name = "GrassMediumType7", Size = 256 };
            //defaultScenery.OverlayTextures = new List<TextureData>();
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "Grass02", Size = 640, Granularity = 0.05f });
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "GrassMediumType30", Size = 256, Granularity = 0.07f });
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "GrassMediumType10", Size = 256, Granularity = 0.1f });
            //defaultScenery.CliffTextureData = new TextureData() { Name = "CliffMediumType3", Size = 256 };
            //defaultScenery.CliffOverlayTextures = new List<TextureData>();
            //defaultScenery.CliffOverlayTextures.Add(new TextureData() { Name = "RocksMediumType1Snow2", Size = 256, Granularity = 0.15f });

            //Sand and Cliffs only
            //defaultScenery = new Scenery();
            //defaultScenery.BaseTextureData = new TextureData() { Name = "DirtMediumType11", Size = 256 };
            //defaultScenery.OverlayTextures = new List<TextureData>();
            ////defaultScenery.OverlayTextures.Add(new TextureData() { Name = "Grass02", Size = 640, Granularity = 0.05f });
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "GrassMediumType23brown", Size = 256, Granularity = 0.05f });
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "RocksMediumType1", Size = 256, Granularity = 0.1f });
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "RocksMediumType1Snow2", Size = 256, Granularity = 0.07f });
            //defaultScenery.CliffTextureData = new TextureData() { Name = "CliffMediumType3", Size = 256 };
            //defaultScenery.CliffOverlayTextures = new List<TextureData>();
            ////defaultScenery.CliffOverlayTextures.Add(new TextureData() { Name = "RocksMediumType1Snow2", Size = 256, Granularity = 0.07f });
            ////defaultScenery.CliffOverlayTextures.Add(new TextureData() { Name = "RocksType1", Size = 128, Granularity = 0.05f });


            //Asphalt and cliffs
            //defaultScenery = new Scenery();
            //defaultScenery.BaseTextureData = new TextureData() { Name = "ConcreteType6", Size = 128 };
            //defaultScenery.OverlayTextures = new List<TextureData>();
            //defaultScenery.CliffTextureData = new TextureData() { Name = "ConcreteType4", Size = 128 };
            //defaultScenery.CliffOverlayTextures = new List<TextureData>();


            //Desert Canyon
            //defaultScenery = new Scenery();
            //defaultScenery.BaseTextureData = new TextureData() { Name = "SandMediumType7", Size = 256 };
            //defaultScenery.OverlayTextures = new List<TextureData>();
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "SandLargeType3", Size = 384, Granularity = 0.05f });
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "SandMediumType10", Size = 256, Granularity = 0.05f });
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "DirtMediumType9", Size = 256, Granularity = 0.1f });
            ////defaultScenery.OverlayTextures.Add(new TextureData() { Name = "SandMediumType8", Size = 256, Granularity = 0.1f });
            //defaultScenery.OverlayTextures.Add(new TextureData() { Name = "SandMediumType7Rocky", Size = 256, Granularity = 0.07f });
            //defaultScenery.CliffTextureData = new TextureData() { Name = "CliffMediumType6b", Size = 256 };
            //defaultScenery.CliffOverlayTextures = new List<TextureData>();
            //defaultScenery.CliffOverlayTextures.Add(new TextureData() { Name = "CliffLargeType13", Size = 384, Granularity = 0.07f });
            //defaultScenery.CliffOverlayTextures.Add(new TextureData() { Name = "CliffMediumType5b", Size = 256, Granularity = 0.05f });


            //TODO: Tropical Island
            defaultScenery = new Scenery();
            defaultScenery.BaseTextureData = new TextureData() { Name = "SandMediumType13", Size = 256 };
            defaultScenery.OverlayTextures = new List<TextureData>();
            defaultScenery.OverlayTextures.Add(new TextureData() { Name = "SandMediumType13b", Size = 256, Granularity = 0.1f });
            defaultScenery.OverlayTextures.Add(new TextureData() { Name = "SandMediumType13c", Size = 256, Granularity = 0.05f });
            defaultScenery.OverlayTextures.Add(new TextureData() { Name = "SandMediumType13d", Size = 256, Granularity = 0.05f });
            defaultScenery.OverlayTextures.Add(new TextureData() { Name = "SandMediumType13grassy4", Size = 256, Granularity = 0.05f });
            defaultScenery.OverlayTextures.Add(new TextureData() { Name = "SandLargeType3Light", Size = 384, Granularity = 0.05f });
            defaultScenery.OverlayTextures.Add(new TextureData() { Name = "SandMediumType6Light", Size = 256, Granularity = 0.05f });
            
            
            defaultScenery.CliffTextureData = new TextureData() { Name = "CliffLargeType1", Size = 384 };

            defaultScenery.CliffOverlayTextures = new List<TextureData>();
            defaultScenery.CliffOverlayTextures.Add(new TextureData() { Name = "CliffMediumType1", Size = 256, Granularity = 0.07f });
            defaultScenery.CliffOverlayTextures.Add(new TextureData() { Name = "CliffMediumType16", Size = 256, Granularity = 0.05f });
            defaultScenery.CliffOverlayTextures.Add(new TextureData() { Name = "CliffMediumType15", Size = 256, Granularity = 0.05f });
            //defaultScenery.CliffOverlayTextures.Add(new TextureData() { Name = "RocksType3", Size = 128, Granularity = 0.025f });


            defaultScenery.WaterTextureData = new TextureData() { Name = "SandMediumType9blue", Size = 256 };
            defaultScenery.WaterOverlayTextures = new List<TextureData>();
            defaultScenery.WaterOverlayTextures.Add(new TextureData() { Name = "SandMediumType9blue2", Size = 256, Granularity = 0.07f });
            defaultScenery.WaterOverlayTextures.Add(new TextureData() { Name = "SandMediumType9blue3", Size = 256, Granularity = 0.04f });



            //<BaseTexture Name="GrassLargeType1" Size="384"/>
            //  <OverlayTextures>
            //    <Texture Name="GrassMediumType22" Size="256" Granularity="0.05"/>
            //    <Texture Name="GrassMediumType6" Size="256" Granularity="0.07"/>
            //    <Texture Name="GrassMediumType5" Size="256" Granularity="0.1"/>
            //  </OverlayTextures>
            //  <CliffTexture Name="CliffMediumType15b" Size="256"/>
            //  <CliffOverlayTextures>
            //    <Texture Name="CliffMediumType17" Size="256" Granularity="0.23"/>
            //  </CliffOverlayTextures>


            //var serializer = new XmlSerializer(typeof(MapLayout[]), new XmlRootAttribute("MapLayouts"));
            //var layouts = serializer.Deserialize(File.OpenRead("../Genesis.UI/MapLayouts.xml")) as MapLayout[];
            //mapLayouts = layouts.ToList();

            //serializer = new XmlSerializer(typeof(Scenery));
            //var sceneries_ = Directory.GetFiles("../Genesis.UI/Sceneries", "*.xml").Select(f =>
            //{
            //    try
            //    {
            //        return (Scenery)serializer.Deserialize(File.OpenRead(f));
            //    }
            //    catch
            //    {
            //        return default(Scenery);
            //    }
            //});

            //sceneries = sceneries_.ToList();

        }


        static void mapFromHeightmapFile(string filename, int borderWidth)
        {
            Bitmap img = new Bitmap(filename);
            var map = new Map(img.Width, img.Height, borderWidth);

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Color pixel = img.GetPixel(i, j);
                    //int h = (int)(255.0f*pixel.GetHue());
                    byte h = pixel.R;
                    map.HeightMap[i, (img.Height-1-j)] = h;
                }
            }
            MapTools.map = map;
            MapTools.mapName = Path.GetFileName(filename);
        }

        public static void importHeightmap()
        {
            var openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "BMP|*.bmp|PNG|*.png|All files (*.*)|*.*";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                Bitmap img = new Bitmap(openFileDialog1.FileName);
                if (map != null)
                {
                    int border = map.HeightMap.Border;
                    map.HeightMap = new HeightMap(img.Width, img.Height, border);
                }
                else
                {
                    map = new Map(img.Width, img.Height, 16);
                }

                for (int i = 0; i < img.Width; i++)
                {
                    for (int j = 0; j < img.Height; j++)
                    {
                        Color pixel = img.GetPixel(i, j);
                        byte h = pixel.R;
                        map.HeightMap[i, (img.Height - 1 - j)] = h;
                    }
                }
            }

        }

        public static void saveMap()
        {
            saveMap(compression);
        }

        public static void saveMap(bool compress)
        {
            saveMap(compress ? MapCompressionType.RefPack : MapCompressionType.None);
        }

        public static void saveMap(MapCompressionType compressionType)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "map files (*.map)|*.map|All files (*.*)|*.*";
            //saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var writer = new Writer();
                writer.Map = map;
                writer.WriteFile(saveFileDialog1.FileName, compressionType);
            }
        }


        public static string openMap()
        {
            var openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "map files (*.map)|*.map|All files (*.*)|*.*";

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;
                var reader = new Reader();
                reader.ReadFile(filename);
                MapTools.map = reader.Map;
                // Remember how the file was stored so it can be saved the same way again.
                MapTools.compression = reader.Compression;
                mapName = Path.GetFileName(filename);
                return mapName;
            }
            return "loading failed";
        }

        public static void exportHeightmap()
        {
            int width = MapTools.map.HeightMap.Width;
            int height = MapTools.map.HeightMap.Height;

            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int color = (int)MapTools.map.HeightMap[x, y];
                    bitmap.SetPixel(x, height - 1 - y, System.Drawing.Color.FromArgb(color,color,color));
                }
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PNG|*.png|All files (*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                bitmap.Save(saveFileDialog1.FileName);
            }
        }

        public static void setBorder(int border)
        {
            if (MapTools.map != null)
                MapTools.map.HeightMap.Border = border;
        }

        //public static void exportTextureMap(int tID)
        //{
        //    if (tID >= map.Tiles.Textures.Count) tID = map.Tiles.Textures.Count - 1;

        //    Texture tex = map.Tiles.Textures[tID];


        //    for (int i = 0; i < map.Tiles.Width; i++)
        //    {
        //        for (int j = 0; j < map.Tiles.Height; j++)
        //        {
        //            Tile t = map.Tiles[i];
        //            if (t.BaseTexture == (short)tID)
        //            {

        //            }
        //        }
        //    }
        //}

        public static void textureGenerator(byte waterLevel = 0)
        {
            int width = map.HeightMap.Width;
            int height = map.HeightMap.Height;

            MapLayout layout = new MapLayout();

            if (info == null)
            {
                Genesis.Settings.MapSettings settings = new Genesis.Settings.MapSettings(defaultLayout, defaultScenery, Frequency.Low, Frequency.Low, CkMp.Data.Enumerations.TimeOfDay.Afternoon);
                info = new MapInfo(0, width, height, settings, 2, CancellationToken.None);
            }

            info.Settings.WaterLevel = waterLevel;

            TextureGenerator texGen = new TextureGenerator();
            texGen.Process(map, info);
        }

        public static void removeBlendTiles()
        {
            map.Tiles.BlendTiles.Clear();
            map.Tiles.NumberOfBlendTiles = 1;
            for (int i = 0; i < map.Tiles.Width * map.Tiles.Height; i++)
            {
                map.Tiles[i].BlendTexture1 = 0;
                map.Tiles[i].BlendTexture2 = 0;
                map.Tiles[i].BlendTexture3 = 0;
            }
        }

        public static void blendAllTiles()
        {

            TextureGenerator texGen = new TextureGenerator();
            texGen.map = map;
            texGen.textureData = TileUtils.getTextureData(map);
            texGen.GenerateBlendTiles();
        }

        public static void importCliffmap(byte threshold)
        {
            var openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "PNG|*.png|BMP|*.bmp|All files (*.*)|*.*";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                Bitmap img = new Bitmap(openFileDialog1.FileName);
                if (map != null && map.HeightMap.Width == img.Width && map.HeightMap.Height == img.Height)
                {
                    int width = map.HeightMap.Width;
                    int height = map.HeightMap.Height;
                    if (info == null)
                    {
                        Genesis.Settings.MapSettings settings = new Genesis.Settings.MapSettings(defaultLayout, defaultScenery, Frequency.Low, Frequency.Low, CkMp.Data.Enumerations.TimeOfDay.Afternoon);
                        info = new MapInfo(0, width, height, settings, 2, CancellationToken.None);
                    }

                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            Color pixel = img.GetPixel(i, j);
                            byte h = pixel.R;
                            if (h > threshold)
                            {
                                info.Tiles[i, (height - 1 - j)] = Genesis.Tiles.TileInfo.Cliff;
                            }
                        }
                    }
                }       
            }

        }

        //public static void setWaterLevel(byte waterLevel)
        //{
        //    if (info == null)
        //    {
        //        int width = map.HeightMap.Width;
        //        int height = map.HeightMap.Height;
        //        Genesis.Settings.MapSettings settings = new Genesis.Settings.MapSettings(defaultLayout, defaultScenery, Frequency.Low, Frequency.Low, CkMp.Data.Enumerations.TimeOfDay.Afternoon);
        //        info = new MapInfo(0, width, height, settings, 2, CancellationToken.None);
        //    }


        //}
    }
}
