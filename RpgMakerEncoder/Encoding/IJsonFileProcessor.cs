using Newtonsoft.Json.Linq;

namespace RpgMakerEncoder.Encoding
{
    public interface IJsonFileProcessor
    {
        void Save(JToken token, string file);
        JToken Load(string file);
    }
}