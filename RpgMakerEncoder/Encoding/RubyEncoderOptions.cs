using System;
using System.Collections.Generic;
using RpgMakerEncoder.Ruby;

namespace RpgMakerEncoder.Encoding
{
    public class RubyEncoderOptions
    {
        public IDictionary<string, Func<RubyObject, byte[]>> UserEncoders { get; set; } = new Dictionary<string, Func<RubyObject, byte[]>>();
    }
}