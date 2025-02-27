using System;
using System.IO;
using System.Xml.Serialization;

namespace ERad5TestGUI.Helpers
{
    public class XmlHelper
    {
        public static void SerializeToXml<T>(T obj, string filePath)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                using (TextWriter writer = new StreamWriter(filePath))
                {
                    xs.Serialize(writer, obj);
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            
        }

        public static T DeserializeFromXml<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return default;
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    return (T)xs.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            return default;
        }
    }
}
