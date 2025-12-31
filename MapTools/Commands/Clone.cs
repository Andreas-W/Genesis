using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CkMp.Data;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using MapTools.Utils;
using MapTools.Options;

namespace MapTools.Commands
{
    public class Clone
    {
        public static byte MODE_FIRSTHALF_OVERRIDE;
        public static void cloneAndMirror(Map map, bool vertical, CloneOptions options)
        {
            if (options.CloneHeightmap)
                cloneMirrorHeightMap(map, vertical, options);

            if (options.CloneTerrain)
                cloneMirrorTiles(map, vertical, options);

            cloneMirrorObjects(map, vertical, options, true);

            if (options.CloneAreas)
                cloneMirrorAreas(map, vertical, options, true);
        }

        public static void cloneAndRotate(Map map, bool vertical, CloneOptions options)
        {
            if (options.CloneHeightmap)
                cloneRotateHeightMap(map, vertical, options);

            if (options.CloneTerrain)
                cloneRotateTiles(map, vertical, options);

            cloneRotateObjects(map, vertical, options, true);

            if (options.CloneAreas)
                cloneRotateAreas(map, vertical, options, true);
        }

        private static void cloneMirrorHeightMap(Map map, bool vertical, CloneOptions options)
        {
            HeightMap hm = map.HeightMap;
            HeightMap hm2 = new HeightMap(hm.Width, hm.Height, map.HeightMap.Border);

            for (int i = 0; i < hm.Width; i++)
            {
                for (int j = 0; j < hm.Height; j++)
                {
                    if (vertical) //flip y
                    {
                        if (j > hm.Height / 2)
                        {
                            hm2[i,j] = hm[i, hm.Height - 1 - j];
                        }else
                        {
                            hm2[i, j] = hm[i, j]; 
                        }
                    }
                    else //flip x
                    {
                        if (i > hm.Width / 2)
                        {
                            hm2[i, j] = hm[hm.Width - 1 - i, j];
                        }
                        else
                        {
                            hm2[i, j] = hm[i, j];
                        }
                    }
                }
            }
            map.HeightMap = hm2;
        }

        private static void cloneMirrorTiles(Map map, bool vertical, CloneOptions options)
        {
            TileData td = map.Tiles;
//            TileData td2 = new TileData(td.Width, td.Height);

            //td2.Textures = td.Textures;
            //td2.NumberOfBaseTiles = td.NumberOfBaseTiles;
            //td2.NumberOfBlendTiles = td.NumberOfBlendTiles;

            //foreach (var tex in td.Textures)
            //{
            //    td2.AddTexture(tex);
            //}

            //td2.BlendTiles = td.BlendTiles;
            //foreach (var bt in td.BlendTiles.ToList())
            //{
            //    BlendType type = bt.BlendType; // TileUtils.getBlendTypeMirrored(bt.BlendType, vertical);
            //    td2.GetBlendTileIndex(bt.TileIndex, type);
            //}

            //td2.BlendTiles = td.BlendTiles;

            for (int i = 0; i < td.Width; i++)
            {
                for (int j = 0; j < td.Height; j++)
                {
                    bool flip = false;
                    int x = i, y = j;
                    if (vertical) //flip y
                    {
                        if (j > td.Height / 2)
                        {
                            y = td.Height - 1 - j;
                            flip = true;
                        }
                    }
                    else //flip x
                    {
                        if (i > td.Width / 2)
                        {
                            x = td.Width - 1 - i;
                            flip = true;
                        }
                    }

                    if (flip)
                    {
                        Tile tile = null;
                        if (!options.CloneBlendTiles)
                        {
                            tile = new Tile()
                            {
                                BaseTexture = td.GetTexture(x, y).GetTileIndex(i, j),
                                Impassable = td[x, y].Impassable
                            };
                        }
                        else
                        {
                            tile = new Tile()
                            {
                                BaseTexture = td.GetTexture(x, y).GetTileIndex(i, j),
                                //BaseTexture = td[x, y].BaseTexture,
                                BlendTexture1 = td[x, y].BlendTexture1,
                                BlendTexture2 = td[x, y].BlendTexture2,
                                BlendTexture3 = td[x, y].BlendTexture3,
                                Impassable = td[x, y].Impassable
                            };

                            if (tile.BlendTexture1 > 0)
                            {
                                BlendTile bt = td.BlendTiles[tile.BlendTexture1 - 1];
                                var tex = td.Textures.Find(t => t.GetTileIndex(x, y) == bt.TileIndex);
                                var tileIndex = tex.GetTileIndex(i, j);
                                tile.BlendTexture1 = td.GetBlendTileIndex(tileIndex, TileUtils.getBlendTypeMirrored(bt.BlendType, vertical));
                                if (tile.BlendTexture2 > 0)
                                {
                                    bt = td.BlendTiles[tile.BlendTexture2 - 1];
                                    tex = td.Textures.Find(t => t.GetTileIndex(x, y) == bt.TileIndex);
                                    tileIndex = tex.GetTileIndex(i, j);
                                    tile.BlendTexture2 = td.GetBlendTileIndex(tileIndex, TileUtils.getBlendTypeMirrored(bt.BlendType, vertical));
                                    if (tile.BlendTexture3 > 0)
                                    {
                                        bt = td.BlendTiles[tile.BlendTexture3 - 1];
                                        tex = td.Textures.Find(t => t.GetTileIndex(x, y) == bt.TileIndex);
                                        tileIndex = tex.GetTileIndex(i, j);
                                        tile.BlendTexture3 = td.GetBlendTileIndex(tileIndex, TileUtils.getBlendTypeMirrored(bt.BlendType, vertical));
                                    }
                                }
                            }
                        }
                        td[i, j] = tile;
                    }
                }
            }
        }

        private static void cloneMirrorObjects(Map map, bool vertical, CloneOptions options, bool autoRename)
        {
            //old center
            float centerX = map.HeightMap.MapWidth * 0.5f * 10.0f;
            float centerY = map.HeightMap.MapHeight * 0.5f * 10.0f;

            List<ScriptObject> newObjects = new List<ScriptObject>();

            foreach (var obj in map.Objects.ToList())
            {
                if (obj.IsWaypoint)
                {
                    if (!options.CloneWaypoints) continue;
                }else
                {
                    if (!options.CloneObjects) continue;
                }                   

                if (vertical) //flip y
                {
                    if (obj.Y < centerY) //duplicate
                    {
                        ScriptObject obj2 = ObjectUtils.cloneObject(obj);
                        if (!obj2.IsWaypoint && obj2.RoadOptions == RoadOptions.None)
                        {
                            obj2.Rotation = ObjectUtils.clipAngle(((float)Math.PI * 2.0f) - obj2.Rotation);
                        }
                        float dy = obj2.Y - centerY;

                        obj2.Y = centerY - dy;
                        newObjects.Add(obj2);
                    }else //Remove
                    {
                        map.Objects.Remove(obj);
                    }
                }
                else //flip x
                {
                    if (obj.X < centerX) //duplicate
                    {
                        ScriptObject obj2 = ObjectUtils.cloneObject(obj);
                        if (!obj2.IsWaypoint && obj2.RoadOptions == RoadOptions.None)
                        {
                            obj2.Rotation = ObjectUtils.clipAngle(((float)Math.PI) - obj2.Rotation);
                        }
                        float dx = obj2.X - centerX;
                        obj2.X = centerX - dx;
                        newObjects.Add(obj2);
                    }
                    else
                    {
                        map.Objects.Remove(obj);
                    }
                   
                }
            }
            //obsolete
            //ObjectUtils.renameObjects(map.Objects.ToList(), newObjects);
            foreach (var o in newObjects)
            {
                map.Objects.Add(o);
            }
        }

        private static void cloneMirrorAreas(Map map, bool vertical, CloneOptions options, bool autoRename)
        {
            //old center
            float centerX = map.HeightMap.MapWidth * 0.5f * 10.0f;
            float centerY = map.HeightMap.MapHeight * 0.5f * 10.0f;

            List<Area> newAreas = new List<Area>();
            foreach (var a in map.Areas.ToList())
            {
                if (a.Name.Equals("DefaultWater"))
                    continue;
                if ((vertical && !a.Points.Exists(p => (float)p[1] < centerY)) || 
                    (!vertical && !a.Points.Exists(p => (float)p[0] < centerX)))
                { //If no point is within half, remove area
                    map.Areas.Remove(a);
                }
                else
                {
                    Area a2 = new Area() { Id = a.Id, IsRiver = a.IsRiver, IsWater = a.IsWater, Name = a.Name, RiverStart = a.RiverStart };
                    foreach (var point in a.Points)
                    {
                        var point2 = new int[3];
                        if (vertical) //flip y
                        {
                            point2[0] = point[0];
                            float dy = (float)point[1] - centerY;
                            point2[1] = Convert.ToInt32(-dy + centerY);
                        }
                        else //flip x
                        {
                            float dx = (float)point[0] - centerX;
                            point2[0] = Convert.ToInt32(-dx + centerX);
                            point2[1] = point[1];
                        }
                        a2.Points.Add(point2);
                    }
                    newAreas.Add(a2);
                }
            }
            ObjectUtils.renameAreas(map.Areas.ToList(), newAreas);
            foreach (var a in newAreas) {
                map.Areas.Add(a);
            }
        }



        //--------------------

        private static void cloneRotateHeightMap(Map map, bool vertical, CloneOptions options)
        {
            HeightMap hm = map.HeightMap;
            HeightMap hm2 = new HeightMap(hm.Width, hm.Height, map.HeightMap.Border);


            for (int i = 0; i < hm.Width; i++)
            {
                for (int j = 0; j < hm.Height; j++)
                {
                    if ((vertical && j > hm.Height / 2) || (!vertical && i > hm.Width / 2))
                    { //flip x and y
                        hm2[i, j] = hm[hm.Width - 1 - i, hm.Height - 1 - j];
                    }else
                    {
                        hm2[i, j] = hm[i, j];
                    }
                }
            }
            map.HeightMap = hm2;
        }

        private static void cloneRotateTiles(Map map, bool vertical, CloneOptions options)
        {
            TileData td = map.Tiles;
            TileData td2 = new TileData(td.Width, td.Height);

            foreach (var tex in td.Textures)
            {
                td2.AddTexture(tex);
            }

            //td2.BlendTiles = td.BlendTiles;
            foreach (var bt in td.BlendTiles.ToList())
            {
                BlendType type = bt.BlendType;
                td2.GetBlendTileIndex(bt.TileIndex, type);
            }

            //td2.BlendTiles = td.BlendTiles;

            for (int i = 0; i < td.Width; i++)
            {
                for (int j = 0; j < td.Height; j++)
                {
                    bool flip = false;
                    int x = i, y = j;
                    if ((vertical && j > td.Height / 2) || (!vertical && i > td.Width / 2))
                    {
                        x = td.Width - 1 - i;
                        y = td.Height - 1 - j;
                        flip = true;
                    }

                    var tile = new Tile()
                    {
                        BaseTexture = td.GetTexture(x, y).GetTileIndex(i, j),
                        //BaseTexture = td[x, y].BaseTexture,
                        BlendTexture1 = td[x, y].BlendTexture1,
                        BlendTexture2 = td[x, y].BlendTexture2,
                        BlendTexture3 = td[x, y].BlendTexture3,
                        Impassable = td[x, y].Impassable
                    };


                    if (flip)
                    {
                        if (tile.BlendTexture1 > 0)
                        {
                            BlendTile bt = td.BlendTiles[tile.BlendTexture1 - 1];
                            var tex = td2.Textures.Find(t => t.GetTileIndex(x, y) == bt.TileIndex);
                            var tileIndex = tex.GetTileIndex(i, j);
                            tile.BlendTexture1 = td2.GetBlendTileIndex(tileIndex, TileUtils.getBlendTypeMirrored(TileUtils.getBlendTypeMirrored(bt.BlendType, vertical), !vertical));
                            if (tile.BlendTexture2 > 0)
                            {
                                bt = td.BlendTiles[tile.BlendTexture2 - 1];
                                tex = td2.Textures.Find(t => t.GetTileIndex(x, y) == bt.TileIndex);
                                tileIndex = tex.GetTileIndex(i, j);
                                tile.BlendTexture2 = td2.GetBlendTileIndex(tileIndex, TileUtils.getBlendTypeMirrored(TileUtils.getBlendTypeMirrored(bt.BlendType, vertical), !vertical));
                                if (tile.BlendTexture3 > 0)
                                {
                                    bt = td.BlendTiles[tile.BlendTexture3 - 1];
                                    tex = td2.Textures.Find(t => t.GetTileIndex(x, y) == bt.TileIndex);
                                    tileIndex = tex.GetTileIndex(i, j);
                                    tile.BlendTexture3 = td2.GetBlendTileIndex(tileIndex, TileUtils.getBlendTypeMirrored(TileUtils.getBlendTypeMirrored(bt.BlendType, vertical), !vertical));
                                }
                            }
                        }
                    }
                    td2[i, j] = tile;
                }
            }
            map.Tiles = td2;
        }

        private static void cloneRotateObjects(Map map, bool vertical, CloneOptions options, bool autoRename)
        {
            //old center
            float centerX = map.HeightMap.MapWidth * 0.5f * 10.0f;
            float centerY = map.HeightMap.MapHeight * 0.5f * 10.0f;

            List<ScriptObject> newObjects = new List<ScriptObject>();

            foreach (var obj in map.Objects.ToList())
            {
                if ((vertical && obj.Y < centerY) || (!vertical && obj.X < centerX)) //flip y
                {
                    ScriptObject obj2 = ObjectUtils.cloneObject(obj);
                    if (!obj2.IsWaypoint)
                    {
                        obj2.Rotation = (obj.Rotation + (float)Math.PI) % ((float)Math.PI * 2.0f);
                    }
                    float dx = obj2.X - centerX;
                    float dy = obj2.Y - centerY;
                    obj2.X = centerX - dx;
                    obj2.Y = centerY - dy;
                    newObjects.Add(obj2);
                    }
                else //Remove
                {
                    map.Objects.Remove(obj);
                }
                
            }
            //obsolete
            //ObjectUtils.renameObjects(map.Objects.ToList(), newObjects);
            foreach (var o in newObjects)
            {
                map.Objects.Add(o);
            }
        }

        private static void cloneRotateAreas(Map map, bool vertical, CloneOptions options, bool autoRename)
        {
            //old center
            float centerX = map.HeightMap.MapWidth * 0.5f * 10.0f;
            float centerY = map.HeightMap.MapHeight * 0.5f * 10.0f;

            List<Area> newAreas = new List<Area>();
            foreach (var a in map.Areas.ToList())
            {
                if (a.Name == "DefaultWater")
                    continue;

                if ((vertical && !a.Points.Exists(p => (float)p[1] < centerY)) ||
                    (!vertical && !a.Points.Exists(p => (float)p[0] < centerX)))
                { //If no point is within half, remove area
                    map.Areas.Remove(a);
                }
                else
                {
                    Area a2 = new Area() { Id = a.Id, IsRiver = a.IsRiver, IsWater = a.IsWater, Name = a.Name, RiverStart = a.RiverStart };
                    foreach (var point in a.Points)
                    {
                        var point2 = new int[3];

                        float dy = (float)point[1] - centerY;
                        point2[1] = Convert.ToInt32(-dy + centerY);

                        float dx = (float)point[0] - centerX;
                        point2[0] = Convert.ToInt32(-dx + centerX);
                        a2.Points.Add(point2);
                    }
                    newAreas.Add(a2);
                }
            }
            ObjectUtils.renameAreas(map.Areas.ToList(), newAreas);
            foreach (var a in newAreas)
            {
                map.Areas.Add(a);
            }
        }
    }
}
