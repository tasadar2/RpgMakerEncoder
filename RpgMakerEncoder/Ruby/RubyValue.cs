using System;

namespace RpgMakerEncoder.Ruby
{
    public class RubyValue : RubyToken
    {
        public override RubyTokenType Type { get; }
        public object Value { get; set; }

        public RubyValue(RubyValue value)
        {
            Type = value.Type;
            Value = value.Value;
        }

        public RubyValue(bool value)
        {
            Type = RubyTokenType.Boolean;
            Value = value;
        }

        public RubyValue(int value)
        {
            Type = RubyTokenType.Number;
            Value = value;
        }

        public RubyValue(long value)
        {
            Type = RubyTokenType.Number;
            Value = value;
        }

        public RubyValue(decimal value)
        {
            Type = RubyTokenType.Float;
            Value = value;
        }

        public RubyValue(char value)
        {
            Type = RubyTokenType.String;
            Value = value;
        }

        public RubyValue(ulong value)
        {
            Type = RubyTokenType.Number;
            Value = value;
        }

        public RubyValue(double value)
        {
            Type = RubyTokenType.Float;
            Value = value;
        }

        public RubyValue(float value)
        {
            Type = RubyTokenType.Float;
            Value = value;
        }

        public RubyValue(string value)
        {
            Type = RubyTokenType.String;
            Value = value;
        }

        public RubyValue(object value)
        {
            Type = GetValueType(value);
            Value = value;
        }

        private static RubyTokenType GetValueType(object value)
        {
            switch (value)
            {
                case null:
                    return RubyTokenType.Null;
                case string _:
                    return RubyTokenType.String;
                case long _:
                case int _:
                case short _:
                case sbyte _:
                case ulong _:
                case uint _:
                case ushort _:
                case byte _:
                    return RubyTokenType.Number;
                case Enum _:
                    return RubyTokenType.Number;
                case double _:
                case float _:
                case decimal _:
                    return RubyTokenType.Float;
                case byte[] _:
                    return RubyTokenType.Bytes;
                case bool _:
                    return RubyTokenType.Boolean;
            }

            throw new ArgumentException($"Could not determine Ruby object type for type {value.GetType()}.");
        }

        /// <inheritdoc />
        public override TType GetValue<TType>()
        {
            if (Value is TType cast)
            {
                return cast;
            }
            return (TType)Convert.ChangeType(Value, typeof(TType));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}