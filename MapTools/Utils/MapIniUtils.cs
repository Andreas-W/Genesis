using System;
using System.Globalization;
using System.IO;

namespace MapTools.Utils
{
    /// <summary>
    /// Reads settings from the map.ini that can sit next to a .map file.
    /// </summary>
    public class MapIniUtils
    {
        public const string FileName = "map.ini";

        /// <summary>
        /// HeightMapScale from the MapData block of the map.ini next to the map.
        /// The game multiplies the stored heights by it when loading the map.
        /// </summary>
        /// <returns>the scale, 1 if there is no map.ini or no valid value</returns>
        public static float readHeightMapScale(string mapPath)
        {
            if (String.IsNullOrEmpty(mapPath))
                return 1f;

            string iniPath = Path.Combine(Path.GetDirectoryName(mapPath), FileName);
            if (!File.Exists(iniPath))
                return 1f;

            bool inMapData = false;
            foreach (string rawLine in File.ReadAllLines(iniPath))
            {
                string line = stripComment(rawLine).Trim();
                if (line.Length == 0)
                    continue;

                if (!inMapData)
                {
                    inMapData = line.Equals("MapData", StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (line.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    inMapData = false;
                    continue;
                }

                int eq = line.IndexOf('=');
                if (eq < 0 || !line.Substring(0, eq).Trim().Equals("HeightMapScale", StringComparison.OrdinalIgnoreCase))
                    continue;

                float scale;
                if (Single.TryParse(line.Substring(eq + 1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out scale) && scale > 0)
                    return scale;
            }
            return 1f;
        }

        static string stripComment(string line)
        {
            int semicolon = line.IndexOf(';');
            if (semicolon >= 0) line = line.Substring(0, semicolon);
            int slashes = line.IndexOf("//");
            if (slashes >= 0) line = line.Substring(0, slashes);
            return line;
        }
    }
}
