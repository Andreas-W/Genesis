using System;
using System.Collections.Generic;
using System.Linq;

using CkMp.Data.Map;
using CkMp.Data.Objects;
using MapTools.Options;
using MapTools.Utils;

namespace MapTools.Commands
{
    /// <summary>
    /// Turns a map into a four way rotationally symmetric one, built from its bottom left quarter:
    /// the quarter is copied into the other three quarters, rotated by 90, 180 and 270 degrees
    /// around the map centre. Everything outside the bottom left quarter is discarded.
    ///
    /// Only works on square maps, since a 90 degree rotation has to map a quarter onto itself.
    /// </summary>
    public class CloneQuarter
    {
        private const string WaypointIdProperty = "waypointID";

        public const string NotSquareMessage =
            "Clone Quarter needs a square map (equal width and height), " +
            "otherwise a quarter cannot be rotated onto another quarter.";

        public static bool canCloneQuarter(Map map)
        {
            return map != null
                && map.HeightMap != null
                && map.HeightMap.Width == map.HeightMap.Height;
        }

        public static void cloneQuarter(Map map, CloneOptions options)
        {
            if (!canCloneQuarter(map))
                throw new InvalidOperationException(NotSquareMessage);

            if (options.CloneHeightmap)
                cloneQuarterHeightMap(map);

            if (options.CloneTerrain)
                cloneQuarterTiles(map, options);

            cloneQuarterObjects(map, options);

            if (options.CloneAreas)
                cloneQuarterAreas(map, options);
        }

        /// <summary>
        /// A copy is placed by rotating left: (x, y) -> (size - 1 - y, x), the same direction
        /// <see cref="Rotate"/> uses. This walks that backwards to find the cell of the bottom left
        /// quarter that ends up at (x, y), and how many rotations got it there.
        /// Returns false for the cells no rotation of the quarter covers. That only happens on grids
        /// with an odd size, where the centre row and column belong to no quarter.
        /// </summary>
        private static bool getQuarterSource(int size, int x, int y, out int sourceX, out int sourceY, out byte rotations)
        {
            int half = size / 2;

            sourceX = x;
            sourceY = y;

            for (byte k = 0; k < 4; k++)
            {
                if (sourceX < half && sourceY < half)
                {
                    rotations = k;
                    return true;
                }

                // Rotate right, the inverse of the rotation used to place the copies.
                int x2 = sourceY;
                int y2 = size - 1 - sourceX;
                sourceX = x2;
                sourceY = y2;
            }

            sourceX = x;
            sourceY = y;
            rotations = 0;
            return false;
        }

        #region heightmap

        private static void cloneQuarterHeightMap(Map map)
        {
            HeightMap hm = map.HeightMap;
            HeightMap hm2 = new HeightMap(hm.Width, hm.Height, hm.Border);

            for (int i = 0; i < hm.Width; i++)
            {
                for (int j = 0; j < hm.Height; j++)
                {
                    int x, y;
                    byte rotations;
                    getQuarterSource(hm.Width, i, j, out x, out y, out rotations);

                    hm2[i, j] = hm[x, y];
                }
            }

            map.HeightMap = hm2;
        }

        #endregion

        #region tiles

        private static void cloneQuarterTiles(Map map, CloneOptions options)
        {
            TileData td = map.Tiles;
            TileData td2 = new TileData(td.Width, td.Height);

            // NumberOfBlendTiles is always one more than the number of entries in the list.
            td2.NumberOfBlendTiles = 1;

            foreach (var texture in td.Textures)
                td2.AddTexture(texture);

            for (int i = 0; i < td.Width; i++)
            {
                for (int j = 0; j < td.Height; j++)
                {
                    int x, y;
                    byte rotations;
                    getQuarterSource(td.Width, i, j, out x, out y, out rotations);

                    Tile source = td[x, y];
                    var tile = new Tile()
                    {
                        // The texture is tiled over the map, so the tile index depends on the target position.
                        BaseTexture = td.GetTexture(x, y).GetTileIndex(i, j),
                        Impassable = source.Impassable
                    };

                    if (options.CloneBlendTiles)
                    {
                        Func<BlendType, BlendType> rotate = t => TileUtils.getBlendTypeRotated(t, rotations);

                        tile.BlendTexture1 = TileUtils.remapBlendTile(td, td2, source.BlendTexture1, x, y, i, j, rotate);
                        if (tile.BlendTexture1 > 0)
                        {
                            tile.BlendTexture2 = TileUtils.remapBlendTile(td, td2, source.BlendTexture2, x, y, i, j, rotate);
                            if (tile.BlendTexture2 > 0)
                                tile.BlendTexture3 = TileUtils.remapBlendTile(td, td2, source.BlendTexture3, x, y, i, j, rotate);
                        }
                    }

                    td2[i, j] = tile;
                }
            }

            map.Tiles = td2;
        }

        #endregion

        #region objects

        private static void cloneQuarterObjects(Map map, CloneOptions options)
        {
            float centerX = map.HeightMap.MapWidth * 0.5f * 10.0f;
            float centerY = map.HeightMap.MapHeight * 0.5f * 10.0f;

            int nextWaypointId = map.Objects.Select(getWaypointId).DefaultIfEmpty(0).Max() + 1;

            var survivors = new List<IList<ScriptObject>>();
            var sources = new List<IList<ScriptObject>>();

            foreach (var unit in getObjectUnits(map))
            {
                // Leave categories that are not being cloned completely untouched.
                if (unit[0].IsWaypoint ? !options.CloneWaypoints : !options.CloneObjects)
                {
                    survivors.Add(unit);
                    continue;
                }

                if (!isInQuarter(unit, centerX, centerY))
                    continue;

                survivors.Add(unit);
                sources.Add(unit);
            }

            map.Objects.Clear();
            foreach (var unit in survivors)
                foreach (var obj in unit)
                    map.Objects.Add(obj);

            // Waypoint id of a cloned waypoint -> the ids of its three copies, needed to clone the paths.
            var waypointCopies = new Dictionary<int, int[]>();

            // One full pass per rotation, in the original order. Appending all three copies of an
            // object back to back instead would tear road segments apart, because a road is a Start
            // object immediately followed by its End object.
            for (byte rotations = 1; rotations <= 3; rotations++)
            {
                foreach (var unit in sources)
                {
                    foreach (var obj in unit)
                    {
                        ScriptObject copy = ObjectUtils.cloneObject(obj, ObjectUtils.COPY_SUFFIX + rotations);
                        rotateObject(copy, centerX, centerY, rotations);

                        int waypointId = getWaypointId(obj);
                        if (waypointId > 0)
                        {
                            int[] copyIds;
                            if (!waypointCopies.TryGetValue(waypointId, out copyIds))
                            {
                                copyIds = new int[3];
                                waypointCopies[waypointId] = copyIds;
                            }

                            copyIds[rotations - 1] = nextWaypointId;
                            copy.SetProperty(WaypointIdProperty, PropertyType.Integer, nextWaypointId);
                            nextWaypointId++;
                        }

                        map.Objects.Add(copy);
                    }
                }
            }

            if (options.CloneWaypoints)
            {
                var keptWaypoints = new HashSet<int>(
                    sources.SelectMany(u => u).Select(getWaypointId).Where(id => id > 0));

                cloneQuarterPaths(map, keptWaypoints, waypointCopies);
            }
        }

        /// <summary>
        /// Groups the object list into the units that have to be kept or dropped together.
        /// A road segment is a Start object immediately followed by its End object; splitting the
        /// two, or putting anything between them, corrupts the road.
        /// </summary>
        private static List<IList<ScriptObject>> getObjectUnits(Map map)
        {
            var units = new List<IList<ScriptObject>>();

            for (int i = 0; i < map.Objects.Count; i++)
            {
                var unit = new List<ScriptObject>() { map.Objects[i] };

                if (map.Objects[i].RoadOptions.HasFlag(RoadOptions.Start)
                    && i + 1 < map.Objects.Count
                    && map.Objects[i + 1].RoadOptions.HasFlag(RoadOptions.End))
                {
                    unit.Add(map.Objects[i + 1]);
                    i++;
                }

                units.Add(unit);
            }

            return units;
        }

        /// <summary>
        /// A road segment survives as soon as one of its two ends is inside the quarter, so segments
        /// crossing the seam stay whole instead of leaving a dangling end point behind. For a single
        /// object this is just the plain "is it inside" test.
        /// </summary>
        private static bool isInQuarter(IList<ScriptObject> unit, float centerX, float centerY)
        {
            foreach (var obj in unit)
            {
                if (obj.X < centerX && obj.Y < centerY)
                    return true;
            }

            return false;
        }

        private static void rotateObject(ScriptObject obj, float centerX, float centerY, byte rotations)
        {
            for (byte k = 0; k < rotations; k++)
            {
                float dx = obj.X - centerX;
                float dy = obj.Y - centerY;

                obj.X = centerX - dy;
                obj.Y = centerY + dx;
            }

            // Waypoints have no facing, and a road end point's rotation is not a facing either:
            // the segment direction comes from the positions of its two ends.
            if (!obj.IsWaypoint && obj.RoadOptions == RoadOptions.None)
                obj.Rotation = ObjectUtils.clipAngle(obj.Rotation + rotations * (float)Math.PI * 0.5f);
        }

        private static int getWaypointId(ScriptObject obj)
        {
            Property property;
            if (obj.Properties.TryGetValue(WaypointIdProperty, out property)
                && property.Type == PropertyType.Integer)
            {
                return (int)property.Value;
            }

            return 0;
        }

        /// <summary>
        /// Drops the waypoint paths that lost an end outside the quarter and rebuilds the surviving
        /// ones for each of the three copies.
        /// </summary>
        private static void cloneQuarterPaths(Map map, HashSet<int> keptWaypoints, IDictionary<int, int[]> waypointCopies)
        {
            var paths = map.Paths.ToList();
            map.Paths.Clear();

            foreach (var path in paths)
            {
                if (!keptWaypoints.Contains(path.Item1) || !keptWaypoints.Contains(path.Item2))
                    continue;

                map.Paths.Add(path);

                int[] from;
                int[] to;
                if (waypointCopies.TryGetValue(path.Item1, out from) && waypointCopies.TryGetValue(path.Item2, out to))
                {
                    for (int k = 0; k < 3; k++)
                        map.Paths.Add(new Tuple<int, int>(from[k], to[k]));
                }
            }
        }

        #endregion

        #region areas

        private static void cloneQuarterAreas(Map map, CloneOptions options)
        {
            float centerX = map.HeightMap.MapWidth * 0.5f * 10.0f;
            float centerY = map.HeightMap.MapHeight * 0.5f * 10.0f;

            foreach (var area in map.Areas.ToList())
            {
                // Skipped areas stay exactly as they are: neither removed nor cloned.
                if (isSkipped(area, options))
                    continue;

                // If no point is inside the quarter, the area is gone.
                if (!area.Points.Exists(p => p[0] < centerX && p[1] < centerY))
                    map.Areas.Remove(area);
            }

            var sources = map.Areas.Where(a => !isSkipped(a, options)).ToList();
            int nextId = map.Areas.Select(a => a.Id).DefaultIfEmpty(0).Max() + 1;

            for (byte rotations = 1; rotations <= 3; rotations++)
            {
                var copies = new List<Area>();

                foreach (var area in sources)
                {
                    var copy = new Area()
                    {
                        Id = nextId++,
                        IsRiver = area.IsRiver,
                        IsWater = area.IsWater,
                        Name = area.Name,
                        RiverStart = area.RiverStart
                    };

                    foreach (var point in area.Points)
                        copy.Points.Add(rotatePoint(point, centerX, centerY, rotations));

                    copies.Add(copy);
                }

                ObjectUtils.renameAreas(map.Areas.ToList(), copies);

                foreach (var copy in copies)
                    map.Areas.Add(copy);
            }
        }

        private static bool isSkipped(Area area, CloneOptions options)
        {
            return options.SkipDefaultWater && ObjectUtils.isDefaultWater(area);
        }

        private static int[] rotatePoint(int[] point, float centerX, float centerY, byte rotations)
        {
            float x = point[0];
            float y = point[1];

            for (byte k = 0; k < rotations; k++)
            {
                float dx = x - centerX;
                float dy = y - centerY;

                x = centerX - dy;
                y = centerY + dx;
            }

            return new int[] { Convert.ToInt32(x), Convert.ToInt32(y), point[2] };
        }

        #endregion
    }
}
