using System;
using System.IO;

namespace RpgMakerEncoder.IO
{
    public static class DirectoryHelper
    {
        public static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static string MakeRelative(string rootPath, string path)
        {
            var rootUri = CreateUri(rootPath);
            var uri = CreateUri(path);
            var relative = rootUri.MakeRelativeUri(uri).OriginalString;
            if (Path.DirectorySeparatorChar != '/')
            {
                relative = relative.Replace('/', Path.DirectorySeparatorChar);
            }
            relative = Uri.UnescapeDataString(relative);

            return relative.TrimEnd(Path.DirectorySeparatorChar);
        }

        public static Uri CreateUri(string url)
        {
            if (!url.EndsWith("/") && !url.EndsWith("\\"))
            {
                url = url + "/";
            }
            return new Uri(url);
        }

        public static string GetSafeFileName(string path)
        {
            foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
            {
                path = path.Replace(invalidFileNameChar, '_');
            }
            return path;
        }
    }
}