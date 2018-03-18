using System.IO;
using RpgMakerEncoder.Ruby;

namespace RpgMakerEncoder.Encoding
{
    public interface IRubyEncoder
    {
        void Encode(RubyToken token, string file);
        void Encode(RubyToken token, string file, RubyEncoderOptions options);
        void Encode(RubyToken token, Stream stream);
        void Encode(RubyToken token, Stream stream, RubyEncoderOptions options);
        RubyToken Decode(string file);
        RubyToken Decode(string file, RubyDecoderOptions options);
        RubyToken Decode(Stream stream);
        RubyToken Decode(Stream stream, RubyDecoderOptions options);
    }
}