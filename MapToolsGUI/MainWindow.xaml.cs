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
                txt_wak_heightScale.Text = formatFloat(MapTools.MapTools.heightMapScale);
                // Default to saving in the same container the map was loaded from.
                cb_compress.IsChecked =
                    MapTools.MapTools.compression != CkMp.Data.Compression.MapCompressionType.None;
            }
        }

        private void SaveMap_Click(object sender, RoutedEventArgs e)
        {
            MapTools.MapTools.saveMap(cb_compress.IsChecked == true);
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

        private void CloneQuarter_Click(object sender, RoutedEventArgs e)
        {
            if (MapTools.MapTools.map == null)
                return;

            if (!MapTools.Commands.CloneQuarter.canCloneQuarter(MapTools.MapTools.map))
            {
                MessageBox.Show(MapTools.Commands.CloneQuarter.NotSquareMessage, "Clone Quarter",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MapTools.Commands.CloneQuarter.cloneQuarter(MapTools.MapTools.map, this.CloneOptions);
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

        private void applyCloneOptions()
        {
            this.CloneOptions.CloneAreas = cb_clone_areas.IsChecked.Value;
            this.CloneOptions.CloneHeightmap = cb_clone_heightmap.IsChecked.Value;
            this.CloneOptions.CloneObjects = cb_clone_objects.IsChecked.Value;
            this.CloneOptions.CloneTerrain = cb_clone_terrain.IsChecked.Value;
            this.CloneOptions.CloneBlendTiles = cb_clone_blendTiles.IsChecked.Value;
            this.CloneOptions.CloneWaypoints = cb_clone_waypoints.IsChecked.Value;
            this.CloneOptions.SkipDefaultWater = cb_clone_skipDefaultWater.IsChecked.Value;
        }

        private void CloneOptions_Click(object sender, RoutedEventArgs e)
        {
            if (this.pu_cloneOptions.IsOpen)
                applyCloneOptions();

            this.pu_cloneOptions.IsOpen = !this.pu_cloneOptions.IsOpen;
        }

        private void btn_clone_close_Click(object sender, RoutedEventArgs e)
        {
            applyCloneOptions();
            this.pu_cloneOptions.IsOpen = false;
        }

        private void cb_cloneOptions_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) return;
            applyCloneOptions();
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

        private void GenerateWak_Click(object sender, RoutedEventArgs e)
        {
            // Default to the water level used for texture generation (heightmap value), if one is set.
            float waterLevel;
            if (!this.pu_wakOptions.IsOpen && tryParseFloat(txt_waterLevel.Text, out waterLevel))
                txt_wak_waterLevel.Text = formatFloat(fromRawHeight(waterLevel, wakHeightUnit, wakHeightMapScale()));

            this.pu_wakOptions.IsOpen = !this.pu_wakOptions.IsOpen;
        }

        private void btn_wak_generate_Click(object sender, RoutedEventArgs e)
        {
            if (MapTools.MapTools.map == null)
                return;

            var options = new MapTools.Options.WakOptions();
            options.HeightMapScale = wakHeightMapScale();
            float value;
            if (tryParseFloat(txt_wak_waterLevel.Text, out value))
                options.WaterHeight = toRawHeight(value, wakHeightUnit, options.HeightMapScale);
            if (tryParseFloat(txt_wak_gradient.Text, out value)) options.GradientThreshold = value;
            if (tryParseFloat(txt_wak_waveDistance.Text, out value) && value > 0) options.WaveDistance = value;
            if (tryParseFloat(txt_wak_inwardsShift.Text, out value)) options.InwardsShift = value;
            if (tryParseFloat(txt_wak_smoothing.Text, out value) && value > 0) options.ShoreSmoothing = value;
            if (tryParseFloat(txt_wak_minWaterWidth.Text, out value) && value > 0) options.MinWaterWidth = value;
            options.WaveType = cmb_wak_waveType.SelectedIndex;

            int count = MapTools.MapTools.generateWak(options);
            if (count >= 0)
            {
                this.pu_wakOptions.IsOpen = false;
                MessageBox.Show(String.Format("Wrote {0} water tracks.", count), "Generate WAK",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btn_wak_close_Click(object sender, RoutedEventArgs e)
        {
            this.pu_wakOptions.IsOpen = false;
        }

        // Units of txt_wak_waterLevel, in the order of cmb_wak_heightUnit.
        const int HeightUnitRaw = 0;        // heightmap value as stored in the map file
        const int HeightUnitWorldBuilder = 1; // raw value * HeightMapScale, as shown in WorldBuilder
        const int HeightUnitFeet = 2;       // game units (ft): WorldBuilder height * MAP_HEIGHT_SCALE

        private int wakHeightUnit = HeightUnitRaw;

        private float wakHeightMapScale()
        {
            float scale;
            return tryParseFloat(txt_wak_heightScale.Text, out scale) && scale > 0 ? scale : 1f;
        }

        private static float toRawHeight(float value, int unit, float heightMapScale)
        {
            if (unit == HeightUnitWorldBuilder) return value / heightMapScale;
            if (unit == HeightUnitFeet) return value / (heightMapScale * MapTools.Commands.GenerateWak.MapHeightScale);
            return value;
        }

        private static float fromRawHeight(float value, int unit, float heightMapScale)
        {
            if (unit == HeightUnitWorldBuilder) return value * heightMapScale;
            if (unit == HeightUnitFeet) return value * heightMapScale * MapTools.Commands.GenerateWak.MapHeightScale;
            return value;
        }

        private void cmb_wak_heightUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsInitialized) return;

            int unit = cmb_wak_heightUnit.SelectedIndex;
            if (unit < 0 || unit == wakHeightUnit) return;

            // Convert the entered value so it keeps describing the same height.
            float value;
            if (tryParseFloat(txt_wak_waterLevel.Text, out value))
            {
                float scale = wakHeightMapScale();
                txt_wak_waterLevel.Text = formatFloat(fromRawHeight(toRawHeight(value, wakHeightUnit, scale), unit, scale));
            }

            wakHeightUnit = unit;
        }

        private static string formatFloat(float value)
        {
            return value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        }

        // Accepts both "7.5" and "7,5" regardless of the system locale.
        private static bool tryParseFloat(string text, out float value)
        {
            return Single.TryParse(text.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out value);
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
