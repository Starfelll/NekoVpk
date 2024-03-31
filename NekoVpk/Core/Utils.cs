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
}
