using CkMp.Data.Map;
using CkMp.Data.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace MapTools.Commands
{
    public class Replace
    {
        static string REPLACE_LIST_TERRAIN = "./ccu_to_default_terrain_replace_list.txt";
        static string REPLACE_LIST_ROADS = "./ccu_to_default_roads_replace_list.txt";
        static string REPLACE_LIST_OBJECTS = "./replace_objects_list.txt";

        public static Dictionary<string, string> ReadKeyValueFile(string fileName)
        {
            var result = new Dictionary<string, string>();

            foreach (var line in File.ReadLines(fileName))
            {
                // Skip empty or whitespace lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Split into at most 2 parts (key + value)
                var parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    var key = parts[0];
                    var value = parts[1];
                    result[key] = value;
                }
                // Optional: handle lines with only a key (no value)
                // else if (parts.Length == 1)
                // {
                //     result[parts[0]] = string.Empty;
                // }
            }

            return result;
        }

        public static void replace(Map map, bool replaceTerrain, bool replaceRoads, bool replaceObjects)
        {
            if (replaceTerrain)
            {
                ReplaceMapTerrain(map, ReadKeyValueFile(Replace.REPLACE_LIST_TERRAIN));
            }

            if (replaceRoads)
            {
                ReplaceMapObjects(map, ReadKeyValueFile(Replace.REPLACE_LIST_ROADS));
            }

            if (replaceObjects)
            {
                ReplaceMapObjects(map, ReadKeyValueFile(Replace.REPLACE_LIST_OBJECTS));
            }
        }

        static void ReplaceMapTerrain(Map map, Dictionary<string, string> replaceDict)
        {

            foreach (var texture in map.Tiles.Textures)
            {
                if (replaceDict.ContainsKey(texture.Name))
                {
                    texture.Name = replaceDict[texture.Name];
                }
            }

            //TODO: do we need to merge textures?
        }
        static void ReplaceMapObjects(Map map, Dictionary<string, string> replaceDict)
        {
            foreach (ScriptObject obj in map.Objects.ToList())
            {
                if (replaceDict.ContainsKey(obj.Type))
                {
                    obj.Type = replaceDict[obj.Type];
                }
            }
        }
    }
}
