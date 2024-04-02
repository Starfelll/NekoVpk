using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValveKeyValue;
using UtfUnknown;
using System.Diagnostics;
using SteamDatabase.ValvePak;
using DotNet.Globbing;

namespace NekoVpk.Core
{
    public static class Utils
    {
        public static void MakeSureCharsetIsDefault(ref byte[] bytes)
        {
            var charset = CharsetDetector.DetectFromBytes(bytes);
            if (charset.Detected.Encoding != Encoding.Default)
            {
                bytes = Encoding.Convert(charset.Detected.Encoding, Encoding.Default, bytes);
            }
        }
    }

    public static class NekoExtensions {
        public static bool IsNekoDir(this PackageEntry entry, out string? realPath)
        {
            var dirs = entry.DirectoryName.Split("/");
            if (dirs.Length >= 3 && dirs[0] == "nekovpk")
            {
                realPath = $"{dirs[0]}/{dirs[1]}/{dirs[2]}/";
                return true;
            }
            realPath = null;
            return false;
        }

        public static string GenNekoDir(this Package package)
        {
            return $"nekovpk/{Guid.NewGuid()}/0/";
        }
    }
}
