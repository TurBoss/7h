/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iros._7th {
    public static class Util {
        public static T Deserialize<T>(System.IO.Stream s) {
            var ser = new System.Xml.Serialization.XmlSerializer(typeof(T));
            return (T)ser.Deserialize(s);
        }
        public static T Deserialize<T>(string file) {
            using (var fs = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                return Deserialize<T>(fs);
            }
        }
        public static T DeserializeString<T>(string data) {
            var ser = new System.Xml.Serialization.XmlSerializer(typeof(T));
            return (T)ser.Deserialize(new System.IO.StringReader(data));
        }

        public static void SerializeBinary(object o, System.IO.Stream s) {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter fmt = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            fmt.Serialize(s, o);
        }

        public static T DeserializeBinary<T>(System.IO.Stream s) {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter fmt = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            return (T)fmt.Deserialize(s);
        }

        public static string Serialize(object o) {
            var ser = new System.Xml.Serialization.XmlSerializer(o.GetType());
            var sw = new System.IO.StringWriter();
            ser.Serialize(sw, o);
            return sw.ToString();
        }
        public static void Serialize(object o, System.IO.Stream s) {
            var ser = new System.Xml.Serialization.XmlSerializer(o.GetType());
            ser.Serialize(s, o);
        }
    }

    public static class XmlUtil {
        public static string NodeTextS(this System.Xml.XmlNode node) {
            return node == null ? String.Empty : node.InnerText;
        }
    }
}
