using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace PeminSpectrumData
{
    public class XMLSerializer<T>
    {
        readonly XmlSerializerNamespaces xmlNamespaces;
        readonly XmlSerializer writeSerializer;
        readonly XmlSerializer readSerializer;

        public XMLSerializer()
        {
            xmlNamespaces = new XmlSerializerNamespaces();
            xmlNamespaces.Add("", "");

            writeSerializer = new XmlSerializer(typeof(T));
            readSerializer = new XmlSerializer(typeof(T));

            writeSerializer.UnknownNode += (sender, e) => { };
            writeSerializer.UnknownAttribute += (sender, e) => { };
            readSerializer.UnknownNode += (sender, e) => { };
            readSerializer.UnknownAttribute += (sender, e) => { };
        }

        public void SaveToFile(string filePath, T data)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writeSerializer.Serialize(writer, data, xmlNamespaces);
                writer.Close();
            }
        }

        public T ReadFromFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                return (T)readSerializer.Deserialize(fs);
            }
        }

        public void SaveToStream(Stream stream, T data) => 
            writeSerializer.Serialize(stream, data, xmlNamespaces);

        public void ReadFromStream(Stream stream, out T result) => 
            result = (T)readSerializer.Deserialize(stream);


        public string SaveToString(T data)
        {
            MemoryStream stream = new MemoryStream();
            TextWriter writer = new StreamWriter(stream);
            writeSerializer.Serialize(writer, data, xmlNamespaces);
            return Encoding.Default.GetString(stream.ToArray());
        }

        public T LoadFromString(string data)
            => (T)readSerializer.Deserialize(new MemoryStream(Encoding.ASCII.GetBytes(data)));
    }
}

