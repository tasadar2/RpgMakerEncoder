using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RpgMakerEncoder.IO;

namespace RpgMakerEncoder.Encoding
{
    public class SimpleJsonFileProcessor : IJsonFileProcessor
    {
        public virtual void Save(JToken token, string file)
        {
            DirectoryHelper.EnsureDirectoryExists(Path.GetDirectoryName(file));
            WriteJsonToken(token, file);
        }

        public virtual JToken Load(string file)
        {
            return JToken.Parse(File.ReadAllText(file));
        }

        private static void WriteJsonToken(JToken token, string file)
        {
            using (var stream = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var textWriter = new StreamWriter(stream))
                {
                    using (var writer = new JsonTextWriter(textWriter))
                    {
                        writer.Formatting = Formatting.Indented;
                        token.WriteTo(writer);
                    }
                }
            }
        }
    }
}