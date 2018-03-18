using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RpgMakerEncoder.RpgMaker.UserDefined;
using RpgMakerEncoder.Ruby;

namespace RpgMakerEncoder.RpgMaker
{
    public static class RpgMakerHelper
    {
        public static string LocateGamePath()
        {
            var directory = Environment.CurrentDirectory;
            var gamePath = Directory.EnumerateFiles(directory, "Game.exe", SearchOption.AllDirectories).FirstOrDefault();
            if (gamePath == null)
            {
                directory = Path.GetDirectoryName(directory);
                gamePath = Directory.EnumerateFiles(directory, "Game.exe", SearchOption.AllDirectories).FirstOrDefault();
            }

            return Path.GetDirectoryName(gamePath);
        }

        public static Dictionary<string, Func<RubyObject, byte[]>> UserDefinitionEncoders()
        {
            return new Dictionary<string, Func<RubyObject, byte[]>>
            {
                {"Table", Table.Encode},
                {"Color", Color.Encode},
                {"Tone", Tone.Encode}
            };
        }

        public static Dictionary<string, Func<BinaryReader, RubyObject>> UserDefinitionDecoders()
        {
            return new Dictionary<string, Func<BinaryReader, RubyObject>>
            {
                {"Table", Table.Decode},
                {"Color", Color.Decode},
                {"Tone", Tone.Decode}
            };
        }
    }
}