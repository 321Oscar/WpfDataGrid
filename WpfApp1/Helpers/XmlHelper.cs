using System.IO;
using System.Xml.Serialization;

namespace WpfApp1.Helpers
{
    public class XmlHelper
    {
        public static void SerializeToXml<T>(T obj, string filePath)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (TextWriter writer = new StreamWriter(filePath))
            {
                xs.Serialize(writer, obj);
            }
        }

        public static T DeserializeFromXml<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return default(T);

            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                return (T)xs.Deserialize(fs);
            }
        }
    }
}
