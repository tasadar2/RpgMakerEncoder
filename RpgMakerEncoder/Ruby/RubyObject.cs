using System;
using System.Collections.Generic;

namespace RpgMakerEncoder.Ruby
{
    public class RubyObject : RubyToken
    {
        public override RubyTokenType Type { get; } = RubyTokenType.Object;
        public RubyClass RubyClass { get; set; } = new RubyClass();
        public Dictionary<string, RubyToken> Properties { get; set; } = new Dictionary<string, RubyToken>();

        /// <inheritdoc />
        public override RubyToken this[object key]
        {
            get
            {
                if (key is string propertyKey)
                {
                    return Properties[propertyKey];
                }
                throw new ArgumentException($"Argument of type {key.GetType().Name} is not a valid property key.");
            }
        }

        public RubyToken this[string key]
        {
            get => Properties[key];
            set => Properties[key] = value;
        }
    }
}