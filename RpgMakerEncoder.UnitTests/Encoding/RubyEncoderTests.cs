using System.IO;
using System.Linq;
using NUnit.Framework;
using RpgMakerEncoder.Encoding;
using RpgMakerEncoder.Ruby;

namespace RpgMakerEncoder.UnitTests.Encoding
{
    [TestFixture]
    public class RubyEncoderTests
    {
        private static object[] _byteTestCases =
        {
            new object[] {new byte[] {0x0C, 0x74, 0x65, 0x73, 0x74, 0x69, 0x6E, 0x67}, new byte[] {0x74, 0x65, 0x73, 0x74, 0x69, 0x6E, 0x67}}
        };

        [TestCaseSource(nameof(_byteTestCases))]
        public void Should_read_bytes(byte[] input, byte[] expectedBytes)
        {
            using (var stream = new MemoryStream(input))
            {
                var context = new RubyEncoder.ReadContext
                {
                    Reader = new BinaryReader(stream)
                };

                var bytes = RubyEncoder.ReadBytes(context);

                Assert.That(bytes.SequenceEqual(expectedBytes));
            }
        }

        [TestCaseSource(nameof(_byteTestCases))]
        public void Should_write_bytes(byte[] expectedBytes, byte[] input)
        {
            using (var stream = new MemoryStream())
            {
                var context = new RubyEncoder.WriteContext
                {
                    Writer = new BinaryWriter(stream)
                };

                RubyEncoder.WriteBytes(input, context);

                Assert.That(stream.ToArray().SequenceEqual(expectedBytes));
            }
        }

        private static object[] _numberTestCases =
        {
            new object[] {new byte[] {0x00}, 0},
            new object[] {new byte[] {0x06}, 1},
            new object[] {new byte[] {0xFA}, -1},
            new object[] {new byte[] {0x19}, 20},
            new object[] {new byte[] {0xE7}, -20},
            new object[] {new byte[] {0x7F}, 122},
            new object[] {new byte[] {0x80}, -123},
            new object[] {new byte[] {0x01, 0x7B}, 123},
            new object[] {new byte[] {0xFF, 0x84}, -124},
            new object[] {new byte[] {0x02, 0x94, 0x11}, 4500},
            new object[] {new byte[] {0xFE, 0x6C, 0xEE}, -4500},
            new object[] {new byte[] {0x03, 0xF8, 0x24, 0x01}, 75000},
            new object[] {new byte[] {0xFD, 0x08, 0xDB, 0xFE}, -75000},
            new object[] {new byte[] {0x04, 0xC0, 0x0E, 0x16, 0x02}, 35000000},
            new object[] {new byte[] {0xFC, 0x40, 0xF1, 0xE9, 0xFD}, -35000000}
        };

        [TestCaseSource(nameof(_numberTestCases))]
        public void Should_read_number(byte[] input, long expectedNumber)
        {
            using (var stream = new MemoryStream(input))
            {
                var context = new RubyEncoder.ReadContext
                {
                    Reader = new BinaryReader(stream)
                };

                var number = RubyEncoder.ReadNumber(context);

                Assert.That(number, Is.EqualTo(expectedNumber));
            }
        }

        [TestCaseSource(nameof(_numberTestCases))]
        public void Should_write_number(byte[] expectedBytes, long number)
        {
            using (var stream = new MemoryStream())
            {
                var context = new RubyEncoder.WriteContext
                {
                    Writer = new BinaryWriter(stream)
                };

                RubyEncoder.WriteNumber(number, context);

                Assert.That(stream.ToArray().SequenceEqual(expectedBytes));
            }
        }

        private static object[] _stringTestCases =
        {
            new object[] {new byte[] {0x00}, ""},
            new object[] {new byte[] {0x0C, 0x74, 0x65, 0x73, 0x74, 0x69, 0x6E, 0x67}, "testing"}
        };

        [TestCaseSource(nameof(_stringTestCases))]
        public void Should_read_string(byte[] input, string expectedString)
        {
            using (var stream = new MemoryStream(input))
            {
                var context = new RubyEncoder.ReadContext
                {
                    Reader = new BinaryReader(stream)
                };

                var value = RubyEncoder.ReadString(context);

                Assert.That(value, Is.EqualTo(expectedString));
            }
        }

        [TestCaseSource(nameof(_stringTestCases))]
        public void Should_write_string(byte[] expectedBytes, string value)
        {
            using (var stream = new MemoryStream())
            {
                var context = new RubyEncoder.WriteContext
                {
                    Writer = new BinaryWriter(stream)
                };

                RubyEncoder.WriteString(value, context);

                Assert.That(stream.ToArray().SequenceEqual(expectedBytes));
            }
        }

        private static object[] _floatTestCases =
        {
            new object[] {new byte[] {0x08, 0x6E, 0x61, 0x6E}, 0},
            new object[] {new byte[] {0x08, 0x69, 0x6E, 0x66}, double.PositiveInfinity},
            new object[] {new byte[] {0x09, 0x2D, 0x69, 0x6E, 0x66}, double.NegativeInfinity},
            new object[] {new byte[] {0x0D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F}, 1},
            new object[] {new byte[] {0x0D, 0x33, 0x33, 0x33, 0x33, 0x33, 0x23, 0x59, 0x40}, 100.55}
        };

        [TestCaseSource(nameof(_floatTestCases))]
        public void Should_read_float(byte[] input, double expectedDouble)
        {
            using (var stream = new MemoryStream(input))
            {
                var context = new RubyEncoder.ReadContext
                {
                    Reader = new BinaryReader(stream)
                };

                var value = RubyEncoder.ReadFloat(context);

                Assert.That(value, Is.EqualTo(expectedDouble));
            }
        }

        [TestCaseSource(nameof(_floatTestCases))]
        public void Should_write_float(byte[] expectedBytes, double value)
        {
            using (var stream = new MemoryStream())
            {
                var context = new RubyEncoder.WriteContext
                {
                    Writer = new BinaryWriter(stream)
                };

                RubyEncoder.WriteFloat(value, context);

                Assert.That(stream.ToArray().SequenceEqual(expectedBytes));
            }
        }

        private static object[] _symbolTestCases =
        {
            new object[] {new byte[] {0x3A, 0x09, 0x6E, 0x61, 0x6D, 0x65}, "name"},
            new object[] {new byte[] {0x3A, 0x0F, 0x69, 0x64, 0x65, 0x6E, 0x74, 0x69, 0x66, 0x69, 0x65, 0x72}, "identifier"}
        };

        [TestCaseSource(nameof(_symbolTestCases))]
        public void Should_read_symbol(byte[] input, string expectedClassName)
        {
            using (var stream = new MemoryStream(input))
            {
                var context = new RubyEncoder.ReadContext
                {
                    Reader = new BinaryReader(stream)
                };

                var value = RubyEncoder.ReadSymbolDefinition(context);

                Assert.That(value.Name, Is.EqualTo(expectedClassName));
            }
        }

        [TestCaseSource(nameof(_symbolTestCases))]
        public void Should_write_symbol(byte[] expectedBytes, string className)
        {
            using (var stream = new MemoryStream())
            {
                var context = new RubyEncoder.WriteContext
                {
                    Writer = new BinaryWriter(stream)
                };

                RubyEncoder.WriteSymbolDefinition(className, context);

                Assert.That(stream.ToArray().SequenceEqual(expectedBytes));
            }
        }

        private static object[] _symbolLinkTestCases =
        {
            new object[] {new byte[] {0x3B, 0x00}, new[] {"name", "identifier"}, "name"},
            new object[] {new byte[] {0x3B, 0x06}, new[] {"name", "identifier"}, "identifier"}
        };

        [TestCaseSource(nameof(_symbolLinkTestCases))]
        public void Should_read_symbol_link(byte[] input, string[] classNames, string expectedClassName)
        {
            using (var stream = new MemoryStream(input))
            {
                var context = new RubyEncoder.ReadContext
                {
                    Reader = new BinaryReader(stream)
                };
                context.Symbols.AddRange(classNames.Select(name => new RubyClass {Name = name}));

                var value = RubyEncoder.ReadSymbolDefinition(context);

                Assert.That(value.Name, Is.EqualTo(expectedClassName));
            }
        }

        [TestCaseSource(nameof(_symbolLinkTestCases))]
        public void Should_write_symbol_link(byte[] expectedBytes, string[] classNames, string className)
        {
            using (var stream = new MemoryStream())
            {
                var context = new RubyEncoder.WriteContext
                {
                    Writer = new BinaryWriter(stream)
                };
                context.Symbols.AddRange(classNames.Select(name => new RubyClass {Name = name}));

                RubyEncoder.WriteSymbolDefinition(className, context);

                Assert.That(stream.ToArray().SequenceEqual(expectedBytes));
            }
        }
    }
}