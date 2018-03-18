using System.Collections.Generic;
using System.IO;
using RpgMakerEncoder.Ruby;

namespace RpgMakerEncoder.RpgMaker.UserDefined
{
    public static class Tone
    {
        public static RubyObject Decode(BinaryReader reader)
        {
            return new RubyObject
            {
                Properties = new Dictionary<string, RubyToken>(4)
                {
                    ["red"] = new RubyValue(reader.ReadDouble()),
                    ["green"] = new RubyValue(reader.ReadDouble()),
                    ["blue"] = new RubyValue(reader.ReadDouble()),
                    ["gray"] = new RubyValue(reader.ReadDouble())
                }
            };
        }

        public static byte[] Encode(RubyObject ruby)
        {
            var bytes = new byte[32];
            using (var stream = new MemoryStream(bytes))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(ruby["red"].GetValue<double>());
                    writer.Write(ruby["green"].GetValue<double>());
                    writer.Write(ruby["blue"].GetValue<double>());
                    writer.Write(ruby["gray"].GetValue<double>());
                }
            }
            return bytes;
        }
    }
}