using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RpgMakerEncoder.Ruby;

namespace RpgMakerEncoder.Encoding
{
    public class RubyEncoder : IRubyEncoder
    {
        public void Encode(RubyToken token, string file)
        {
            Encode(token, file, null);
        }

        public void Encode(RubyToken token, string file, RubyEncoderOptions options)
        {
            using (var stream = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                Encode(token, stream, options);
            }
        }

        public void Encode(RubyToken token, Stream stream)
        {
            Encode(token, stream, null);
        }

        public void Encode(RubyToken token, Stream stream, RubyEncoderOptions options)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((short)0x0804);
                WriteEntry(token,
                           new WriteContext
                           {
                               Writer = writer
                           },
                           options ?? new RubyEncoderOptions());
            }
        }

        internal static void WriteEntry(RubyToken token, WriteContext context, RubyEncoderOptions options)
        {
            switch (token.Type)
            {
                case RubyTokenType.Array:
                    var rubyArray = (RubyArray)token;
                    context.Data.Add(rubyArray);

                    context.Writer.Write((byte)RubyType.Array);
                    WriteNumber(rubyArray.Count, context);
                    foreach (var item in rubyArray)
                    {
                        WriteEntry(item, context, options);
                    }

                    break;
                case RubyTokenType.Object:
                    var rubyObject = (RubyObject)token;
                    context.Data.Add(rubyObject);
                    if (rubyObject.RubyClass.Name == "encoder:Hash")
                    {
                        context.Writer.Write((byte)RubyType.Hash);
                        WriteNumber(rubyObject.Properties.Count, context);

                        foreach (var property in rubyObject.Properties)
                        {
                            var rubyKey = long.TryParse(property.Key, out var numberKey) ? new RubyValue(numberKey) : new RubyValue(property.Key);
                            WriteEntry(rubyKey, context, options);
                            WriteEntry(property.Value, context, options);
                        }
                    }
                    else if (options.UserEncoders.TryGetValue(rubyObject.RubyClass.Name, out var encoder))
                    {
                        context.Writer.Write((byte)RubyType.UserDefined);
                        WriteSymbolDefinition(rubyObject.RubyClass.Name, context);

                        var bytes = encoder(rubyObject);
                        WriteNumber(bytes.Length, context);
                        context.Writer.Write(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        context.Writer.Write((byte)RubyType.Object);
                        WriteSymbolDefinition(rubyObject.RubyClass.Name, context);
                        WriteNumber(rubyObject.Properties.Count, context);

                        foreach (var property in rubyObject.Properties)
                        {
                            WriteSymbolDefinition("@" + property.Key, context);
                            WriteEntry(property.Value, context, options);
                        }
                    }

                    break;
                case RubyTokenType.Boolean:
                    context.Writer.Write((byte)(token.GetValue<bool>() ? RubyType.True : RubyType.False));
                    break;
                case RubyTokenType.String:
                    context.Writer.Write((byte)RubyType.String);
                    WriteString(token.GetValue<string>(), context);
                    break;
                case RubyTokenType.Bytes:
                    context.Writer.Write((byte)RubyType.String);
                    WriteBytes(token.GetValue<byte[]>(), context);
                    break;
                case RubyTokenType.Number:
                    context.Writer.Write((byte)RubyType.Number);
                    WriteNumber(token.GetValue<long>(), context);
                    break;
                case RubyTokenType.Float:
                    context.Writer.Write((byte)RubyType.Decimal);
                    context.Data.Add(token);
                    WriteFloat(token.GetValue<double>(), context);
                    break;
                case RubyTokenType.Null:
                    context.Writer.Write((byte)RubyType.Nil);
                    break;
                default:
                    throw new InvalidOperationException($"Cannot write type of {token.Type}.");
            }
        }

        public RubyToken Decode(string file)
        {
            return Decode(file, null);
        }

        public RubyToken Decode(string file, RubyDecoderOptions options)
        {
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return Decode(stream, options);
            }
        }

        public RubyToken Decode(Stream stream)
        {
            return Decode(stream, null);
        }

        public RubyToken Decode(Stream stream, RubyDecoderOptions options)
        {
            using (var reader = new BinaryReader(stream))
            {
                if (reader.ReadInt16() == 0x0804)
                {
                    return ReadEntry(new ReadContext
                                     {
                                         Reader = reader
                                     },
                                     options ?? new RubyDecoderOptions());
                }
            }

            return null;
        }

        internal static RubyToken ReadEntry(ReadContext context, RubyDecoderOptions options)
        {
            RubyClass rubyClass;
            int fieldCount;
            RubyObject objectEntry;

            var type = (RubyType)context.Reader.ReadByte();
            switch (type)
            {
                case RubyType.Nil:
                    return new RubyValue((object)null);
                case RubyType.TypeLink:
                    var dataId = (int)ReadNumber(context);
                    if (context.Data.Count > dataId)
                    {
                        return context.Data[dataId];
                    }
                    else
                    {
                        throw new Exception("Failed to locate linked data");
                    }
                case RubyType.Array:
                    var length = (int)ReadNumber(context);
                    var arrayEntry = new RubyArray
                    {
                        Array = new List<RubyToken>(length)
                    };
                    context.Data.Add(arrayEntry);

                    for (var index = 0; index < length; index++)
                    {
                        arrayEntry.Array.Add(ReadEntry(context, options));
                    }

                    return arrayEntry;
                case RubyType.Object:
                    rubyClass = ReadSymbolDefinition(context);
                    fieldCount = (int)ReadNumber(context);
                    objectEntry = new RubyObject
                    {
                        RubyClass = rubyClass,
                        Properties = new Dictionary<string, RubyToken>(fieldCount)
                    };
                    context.Data.Add(objectEntry);

                    for (var fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++)
                    {
                        var keySymbol = ReadSymbolDefinition(context);
                        var value = ReadEntry(context, options);
                        objectEntry.Properties.Add(keySymbol.Name.TrimStart('@'), value);
                    }

                    return objectEntry;
                case RubyType.UserDefined:
                    rubyClass = ReadSymbolDefinition(context);
                    if (options.UserDecoders != null && options.UserDecoders.TryGetValue(rubyClass.Name, out var decoder))
                    {
                        ReadNumber(context);
                        var userDefinedEntry = decoder(context.Reader);
                        userDefinedEntry.RubyClass = rubyClass;
                        context.Data.Add(userDefinedEntry);
                        return userDefinedEntry;
                    }
                    else
                    {
                        throw new Exception($"User defined type not handled: {rubyClass.Name}");
                    }
                case RubyType.Hash:
                    fieldCount = (int)ReadNumber(context);
                    objectEntry = new RubyObject
                    {
                        RubyClass = new RubyClass {Name = "encoder:Hash"},
                        Properties = new Dictionary<string, RubyToken>(fieldCount)
                    };
                    context.Data.Add(objectEntry);

                    for (var fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++)
                    {
                        var key = ReadEntry(context, options);
                        var value = ReadEntry(context, options);
                        objectEntry.Properties.Add(key.ToString(), value);
                    }

                    return objectEntry;
                case RubyType.Number:
                    return new RubyValue(ReadNumber(context));
                case RubyType.Decimal:
                    var rubyFloat = new RubyValue(ReadFloat(context));
                    context.Data.Add(rubyFloat);
                    return rubyFloat;
                case RubyType.String:
                    var stringEntry = ReadStringValue(context);
                    context.Data.Add(stringEntry);
                    return stringEntry;
                case RubyType.True:
                    return new RubyValue(true);
                case RubyType.False:
                    return new RubyValue(false);
                default:
                    throw new Exception($"Unknown type: {type}");
            }
        }

        internal static void WriteSymbolDefinition(string name, WriteContext context)
        {
            var id = context.Symbols.FindIndex(rc => rc.Name == name);
            if (id == -1)
            {
                context.Symbols.Add(new RubyClass
                {
                    Name = name
                });

                context.Writer.Write((byte)RubyType.Symbol);
                WriteString(name, context);
            }
            else
            {
                context.Writer.Write((byte)RubyType.SymbolLink);
                WriteNumber(id, context);
            }
        }

        internal static RubyClass ReadSymbolDefinition(ReadContext context)
        {
            var type = (RubyType)context.Reader.ReadByte();
            switch (type)
            {
                case RubyType.Symbol:
                    return ReadSymbol(context);
                case RubyType.SymbolLink:
                    return ReadLink(context);
                default:
                    throw new Exception($"Unknown type: {type}");
            }
        }

        internal static void WriteFloat(double value, WriteContext context)
        {
            switch (value)
            {
                case 0:
                    WriteBytes(DecimalTexts.NotANumber, context);
                    break;
                case double.PositiveInfinity:
                    WriteBytes(DecimalTexts.Infinity, context);
                    break;
                case double.NegativeInfinity:
                    WriteBytes(DecimalTexts.NegativeInfinity, context);
                    break;
                default:
                    WriteBytes(BitConverter.GetBytes(value), context);
                    break;
            }
        }

        internal static double ReadFloat(ReadContext context)
        {
            var bytes = ReadBytes(context);
            if (bytes.SequenceEqual(DecimalTexts.NotANumber))
            {
                return 0;
            }
            if (bytes.SequenceEqual(DecimalTexts.Infinity))
            {
                return double.PositiveInfinity;
            }
            if (bytes.SequenceEqual(DecimalTexts.NegativeInfinity))
            {
                return double.NegativeInfinity;
            }
            return BitConverter.ToDouble(bytes, 0);
        }

        internal static RubyClass ReadSymbol(ReadContext context)
        {
            var rubyClass = new RubyClass
            {
                Name = ReadString(context)
            };

            context.Symbols.Add(rubyClass);

            return rubyClass;
        }

        internal static RubyClass ReadLink(ReadContext context)
        {
            var id = (int)ReadNumber(context);
            return context.Symbols[id];
        }

        internal static byte[] ReadBytes(ReadContext context)
        {
            var length = (int)ReadNumber(context);
            return ReadBytes(context, length);
        }

        internal static void WriteBytes(byte[] bytes, WriteContext context)
        {
            WriteNumber(bytes.Length, context);
            context.Writer.Write(bytes);
        }

        internal static byte[] ReadBytes(ReadContext context, int length)
        {
            if (length != 0)
            {
                return context.Reader.ReadBytes(length);
            }
            return null;
        }

        internal static void WriteNumber(long value, WriteContext context)
        {
            if (value == 0)
            {
                context.Writer.Write((byte)0);
            }
            else if (0 < value && value < 123)
            {
                context.Writer.Write((sbyte)(value + 5));
            }
            else if (-124 < value && value < 0)
            {
                context.Writer.Write((sbyte)(value - 5));
            }
            else
            {
                var bytes = new byte[5];
                byte i;
                for (i = 1; i < 5; i++)
                {
                    bytes[i] = (byte)(value & 0xff);
                    value = value >> 8;
                    if (value == 0)
                    {
                        bytes[0] = i;
                        break;
                    }
                    if (value == -1)
                    {
                        bytes[0] = (byte)-i;
                        break;
                    }
                }
                context.Writer.Write(bytes, 0, i + 1);
            }
        }

        internal static long ReadNumber(ReadContext context)
        {
            long value = 0;
            int length = (sbyte)context.Reader.ReadByte();

            if (length > 0)
            {
                if (length > 4)
                {
                    value = length - 5;
                }
                else
                {
                    value = 0;
                    for (var index = 0; index < length; index++)
                    {
                        value |= (long)context.Reader.ReadByte() << (8 * index);
                    }
                }
            }
            else if (length != 0)
            {
                if (length < -4)
                {
                    value = length + 5;
                }
                else
                {
                    length = -length;
                    value = -1;
                    for (var index = 0; index < length; index++)
                    {
                        value &= ~((long)0xff << (8 * index));
                        value |= (long)context.Reader.ReadByte() << (8 * index);
                    }
                }
            }

            return value;
        }

        internal static void WriteString(string value, WriteContext context)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            WriteBytes(bytes, context);
        }

        internal static RubyValue ReadStringValue(ReadContext context)
        {
            var bytes = ReadBytes(context);
            if (bytes != null)
            {
                if (bytes.Length > 1 && bytes[0] == 0x78 && (bytes[1] == 0x9c || bytes[1] == 0xda))
                {
                    return new RubyValue(bytes);
                }
                return new RubyValue(System.Text.Encoding.UTF8.GetString(bytes));
            }
            return new RubyValue("");
        }

        internal static string ReadString(ReadContext context)
        {
            var bytes = ReadBytes(context);
            if (bytes != null)
            {
                return System.Text.Encoding.ASCII.GetString(bytes);
            }
            return "";
        }

        internal class MarhalContext
        {
            public List<RubyToken> Data { get; } = new List<RubyToken>();
            public List<RubyClass> Symbols { get; } = new List<RubyClass>();
        }

        internal class ReadContext : MarhalContext
        {
            public BinaryReader Reader { get; set; }
        }

        internal class WriteContext : MarhalContext
        {
            public BinaryWriter Writer { get; set; }
        }

        internal static class DecimalTexts
        {
            public static readonly byte[] NotANumber = {0x6e, 0x61, 0x6e};
            public static readonly byte[] Infinity = {0x69, 0x6E, 0x66};
            public static readonly byte[] NegativeInfinity = {0x2d, 0x69, 0x6E, 0x66};
        }

        internal enum RubyType : byte
        {
            String = 0x22,
            Nil = 0x30,
            Symbol = 0x3a,
            SymbolLink = 0x3b,
            TypeLink = 0x40,
            False = 0x46,
            True = 0x54,
            Array = 0x5b,
            Decimal = 0x66,
            Number = 0x69,
            Object = 0x6f,
            UserDefined = 0x75,
            Hash = 0x7b
        }
    }
}