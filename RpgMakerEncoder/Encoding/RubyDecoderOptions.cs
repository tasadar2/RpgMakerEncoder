using System;
using System.Collections.Generic;
using System.IO;
using RpgMakerEncoder.Ruby;

namespace RpgMakerEncoder.Encoding
{
    public class RubyDecoderOptions
    {
        public IDictionary<string, Func<BinaryReader, RubyObject>> UserDecoders { get; set; } = new Dictionary<string, Func<BinaryReader, RubyObject>>();
    }
}