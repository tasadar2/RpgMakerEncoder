using System.Linq;
using Newtonsoft.Json.Linq;
using RpgMakerEncoder.Ruby;

namespace RpgMakerEncoder.Conversion
{
    public class JTokenConverter : IConvert<RubyToken, JToken>
    {
        public JToken Convert(RubyToken ruby)
        {
            switch (ruby.Type)
            {
                case RubyTokenType.Array:
                    var arrayToken = new JArray();
                    foreach (var item in ruby)
                    {
                        arrayToken.Add(Convert(item));
                    }
                    return arrayToken;
                case RubyTokenType.Object:
                    var rubyObject = (RubyObject)ruby;
                    var objectToken = new JObject(new JProperty("typeName", new JValue(rubyObject.RubyClass.Name)));
                    foreach (var property in rubyObject.Properties.OrderBy(prop => prop.Key))
                    {
                        objectToken[property.Key] = Convert(property.Value);
                    }
                    return objectToken;
                case RubyTokenType.Null:
                    return new JValue((object)null);
                case RubyTokenType.Boolean:
                    return new JValue(ruby.GetValue<bool>());
                case RubyTokenType.String:
                    return new JValue(ruby.GetValue<string>());
                default:
                    return new JValue(((RubyValue)ruby).Value);
            }
        }
    }
}