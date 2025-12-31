using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CkMp.Data.Objects;

namespace MapTools.Utils
{
    public class ObjectUtils
    {

        public static string COPY_SUFFIX = "_Copy";
        public static List<string> PROPERTY_RENAME_KEYS = new List<string>( new string[]
        {
            "uniqueID",
            "waypointName",
            "waypointPathLabel1",
            "waypointPathLabel2",
            "waypointPathLabel3",
            "objectName"
        });
        public static void renameAreas(List<Area> oldAreas, List<Area> newAreas)
        {
            foreach(var area in newAreas.ToList())
            {
                int i = 0;
                string name = area.Name;
                while (oldAreas.Exists(a=> a.Name == area.Name))
                {
                    if (i == 0) area.Name = name + COPY_SUFFIX;
                    else area.Name = name + COPY_SUFFIX + i;
                    i++;
                }
            }
        }

        //Obsolete
        public static void renameObjects(List<ScriptObject> oldObjects, List<ScriptObject> newObjects)
        {
            List<string> rename_keys = new List<string>(PROPERTY_RENAME_KEYS);

            foreach (var obj in newObjects.ToList())
            {            
                var keys = obj.Properties.Keys.Where(k => rename_keys.Contains(k));
                foreach (var k in keys)
                {
                    var p = obj.Properties[k];
                    if (p.Type == PropertyType.OneByteString || p.Type == PropertyType.TwoByteString) {
                        p.Value = (string)p.Value + COPY_SUFFIX;
                    }                    
                }              
            }
        }

        public static ScriptObject cloneObject(ScriptObject src)
        {

            ScriptObject other = new ScriptObject();
            other.X = src.X;
            other.Y = src.Y;
            other.Z = src.Z;
            other.Type = src.Type;
            other.Rotation = src.Rotation;
            other.RoadOptions = src.RoadOptions;
            other.Properties = new Dictionary<String, Property>();
            foreach (var k in src.Properties.Keys)
            {
                var p = src.Properties[k];
                var val = p.Value;
                if ((p.Type == PropertyType.OneByteString || p.Type == PropertyType.TwoByteString) && PROPERTY_RENAME_KEYS.Contains(k))
                {
                    val = (string)p.Value + COPY_SUFFIX;
                }
                other.SetProperty(k, p.Type, val);
            }

            return other;
        }

        public static float clipAngle(float angle)
        {
            while (angle < 0f)
            {
                angle += (float)(Math.PI * 2.0);
            }
            while (angle > Math.PI * 2f)
            {
                angle -= (float)(Math.PI * 2.0);
            }
            return angle;
        }
    }
}
