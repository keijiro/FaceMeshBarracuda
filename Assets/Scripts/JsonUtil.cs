using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

public static class JsonUtil
{
    // 指定したオブジェクトをJSONに変換する
    public static string SerializeJsonToString<T>(T src, bool useIndent = false)
    {
        using (var ms = new MemoryStream())
        {
            var settings = new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true, // Dictionary を「"key" : "value"」形式で出力するための指定
            };
            var serializer = new DataContractJsonSerializer(typeof(T), settings);

            if (useIndent)
            {
                // インデント付きで出力するための指定
                using (var writer =
                    JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.UTF8, false, true, "  "))
                {
                    serializer.WriteObject(writer, src);
                    writer.Flush(); // ★忘れずに
                }
            }
            else
            {
                serializer.WriteObject(ms, src); // インデント無し
            }

            using (var sr = new StreamReader(ms))
            {
                ms.Position = 0;
                return sr.ReadToEnd();
            }
        }
    }

    public static T Deserialize<T>(string message)
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
        {
            //var setting = new DataContractJsonSerializerSettings()
            //{
            //    UseSimpleDictionaryFormat = true,
            //};
            var serializer = new DataContractJsonSerializer(typeof(T)/*, setting*/);
            return (T)serializer.ReadObject(stream);
        }
    }
}