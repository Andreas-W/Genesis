using CkMp.Data.Map;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MapTools.Commands
{

    public class GenerateWak
    {

        public static void generateWakFile(Map map, float waterHeight, float gradientThrehold, int waveType = 1)
        {
            // TODO
        }

        public static Point2f[] ResampleContour(Point[] contour, float step)
        {
            // Convert to Point2f
            var pts = contour.Select(p => new Point2f(p.X, p.Y)).ToArray();

            int n = pts.Length;
            if (n < 2) return pts;

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
            List<Point2f> result = new List<Point2f>();

            foreach (float sd in sampleDists)
            {
                int i = Array.FindLastIndex(cumdist, cd => cd <= sd);
                if (i < 0) i = 0;
                if (i >= n - 1) i = n - 2;

                float t = (sd - cumdist[i]) / (cumdist[i + 1] - cumdist[i] + 1e-6f);

                float x = pts[i].X + t * (pts[i + 1].X - pts[i].X);
                float y = pts[i].Y + t * (pts[i + 1].Y - pts[i].Y);

                result.Add(new Point2f(x, y));
            }

            return result.ToArray();
        }

        public static List<(Point2f start, Point2f end, int type)> GenerateWaterTracks(float[,] hm, float waterHeight = 50f)
        {
            int h = hm.GetLength(0);
            int w = hm.GetLength(1);

            float waveLength = 1.0f;
            float waveDistance = 45.0f;
            float gradientTh = 15.0f;

            // Height mask
            Mat hmTh = new Mat(h, w, MatType.CV_8UC1);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    hmTh.Set(y, x, hm[y, x] > waterHeight ? (byte)255 : (byte)0);
                }
            }

            // Compute gradient manually
            float[,] gx = new float[h, w];
            float[,] gy = new float[h, w];
            float[,] gradVals = new float[h, w];

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    gx[y, x] = (hm[y, x + 1] - hm[y, x - 1]) * 0.5f;
                    gy[y, x] = (hm[y + 1, x] - hm[y - 1, x]) * 0.5f;

                    gradVals[y, x] = (float)Math.Sqrt(gx[y, x] * gx[y, x] + gy[y, x] * gy[y, x]);
                }
            }

            // Scale image
            int mapWidth = w * 10;
            int mapHeight = h * 10;

            Mat hmScaled = new Mat();
            Cv2.Resize(hmTh, hmScaled, new Size(mapWidth, mapHeight));

            // Find contours
            Cv2.FindContours(hmScaled, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxNone);

            var waterTracks = new List<(Point2f, Point2f, int)>();

            foreach (var contour in contours)
            {
                var resampled = ResampleContour(contour, waveDistance);

                foreach (var posStart in resampled)
                {
                    int px = (int)Math.Round(posStart.X * 0.1f);
                    int py = (int)Math.Round(posStart.Y * 0.1f);

                    if (px < 0 || py < 0 || px >= w || py >= h)
                        continue;

                    float gradVal = gradVals[py, px];

                    if (gradVal < gradientTh)
                    {
                        float gxVal = gx[py, px];
                        float gyVal = gy[py, px];

                        float norm = (float)Math.Sqrt(gxVal * gxVal + gyVal * gyVal);
                        if (norm == 0) norm = 1;

                        float dirX = gxVal / norm;
                        float dirY = gyVal / norm;

                        Point2f posEnd = new Point2f(
                            posStart.X + dirX * waveLength,
                            posStart.Y + dirY * waveLength
                        );

                        waterTracks.Add((posStart, posEnd, 1));
                    }
                }
            }

            return waterTracks;
        }
    }
}
