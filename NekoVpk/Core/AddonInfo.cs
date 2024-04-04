using Avalonia.Controls.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValveKeyValue;
using UtfUnknown;

namespace NekoVpk.Core
{
    public class AddonInfo
    {
        [KVProperty("AddonAuthor")]
        public string? Author { get; set; }

        [KVProperty("AddonTitle")]
        public string? Title { get; set; }

        [KVProperty("AddonVersion")]
        public string? Version { get; set; }

        [KVProperty("AddonDescription")]
        public string? Description { get; set; }

        [KVProperty("addonURL0")]
        public string? Url0 { get; set; }

        public static AddonInfo Load(byte[] data)
        {
            var charset = CharsetDetector.DetectFromBytes(data);
            KVSerializerOptions options = new()
            {
                HasEscapeSequences = true,
                Encoding = charset.Detected.Encoding
            };

            var kvs = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);

            return kvs.Deserialize<AddonInfo>(new MemoryStream(data), options);
        }
    }
}
