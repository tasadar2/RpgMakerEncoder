using System.Collections.Generic;

namespace RpgMakerEncoder.Ruby
{
    public interface IRubyEnumerable<out TType> : IEnumerable<TType>
        where TType : RubyToken
    {
        TType this[object key] { get; }
    }
}