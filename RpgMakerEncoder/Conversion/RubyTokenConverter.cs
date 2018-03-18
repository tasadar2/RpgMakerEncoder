using Newtonsoft.Json.Linq;
using RpgMakerEncoder.Ruby;

namespace RpgMakerEncoder.Conversion
{
    public class RubyTokenConverter : IConvert<JToken, RubyToken>
    {
        public RubyToken Convert(JToken json)
        {
            switch (json.Type)
            {
                case JTokenType.Array:
                    var arrayToken = new RubyArray();
                    foreach (var item in (JArray)json)
                    {
                        arrayToken.Add(Convert(item));
                    }
                    return arrayToken;
                case JTokenType.Object:
                    var objectToken = new RubyObject();
                    var jsonObject = (JObject)json;
                    foreach (var property in jsonObject)
                    {
                        if (property.Key == "typeName")
                        {
                            objectToken.RubyClass.Name = property.Value.Value<string>();
                        }
                        else
                        {
                            objectToken[property.Key] = Convert(property.Value);
                        }
                    }
                    return objectToken;
                case JTokenType.Null:
                    return new RubyValue((object)null);
                case JTokenType.Boolean:
                    return new RubyValue(json.Value<bool>());
                case JTokenType.String:
                    return new RubyValue(json.Value<string>());
                default:
                    return new RubyValue(((JValue)json).Value);
            }
        }
    }
}