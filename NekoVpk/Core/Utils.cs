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
        public static Encoding MakeSureCharsetIsDefault(ref byte[] bytes)
        {
            var charset = CharsetDetector.DetectFromBytes(bytes);
            if (charset.Detected.Encoding != Encoding.Default)
            {
                bytes = Encoding.Convert(charset.Detected.Encoding, Encoding.Default, bytes);
            }
            return charset.Detected.Encoding;
        }
    }

    public static class NekoExtensions {
        public static bool IsNekoDir(this string str, out string? realPath)
        {
            var strs = str.Split("/");
            if (strs.Length >= 3 && strs[0] == "nekovpk")
            {
                realPath = $"{strs[0]}/{strs[1]}/{strs[2]}/";
                return true;
            }
            realPath = null;
            return false;
        }

        public static string GenNekoDir(this Package package)
        {
            return $"nekovpk/{Guid.NewGuid()}/";
        }

        public static void ExtratFile(this Package pkg, PackageEntry entry, FileInfo outFile)
        {
            if (outFile.Exists)
                throw new Exception("File exist.");

            var outDir = outFile.Directory;
            if (outDir != null && !outDir.Exists)
                outDir.Create();

            pkg.ReadEntry(entry, out byte[] buffer);
            var writter = outFile.OpenWrite();
            writter.Write(buffer);
            writter.Close();
        }

        public static PackageEntry AddFile(this Package pkg, string filePath, FileInfo srcFile)
        {
            if (!srcFile.Exists)
                throw new FileNotFoundException();

            var reader = srcFile.OpenRead();
            var bytes = new byte[reader.Length];
            reader.Read(bytes);
            reader.Close();

            return pkg.AddFile(filePath, bytes);
        }

        public static PackageEntry AddFile(this Package pkg, string filePath, FileStream fileSteam)
        {
            var bytes = new byte[fileSteam.Length];
            fileSteam.Read(bytes);
            return pkg.AddFile(filePath, bytes);
        }

        public static MemoryStream ReadEntry(this Package pkg, PackageEntry entry)
        {
            pkg.ReadEntry(entry, out byte[] buffer);
            return new MemoryStream(buffer);
        }
    }
}
