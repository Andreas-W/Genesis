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
using System.Windows.Navigation;
using System.Windows.Shapes;

using MapTools;


namespace MapToolsGUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MapTools.Options.CloneOptions CloneOptions;

        public MainWindow()
        {
            InitializeComponent();
            MapTools.MapTools.initialize();

            CloneOptions = new MapTools.Options.CloneOptions();
        }

        private void LoadMap_Click(object sender, RoutedEventArgs e)
        {
            string filename = MapTools.MapTools.openMap();
            if (MapTools.MapTools.map != null)
            {
                lbl_map.Content = filename;
                txt_border.Text = MapTools.MapTools.map.HeightMap.Border.ToString();
            }
        }

        private void SaveMap_Click(object sender, RoutedEventArgs e)
        {
            MapTools.MapTools.saveMap();
        }

        private void ImportHeightmap_Click(object sender, RoutedEventArgs e)
        {
            MapTools.MapTools.importHeightmap();
        }

        private void ExportHeightmap_Click(object sender, RoutedEventArgs e)
        {
            MapTools.MapTools.exportHeightmap();
        }

        private void UpdateBorder_Click(object sender, RoutedEventArgs e)
        {
            MapTools.MapTools.setBorder(Int32.Parse(txt_border.Text));
        }

        private void GenerateTextures_Click(object sender, RoutedEventArgs e)
        {
            byte t;
            if (Byte.TryParse(txt_waterLevel.Text, out t))
            {
                MapTools.MapTools.textureGenerator(t);
            }
            else
            {
                MapTools.MapTools.textureGenerator();
            }
        }

        private void LoadCliffs_Click(object sender, RoutedEventArgs e)
        {
            byte t;
            if (Byte.TryParse(txt_cliffthreshold.Text, out t)) {
                MapTools.MapTools.importCliffmap(t);
            }
        }

        private void Rotate_Click(object sender, RoutedEventArgs e)
        {
            MapTools.Commands.Rotate.rotateMap(MapTools.MapTools.map, 1);
        }

        private void MirrorHorizontal_Click(object sender, RoutedEventArgs e)
        {
            MapTools.Commands.Mirror.mirror(MapTools.MapTools.map, false);
        }

        private void MirrorVertical_Click(object sender, RoutedEventArgs e)
        {
            MapTools.Commands.Mirror.mirror(MapTools.MapTools.map, true);
        }

        private void CloneMirrorH_Click(object sender, RoutedEventArgs e)
        {
            MapTools.Commands.Clone.cloneAndMirror(MapTools.MapTools.map, false, this.CloneOptions);
        }

        private void CloneMirrorV_Click(object sender, RoutedEventArgs e)
        {
            MapTools.Commands.Clone.cloneAndMirror(MapTools.MapTools.map, true, this.CloneOptions);
        }

        private void CloneRotateH_Click(object sender, RoutedEventArgs e)
        {
            MapTools.Commands.Clone.cloneAndRotate(MapTools.MapTools.map, false, this.CloneOptions);
        }

        private void CloneRotateV_Click(object sender, RoutedEventArgs e)
        {
            MapTools.Commands.Clone.cloneAndRotate(MapTools.MapTools.map, true, this.CloneOptions);
        }

        private void MapMerge_Click(object sender, RoutedEventArgs e)
        {
            MapMergeWindow window = new MapMergeWindow();
            window.ShowDialog();
        }

        private void BlendAll_Click(object sender, RoutedEventArgs e)
        {
            if (MapTools.MapTools.map != null)
                MapTools.MapTools.blendAllTiles();
        }

        private void RemoveBlend_Click(object sender, RoutedEventArgs e)
        {
            if (MapTools.MapTools.map != null)
                MapTools.MapTools.removeBlendTiles();
        }

        private void CloneOptions_Click(object sender, RoutedEventArgs e)
        {
            if (this.pu_cloneOptions.IsOpen)
            {
                this.CloneOptions.CloneAreas = cb_clone_areas.IsChecked.Value;
                this.CloneOptions.CloneHeightmap = cb_clone_heightmap.IsChecked.Value;
                this.CloneOptions.CloneObjects = cb_clone_objects.IsChecked.Value;
                this.CloneOptions.CloneTerrain = cb_clone_terrain.IsChecked.Value;
                this.CloneOptions.CloneBlendTiles = cb_clone_blendTiles.IsChecked.Value;
                this.CloneOptions.CloneWaypoints = cb_clone_waypoints.IsChecked.Value;
            }
            this.pu_cloneOptions.IsOpen = !this.pu_cloneOptions.IsOpen;
        }

        private void btn_clone_close_Click(object sender, RoutedEventArgs e)
        {
            this.CloneOptions.CloneAreas = cb_clone_areas.IsChecked.Value;
            this.CloneOptions.CloneHeightmap = cb_clone_heightmap.IsChecked.Value;
            this.CloneOptions.CloneObjects = cb_clone_objects.IsChecked.Value;
            this.CloneOptions.CloneTerrain = cb_clone_terrain.IsChecked.Value;
            this.CloneOptions.CloneBlendTiles = cb_clone_blendTiles.IsChecked.Value;
            this.CloneOptions.CloneWaypoints = cb_clone_waypoints.IsChecked.Value;
            this.pu_cloneOptions.IsOpen = false;
        }

        private void cb_cloneOptions_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) return;
            this.CloneOptions.CloneAreas = cb_clone_areas.IsChecked.Value;
            this.CloneOptions.CloneHeightmap = cb_clone_heightmap.IsChecked.Value;
            this.CloneOptions.CloneObjects = cb_clone_objects.IsChecked.Value;
            this.CloneOptions.CloneTerrain = cb_clone_terrain.IsChecked.Value;
            this.CloneOptions.CloneBlendTiles = cb_clone_blendTiles.IsChecked.Value;
            this.CloneOptions.CloneWaypoints = cb_clone_waypoints.IsChecked.Value;
        }

        private void btn_resize_apply_Click(object sender, RoutedEventArgs e)
        {

            int dL = 0, dT = 0, dR = 0, dB = 0;

            Int32.TryParse(txt_resizeL.Text, out dL);
            Int32.TryParse(txt_resizeT.Text, out dT);
            Int32.TryParse(txt_resizeR.Text, out dR);
            Int32.TryParse(txt_resizeB.Text, out dB);

            int[] sizeDiff = new int[] { dL, dT, dR, dB };
            var options = new MapTools.Options.ResizeOptions();
            options.RemoveObjects = false;

            if (MapTools.MapTools.map != null)
            {
                MapTools.Commands.Resize.resize(MapTools.MapTools.map, sizeDiff, options);
            }


            this.pu_resizeOptions.IsOpen = false;
        }

        private void Resize_Click(object sender, RoutedEventArgs e)
        {
            this.pu_resizeOptions.IsOpen = !this.pu_resizeOptions.IsOpen;
        }

        private void btn_resize_close_Click(object sender, RoutedEventArgs e)
        {
            this.pu_resizeOptions.IsOpen = false;
        }

        private void btn_replace_all_Click(object sender, RoutedEventArgs e)
        {
            MapTools.Commands.Replace.replace(MapTools.MapTools.map, true, true, false);
        }

        //private void txt_waterLevel_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    byte waterLevel;

        //    if (Byte.TryParse(txt_waterLevel.Text, out waterLevel))
        //    {
        //        MapTools.MapTools.info.Settings.WaterLevel = waterLevel;
        //    }
        //}
    }
}
