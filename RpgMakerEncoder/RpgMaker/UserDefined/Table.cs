using System.Collections.Generic;
using System.IO;
using RpgMakerEncoder.Ruby;

namespace RpgMakerEncoder.RpgMaker.UserDefined
{
    public static class Table
    {
        public static RubyObject Decode(BinaryReader reader)
        {
            var ruby = new RubyObject
            {
                Properties = new Dictionary<string, RubyToken>(5)
                {
                    ["reserved"] = new RubyValue(reader.ReadInt32()),
                    ["xsize"] = new RubyValue(reader.ReadInt32()),
                    ["ysize"] = new RubyValue(reader.ReadInt32()),
                    ["zsize"] = new RubyValue(reader.ReadInt32())
                }
            };

            var size = reader.ReadInt32();
            var data = new RubyArray
            {
                Array = new List<RubyToken>(size)
            };
            for (var index = 0; index < size; index++)
            {
                data.Array.Add(new RubyValue(reader.ReadInt16()));
            }
            ruby.Properties["data"] = data;

            return ruby;
        }

        public static byte[] Encode(RubyObject ruby)
        {
            var xsize = ruby["xsize"].GetValue<int>();
            var ysize = ruby["ysize"].GetValue<int>();
            var zsize = ruby["zsize"].GetValue<int>();
            var size = xsize * ysize * zsize;
            var bytes = new byte[size * 2 + 20];
            using (var stream = new MemoryStream(bytes))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(ruby["reserved"].GetValue<int>());
                    writer.Write(xsize);
                    writer.Write(ysize);
                    writer.Write(zsize);
                    writer.Write(size);

                    foreach (var value in ruby["data"])
                    {
                        writer.Write(value.GetValue<short>());
                    }
                }
            }
            return bytes;
        }
    }
}