using CkMp.Data.Map;
using MapTools.Options;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MapTools.Commands
{

    public class GenerateWak
    {
        /// <summary>
        /// World units per heightmap cell (MAP_XY_FACTOR in the game).
        /// </summary>
        const float CellSize = 10f;

        /// <summary>
        /// Game units (ft) per (scaled) height unit (MAP_HEIGHT_SCALE in the game).
        /// </summary>
        public const float MapHeightScale = CellSize / 16f;

        /// <summary>
        /// Length of a single wave track (start to end), in world units.
        /// </summary>
        const float WaveLength = 1f;

        /// <summary>
        /// How far off the shore (in cells) the water is checked for the min water width filter.
        /// </summary>
        const float WaterProbeDistance = 1.5f;

        /// <summary>
        /// Generates shore waves from the map's heightmap and writes them to a .wak file.
        /// </summary>
        /// <returns>number of written water tracks</returns>
        public static int generateWakFile(Map map, string path, WakOptions options)
        {
            var tracks = GenerateWaterTracks(map.HeightMap, options);
            WriteWakFile(path, tracks);
            return tracks.Count;
        }

        public static void WriteWakFile(string path, List<(Point2f start, Point2f end, int type)> tracks)
        {
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                foreach (var track in tracks)
                {
                    writer.Write(track.start.X);
                    writer.Write(track.start.Y);
                    writer.Write(track.end.X);
                    writer.Write(track.end.Y);
                    writer.Write(track.type);
                }
                // track count is stored at the end of the file
                writer.Write(tracks.Count);
            }
        }

        /// <summary>
        /// Gaussian smoothing of a closed contour along its length.
        /// </summary>
        /// <param name="sigma">in contour units (scaled pixels = world units)</param>
        public static Point2f[] SmoothContour(Point2f[] pts, float sigma)
        {
            int n = pts.Length;
            int radius = (int)Math.Ceiling(3 * sigma);
            if (n < 3 || radius < 1) return pts;

            float[] kernel = new float[2 * radius + 1];
            float sum = 0;
            for (int k = -radius; k <= radius; k++)
            {
                kernel[k + radius] = (float)Math.Exp(-0.5 * k * k / (sigma * sigma));
                sum += kernel[k + radius];
            }

            var result = new Point2f[n];
            for (int i = 0; i < n; i++)
            {
                float x = 0, y = 0;
                for (int k = -radius; k <= radius; k++)
                {
                    // contours are closed, wrap around
                    var p = pts[((i + k) % n + n) % n];
                    x += kernel[k + radius] * p.X;
                    y += kernel[k + radius] * p.Y;
                }
                result[i] = new Point2f(x / sum, y / sum);
            }
            return result;
        }

        /// <summary>
        /// Samples points every <paramref name="step"/> along the contour, with the contour direction at each point.
        /// </summary>
        public static List<(Point2f pos, Point2f tangent)> ResampleContour(Point2f[] pts, float step)
        {
            int n = pts.Length;
            if (n < 2) return pts.Select(p => (p, new Point2f(0, 0))).ToList();

            // Distances between consecutive points
            float[] dists = new float[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                float dx = pts[i + 1].X - pts[i].X;
                float dy = pts[i + 1].Y - pts[i].Y;
                dists[i] = (float)Math.Sqrt(dx * dx + dy * dy);
            }

            // Cumulative distance
            float[] cumdist = new float[n];
            cumdist[0] = 0;
            for (int i = 1; i < n; i++)
                cumdist[i] = cumdist[i - 1] + dists[i - 1];

            float totalLength = cumdist[n - 1];

            // Sample distances
            List<float> sampleDists = new List<float>();
            for (float d = 0; d < totalLength; d += step)
                sampleDists.Add(d);

            // Interpolate
            var result = new List<(Point2f, Point2f)>();

            foreach (float sd in sampleDists)
            {
                int i = Array.FindLastIndex(cumdist, cd => cd <= sd);
                if (i < 0) i = 0;
                if (i >= n - 1) i = n - 2;

                float t = (sd - cumdist[i]) / (cumdist[i + 1] - cumdist[i] + 1e-6f);

                float x = pts[i].X + t * (pts[i + 1].X - pts[i].X);
                float y = pts[i].Y + t * (pts[i + 1].Y - pts[i].Y);

                result.Add((new Point2f(x, y), new Point2f(pts[i + 1].X - pts[i].X, pts[i + 1].Y - pts[i].Y)));
            }

            return result;
        }

        /// <summary>
        /// Places waves along the water line of the heightmap, pointing uphill (towards land).
        /// Returned positions are in world coordinates (border excluded), as stored in .wak files.
        /// </summary>
        public static List<(Point2f start, Point2f end, int type)> GenerateWaterTracks(HeightMap hm, WakOptions options)
        {
            int w = hm.Width;
            int h = hm.Height;
            byte[] heights = hm.ToArray(); // row-major (y * Width + x), y pointing up

            // Gradient (central differences, one-sided at the edges like numpy.gradient), in scaled
            // height units so the gradient threshold means the same slope regardless of HeightMapScale
            float scale = options.HeightMapScale > 0 ? options.HeightMapScale : 1f;
            float[,] gx = new float[h, w];
            float[,] gy = new float[h, w];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int x0 = Math.Max(x - 1, 0), x1 = Math.Min(x + 1, w - 1);
                    int y0 = Math.Max(y - 1, 0), y1 = Math.Min(y + 1, h - 1);

                    if (x1 > x0)
                        gx[y, x] = scale * (heights[y * w + x1] - heights[y * w + x0]) / (x1 - x0);
                    if (y1 > y0)
                        gy[y, x] = scale * (heights[y1 * w + x] - heights[y0 * w + x]) / (y1 - y0);
                }
            }

            Point[][] contours;
            using (var hmMat = new Mat(h, w, MatType.CV_8UC1))
            using (var hmScaled = new Mat())
            {
                hmMat.SetArray(heights);

                // Scale the heightmap up (world unit precision) before thresholding, so the
                // contour follows the interpolated water line instead of whole cells
                Cv2.Resize(hmMat, hmScaled, new Size(w * (int)CellSize, h * (int)CellSize));
                Cv2.Threshold(hmScaled, hmScaled, options.WaterHeight, 255, ThresholdTypes.Binary);
                Cv2.FindContours(hmScaled, out contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxNone);
            }

            byte[] wideWater = BuildWideWaterMask(heights, w, h, options);

            float border = hm.Border;
            var waterTracks = new List<(Point2f, Point2f, int)>();

            bool smooth = options.ShoreSmoothing > 0;

            foreach (var contour in contours)
            {
                var pts = contour.Select(c => new Point2f(c.X, c.Y)).ToArray();
                if (smooth)
                    pts = SmoothContour(pts, options.ShoreSmoothing);

                foreach (var (p, tangent) in ResampleContour(pts, options.WaveDistance))
                {
                    // scaled pixel -> heightmap cell coordinates
                    float u = (p.X + 0.5f) / CellSize - 0.5f;
                    float v = (p.Y + 0.5f) / CellSize - 0.5f;

                    if (u < 0 || v < 0 || u > w - 1 || v > h - 1)
                        continue;

                    float gxVal = SampleBilinear(gx, u, v);
                    float gyVal = SampleBilinear(gy, u, v);
                    float norm = (float)Math.Sqrt(gxVal * gxVal + gyVal * gyVal);

                    // flat spots have no direction, steep shores get no waves
                    if (norm == 0 || norm >= options.GradientThreshold)
                        continue;

                    float dirX = gxVal / norm;
                    float dirY = gyVal / norm;

                    // Smoothed shore: wave perpendicular to the shore line, on the uphill side
                    float tangentLength = (float)Math.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y);
                    if (smooth && tangentLength > 0)
                    {
                        float nx = -tangent.Y / tangentLength;
                        float ny = tangent.X / tangentLength;
                        if (nx * dirX + ny * dirY < 0)
                        {
                            nx = -nx;
                            ny = -ny;
                        }
                        dirX = nx;
                        dirY = ny;
                    }

                    // Thin water (rivers, channels): the water just off the shore was removed from the mask
                    if (wideWater != null)
                    {
                        int px = (int)Math.Round(u - dirX * WaterProbeDistance);
                        int py = (int)Math.Round(v - dirY * WaterProbeDistance);
                        if (px < 0 || py < 0 || px >= w || py >= h || wideWater[py * w + px] == 0)
                            continue;
                    }

                    var start = new Point2f(
                        (u - border) * CellSize + dirX * options.InwardsShift,
                        (v - border) * CellSize + dirY * options.InwardsShift);
                    var end = new Point2f(
                        start.X + dirX * WaveLength,
                        start.Y + dirY * WaveLength);

                    waterTracks.Add((start, end, options.WaveType));
                }
            }

            return waterTracks;
        }

        /// <summary>
        /// Water mask (cell resolution, 255 = water) without the parts narrower than MinWaterWidth.
        /// </summary>
        /// <returns>null if the filter is off</returns>
        static byte[] BuildWideWaterMask(byte[] heights, int w, int h, WakOptions options)
        {
            int kernelSize = (int)Math.Round(options.MinWaterWidth / CellSize);
            if (kernelSize < 2)
                return null;

            byte[] water = heights.Select(v => v > options.WaterHeight ? (byte)0 : (byte)255).ToArray();

            using (var waterMat = new Mat(h, w, MatType.CV_8UC1))
            using (var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(kernelSize, kernelSize)))
            {
                waterMat.SetArray(water);
                // Opening keeps all water a circle of the min width fits into, so channels narrower than that disappear
                Cv2.MorphologyEx(waterMat, waterMat, MorphTypes.Open, kernel);
                waterMat.GetArray(out water);
            }
            return water;
        }

        static float SampleBilinear(float[,] grid, float x, float y)
        {
            int x0 = Math.Min((int)x, grid.GetLength(1) - 2);
            int y0 = Math.Min((int)y, grid.GetLength(0) - 2);
            float tx = x - x0;
            float ty = y - y0;

            float top = grid[y0, x0] * (1 - tx) + grid[y0, x0 + 1] * tx;
            float bottom = grid[y0 + 1, x0] * (1 - tx) + grid[y0 + 1, x0 + 1] * tx;
            return top * (1 - ty) + bottom * ty;
        }
    }
}
