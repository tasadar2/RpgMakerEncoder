using System;
using System.Collections;
using System.Collections.Generic;

namespace RpgMakerEncoder.Ruby
{
    public abstract class RubyToken : IRubyEnumerable<RubyToken>
    {
        public abstract RubyTokenType Type { get; }

        public virtual TType GetValue<TType>()
        {
            throw new InvalidOperationException($"Cannot convert Ruby type of {GetType().Name}, subtype of {Type}, to requested type of {typeof(TType).Name}.");
        }

        /// <inheritdoc />
        public virtual IEnumerator<RubyToken> GetEnumerator()
        {
            throw new InvalidOperationException($"Ruby type of {GetType().Name}, subtype of {Type}, is not enumerable.");
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public virtual RubyToken this[object key] => throw new InvalidOperationException("");
    }
}