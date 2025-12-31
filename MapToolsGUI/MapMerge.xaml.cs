using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using MapTools;
using CkMp.Data.Map;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using Microsoft.Win32;
using System.Windows.Forms;

using MapTools.MapMerge;
using System.Diagnostics;

namespace MapToolsGUI
{
    /// <summary>
    /// Interaktionslogik für Window1.xaml
    /// </summary>
    public partial class MapMergeWindow : Window
    {
        public MapMerger Merger = new MapMerger();

        //public bool IsMap1Loaded {get { return Merger.Map1 != null; } }
        //public bool IsMap2Loaded {get { return Merger.Map2 != null; } }

        public BitmapImage Map1Image;
        public BitmapImage Map2Image;

        private RotateTransform map1Rotate;
        private ScaleTransform map1Mirror;

        private RotateTransform map2Rotate;
        private ScaleTransform map2Mirror;

        public MapMergeWindow()
        {
            InitializeComponent();

            if (MapTools.MapTools.map != null)
            {
                Merger.Map1 = new MergeMapInfo()
                {
                    Map = MapTools.MapTools.map,
                    MapName = MapTools.MapTools.mapName
                };
            }

            updateMap1Properties();
            updateMap2Properties();

            //Map map1 = MapTools.MapTools.map;
            //string map1Name = MapTools.MapTools.mapName;

            //this.img_map1.Source = getMapImage(map1);
            //this.img_map1.Height = map1.HeightMap.Height;
            //this.img_map1.Width = map1.HeightMap.Width;

            //this.rect_border1.Height = map1.HeightMap.Height - map1.HeightMap.Border*2;
            //this.rect_border1.Width = map1.HeightMap.Width - map1.HeightMap.Border * 2;
            //Thickness margin1 = new Thickness(map1.HeightMap.Border, map1.HeightMap.Border, 0, 0);
            //this.rect_border1.Margin = margin1;

            //this.lbl_map1.Content = map1Name;
            //this.tb_map1_height.Text = ""+ map1.HeightMap.Height;
            //this.tb_map1_width.Text = "" + map1.HeightMap.Width;

        }

        BitmapImage getMapImage(Map map)
        {
            int width = map.HeightMap.Width;
            int height = map.HeightMap.Height;

            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int color = (int)map.HeightMap[x, y];
                    bitmap.SetPixel(x, height - 1 - y, System.Drawing.Color.FromArgb(color, color, color));
                }
            }
            return toBitmapImage(bitmap);
        }


        static BitmapImage toBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void updateMap1Properties()
        {
            if (Merger.Map1 != null)
            {
                var map = Merger.Map1;
                int height = map.Map.HeightMap.Height;
                int width = map.Map.HeightMap.Width;
                int border = map.Map.HeightMap.Border;

                this.Map1Image = getMapImage(map.Map);
                this.img_map1.Source = Map1Image;
                this.img_map1.Height = height;
                this.img_map1.Width = width;

                this.map1Rotate = new RotateTransform(Merger.Map1.Rotation * 90, width/2, height/2);
                this.map1Mirror = new ScaleTransform(Merger.Map1.MirroredHorizontal?-1:1, Merger.Map1.MirroredVertical?-1:1, width/2, height/2);
                var tg = new TransformGroup();
                tg.Children.Add(map1Mirror);
                tg.Children.Add(map1Rotate);
                this.img_map1.RenderTransform = tg;

                this.tb_map1_border.Text = "" + border;

                this.rect_border1.Height = height - border * 2;
                this.rect_border1.Width = width - border * 2;
                Thickness margin1 = new Thickness(border, border, 0, 0);
                this.rect_border1.Margin = margin1;

                tg = new TransformGroup();
                tg.Children.Add(new TranslateTransform(border, border));
                tg.Children.Add(map1Mirror);
                tg.Children.Add(map1Rotate);
                tg.Children.Add(new TranslateTransform(-border, -border));
                this.rect_border1.RenderTransform = tg;

                this.lbl_map1.Content = map.MapName;
                this.tb_map1_height.Text = "" + height;
                this.tb_map1_width.Text = "" + width;
            }

            this.panel_map1buttons.IsEnabled = Merger.Map1 != null;
            this.panel_map1crop.IsEnabled = Merger.Map1 != null;
            this.panel_map1offset.IsEnabled = Merger.Map1 != null;
        }

        private void updateMap2Properties()
        {
            if (Merger.Map2 != null)
            {
                var map = Merger.Map2;
                int height = map.Map.HeightMap.Height;
                int width = map.Map.HeightMap.Width;
                int border = map.Map.HeightMap.Border;

                this.Map2Image = getMapImage(map.Map);
                this.img_map2.Source = Map2Image;
                this.img_map2.Height = height;
                this.img_map2.Width = width;

                this.map2Rotate = new RotateTransform(Merger.Map2.Rotation * 90, width/2, height/2 );
                this.map2Mirror = new ScaleTransform(Merger.Map2.MirroredHorizontal ? -1 : 1, Merger.Map2.MirroredVertical ? -1 : 1, width/2, height/2);
                var tg = new TransformGroup();
                tg.Children.Add(map2Mirror);
                tg.Children.Add(map2Rotate);
                this.img_map2.RenderTransform = tg;

                this.tb_map2_border.Text = "" + border;

                this.rect_border2.Height = height - border * 2;
                this.rect_border2.Width = width - border * 2;
                Thickness margin2 = new Thickness(border, border, 0, 0);
                this.rect_border2.Margin = margin2;

                tg = new TransformGroup();
                tg.Children.Add(new TranslateTransform(border, border));
                tg.Children.Add(map2Mirror);
                tg.Children.Add(map2Rotate);
                tg.Children.Add(new TranslateTransform(-border, -border));
                this.rect_border2.RenderTransform = tg;

                this.lbl_map2.Content = map.MapName;
                this.tb_map2_height.Text = "" + height;
                this.tb_map2_width.Text = "" + width;

            }
            this.panel_map2buttons.IsEnabled = Merger.Map2 != null;
            this.panel_map2crop.IsEnabled = Merger.Map2 != null;
            this.panel_map2offset.IsEnabled = Merger.Map2 != null;
        }

        private void btn_map1_open_Click(object sender, RoutedEventArgs e)
        {
            Merger.Map1 = Merger.openMap();
            updateMap1Properties();
        }

        private void btn_map2_open_Click(object sender, RoutedEventArgs e)
        {
            Merger.Map2 = Merger.openMap();
            updateMap2Properties();
        }

        private void btn_map1_mirror_H_Click(object sender, RoutedEventArgs e)
        {
            Merger.Map1.MirroredHorizontal = !Merger.Map1.MirroredHorizontal;

            map1Mirror.ScaleX = Merger.Map1.MirroredHorizontal ? -1 : 1;
        }

        private void btn_map1_mirror_V_Click(object sender, RoutedEventArgs e)
        {
            Merger.Map1.MirroredVertical = !Merger.Map1.MirroredVertical;

            map1Mirror.ScaleY = Merger.Map1.MirroredVertical ? -1 : 1;
        }

        private void btn_map1_rotatePlus_Click(object sender, RoutedEventArgs e)
        {
            int height = Merger.Map1.Map.HeightMap.Height;
            int width = Merger.Map1.Map.HeightMap.Width;
            int border = Merger.Map1.Map.HeightMap.Border;
            byte rota = (byte)((Merger.Map1.Rotation+1) % 4);

            //img_map1.RenderTransform = new RotateTransform(90 * rota, width / 2, height / 2);
            //rect_border1.RenderTransform = new RotateTransform(90 * rota, width / 2 - border, height / 2 - border);
            map1Rotate.Angle = rota * 90;

            Merger.Map1.Rotation = rota; 
        }

        private void btn_map1_rotateMinus_Click(object sender, RoutedEventArgs e)
        {
            int height = Merger.Map1.Map.HeightMap.Height;
            int width = Merger.Map1.Map.HeightMap.Width;
            int border = Merger.Map1.Map.HeightMap.Border;
            byte rota = (byte)((Merger.Map1.Rotation - 1) % 4);

            //img_map1.RenderTransform = new RotateTransform(90 * rota, width / 2, height / 2);
            //rect_border1.RenderTransform = new RotateTransform(90 * rota, width / 2 - border, height / 2 - border);
            map1Rotate.Angle = rota * 90;

            Merger.Map1.Rotation = rota;
        }

        private void btn_map2_mirror_H_Click(object sender, RoutedEventArgs e)
        {
            Merger.Map2.MirroredHorizontal = !Merger.Map2.MirroredHorizontal;

            map2Mirror.ScaleX = Merger.Map2.MirroredHorizontal ? -1 : 1;
        }

        private void btn_map2_mirror_V_Click(object sender, RoutedEventArgs e)
        {
            Merger.Map2.MirroredVertical = !Merger.Map2.MirroredVertical;

            map2Mirror.ScaleY = Merger.Map2.MirroredVertical ? -1 : 1;
        }

        private void btn_map2_rotatePlus_Click(object sender, RoutedEventArgs e)
        {
            int height = Merger.Map2.Map.HeightMap.Height;
            int width = Merger.Map2.Map.HeightMap.Width;
            int border = Merger.Map2.Map.HeightMap.Border;
            byte rota = (byte)((Merger.Map2.Rotation + 3) % 4);

            map2Rotate.Angle = rota * -90;
            // img_map2.RenderTransform = new RotateTransform(90 * rota, width / 2, height / 2);
            // rect_border2.RenderTransform = new RotateTransform(90 * rota, width / 2 - border, height / 2 - border);

            Merger.Map2.Rotation = rota;
        }

        private void btn_map2_rotateMinus_Click(object sender, RoutedEventArgs e)
        {
            int height = Merger.Map2.Map.HeightMap.Height;
            int width = Merger.Map2.Map.HeightMap.Width;
            int border = Merger.Map2.Map.HeightMap.Border;
            byte rota = (byte)((Merger.Map2.Rotation + 1) % 4);

            map2Rotate.Angle = rota * -90;
            //img_map2.RenderTransform = new RotateTransform(90 * rota, width / 2, height / 2);
            //rect_border2.RenderTransform = new RotateTransform(90 * rota, width / 2 - border, height / 2 - border);


            Merger.Map2.Rotation = rota;
        }

        private void tb_map1_offset_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsInitialized) return;

            int offsetX = 0, offsetY = 0;
            if (tb_map1_offsetX.Text != null)
            {
                Int32.TryParse(tb_map1_offsetX.Text, out offsetX);
            }
            if (tb_map1_offsetY.Text != null)
            {
                Int32.TryParse(tb_map1_offsetY.Text, out offsetY);
            }

            Merger.Map1.OffsetX = offsetX;
            Merger.Map1.OffsetY = offsetY;

            int border = Merger.Map1.Map.HeightMap.Border;

            Thickness m = new Thickness(offsetX, offsetY, 0, 0);
            img_map1.Margin = m;

            rect_border1.Margin = new Thickness(offsetX+border, offsetY+border, 0, 0);
        }

        private void tb_map2_offset_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsInitialized) return;

            int offsetX = 0, offsetY = 0;
            if (tb_map2_offsetX.Text != null)
            {
                Int32.TryParse(tb_map2_offsetX.Text, out offsetX);
            }
            if (tb_map2_offsetY.Text != null)
            {
                Int32.TryParse(tb_map2_offsetY.Text, out offsetY);
            }

            Merger.Map2.OffsetX = offsetX;
            Merger.Map2.OffsetY = offsetY;

            int border = Merger.Map2.Map.HeightMap.Border;

            Thickness m = new Thickness(offsetX, offsetY, 0, 0);
            img_map2.Margin = m;

            rect_border2.Margin = new Thickness(offsetX + border, offsetY + border, 0, 0);
        }

        private void tb_map1_offset_CropChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsInitialized) return;

            int cropL = 0, cropT = 0, cropR = 0, cropB = 0;
            if (tb_map1_c_l.Text != null)
            {
                Int32.TryParse(tb_map1_c_l.Text, out cropL);
            }
            if (tb_map1_c_t.Text != null)
            {
                Int32.TryParse(tb_map1_c_t.Text, out cropT);
            }
            if (tb_map1_c_r.Text != null)
            {
                Int32.TryParse(tb_map1_c_r.Text, out cropR);
            }
            if (tb_map1_c_b.Text != null)
            {
                Int32.TryParse(tb_map1_c_b.Text, out cropB);
            }

            int height = Merger.Map1.Map.HeightMap.Height;
            int width = Merger.Map1.Map.HeightMap.Width;
            int border = Merger.Map1.Map.HeightMap.Border;

            Merger.Map1.Crop[0] = cropL;
            Merger.Map1.Crop[1] = cropT;
            Merger.Map1.Crop[2] = cropR;
            Merger.Map1.Crop[3] = cropB;

            img_map1.Clip = new RectangleGeometry(new Rect(cropL, cropT, width - cropL - cropR, height - cropT - cropB));
            rect_border1.Clip = new RectangleGeometry(new Rect(Math.Max(0, cropL - border), Math.Max(0, cropT - border), width-border*2 - Math.Max(0, cropL - border) - Math.Max(0, cropR - border), height-border*2 -Math.Max(0, cropT - border) - Math.Max(0, cropB - border)));
            //CroppedBitmap cb = new CroppedBitmap(Map1Image, new Int32Rect(cropL, cropT, width - cropL- cropR, height - cropT - cropB));
            //img_map1.Source = cb;

        }

        private void tb_map2_offset_CropChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsInitialized) return;

            int cropL = 0, cropT = 0, cropR = 0, cropB = 0;
            if (tb_map2_c_l.Text != null)
            {
                Int32.TryParse(tb_map2_c_l.Text, out cropL);
            }
            if (tb_map2_c_t.Text != null)
            {
                Int32.TryParse(tb_map2_c_t.Text, out cropT);
            }
            if (tb_map2_c_r.Text != null)
            {
                Int32.TryParse(tb_map2_c_r.Text, out cropR);
            }
            if (tb_map2_c_b.Text != null)
            {
                Int32.TryParse(tb_map2_c_b.Text, out cropB);
            }

            int height = Merger.Map2.Map.HeightMap.Height;
            int width = Merger.Map2.Map.HeightMap.Width;
            int border = Merger.Map2.Map.HeightMap.Border;

            Merger.Map2.Crop[0] = cropL;
            Merger.Map2.Crop[1] = cropT;
            Merger.Map2.Crop[2] = cropR;
            Merger.Map2.Crop[3] = cropB;

            img_map2.Clip = new RectangleGeometry(new Rect(cropL, cropT, width - cropL - cropR, height - cropT - cropB));
            rect_border2.Clip = new RectangleGeometry(new Rect(Math.Max(0, cropL - border), Math.Max(0, cropT - border), width - border * 2 - Math.Max(0, cropL - border) - Math.Max(0, cropR - border), height - border * 2 - Math.Max(0, cropT - border) - Math.Max(0, cropB - border)));
            //CroppedBitmap cb = new CroppedBitmap(Map2Image, new Int32Rect(cropL, cropT, width - cropL - cropR, height - cropT - cropB));
            //img_map1.Source = cb;

        }

        private void tb_merged_size_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsInitialized) return;

            int width = 0, height = 0, border = 0;
            if (tb_merged_width.Text != null)
            {
                Int32.TryParse(tb_merged_width.Text, out width);
            }
            if (tb_merged_height.Text != null)
            {
                Int32.TryParse(tb_merged_height.Text, out height);
            }
            if (tb_merged_border.Text != null)
            {
                Int32.TryParse(tb_merged_border.Text, out border);
            }

            border = Math.Min(Math.Min(width, height)/2, border);

            Merger.MergedBorder = border;
            Merger.MergedWidth = width;
            Merger.MergedHeight = height;

            rect_mergedMap.Width = width;
            rect_mergedMap.Height = height;
            rect_border_merged.Width = width - 2 * border;
            rect_border_merged.Height = height - 2 * border;
            rect_border_merged.Margin = new Thickness(border, border, 0, 0);
        }

        private void btn_mergeMaps_Click(object sender, RoutedEventArgs e)
        {
            Map result = this.Merger.MergeMaps();
            MapTools.MapTools.map = result;
            //this.Close();
        }

        private void btn_test_Click(object sender, RoutedEventArgs e)
        {
            //string PATH = "..\\..\\..\\Test\\MapMerge";
            string PATH = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "..\\..\\..\\..\\Test\\MapMerge\\";
            string TESTMAP1 = "Alpine Assault_b.map";
            string TESTMAP2 = "Tournament Desert_b.map";

           
            Merger.Map1 = this.Merger.openMap(PATH+TESTMAP1);
            updateMap1Properties();
            Merger.Map2 = this.Merger.openMap(PATH + TESTMAP2);
            updateMap2Properties();

            //load default values

            this.tb_map2_c_l.Text = "140";
            this.tb_map2_offsetX.Text = "35";
            this.tb_map2_offsetY.Text = "30";

            this.tb_merged_width.Text = "350";
            this.tb_merged_height.Text = "400";
            this.tb_merged_border.Text = "65";
        }
    }
}
