using System;
using System.IO;
using Ionic.Zlib;
using Newtonsoft.Json.Linq;
using RpgMakerEncoder.IO;

namespace RpgMakerEncoder.Encoding
{
    public class RpgMakerJsonFileProcessor : SimpleJsonFileProcessor
    {
        public override void Save(JToken token, string file)
        {
            var fileType = Path.GetFileNameWithoutExtension(file);
            if (string.Equals(fileType, "scripts", StringComparison.OrdinalIgnoreCase))
            {
                var extractPath = Path.Combine(Path.GetDirectoryName(file), fileType);
                DirectoryHelper.EnsureDirectoryExists(extractPath);
                ExtractScripts(token, extractPath);
            }

            base.Save(token, file);
        }

        public override JToken Load(string file)
        {
            var token = base.Load(file);

            var fileType = Path.GetFileNameWithoutExtension(file);
            if (string.Equals(fileType, "scripts", StringComparison.OrdinalIgnoreCase))
            {
                var sourcePath = Path.Combine(Path.GetDirectoryName(file), fileType);
                CombineScripts(token, sourcePath);
            }

            return token;
        }

        private static void ExtractScripts(JToken token, string extractPath)
        {
            foreach (var scriptToken in (JArray)token)
            {
                var scriptArray = (JArray)scriptToken;
                var fileName = Path.Combine(extractPath, DirectoryHelper.GetSafeFileName(scriptArray[1].Value<string>() + ".rb"));
                var compressed = scriptArray[2].Value<byte[]>();
                var uncompressed = Decompress(compressed);
                scriptArray.RemoveAt(2);
                File.WriteAllBytes(fileName, uncompressed);
            }
        }

        private static void CombineScripts(JToken token, string sourcePath)
        {
            foreach (var scriptToken in (JArray)token)
            {
                var scriptArray = (JArray)scriptToken;
                var fileName = Path.Combine(sourcePath, DirectoryHelper.GetSafeFileName(scriptArray[1].Value<string>() + ".rb"));
                var uncompressed = File.ReadAllBytes(fileName);
                var compressed = Compress(uncompressed);
                scriptArray.Add(compressed);
            }
        }

        private static byte[] Decompress(byte[] bytes)
        {
            return ZlibStream.UncompressBuffer(bytes);
        }

        private static byte[] Compress(byte[] bytes)
        {
            using (var resultStream = new MemoryStream())
            {
                using (var compressor = new ZlibStream(resultStream, CompressionMode.Compress, CompressionLevel.Default))
                {
                    compressor.Write(bytes, 0, bytes.Length);
                }

                return resultStream.ToArray();
            }
        }
    }
}