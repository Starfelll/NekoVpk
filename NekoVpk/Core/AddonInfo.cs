using Avalonia.Controls.Chrome;
using Gameloop.Vdf.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValveKeyValue;

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
            Utils.MakeSureCharsetIsDefault(ref data);
            var kvs = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            return kvs.Deserialize<AddonInfo>(new MemoryStream(data));
        }
    }
}
