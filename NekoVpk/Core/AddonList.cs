using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UtfUnknown;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using ValveKeyValue;
using System.Globalization;

namespace NekoVpk.Core
{
    public class AddonList
    {
        protected Dictionary<string, int> KeyValue = [];

        protected Encoding SrcEncoding = Encoding.Default;

        public void Load(string gameDir)
        {
            FileInfo file = GetFileInfo(gameDir);
            if (file.Exists)
            {
                byte[] buffer = new byte[file.Length];
                var stream = file.OpenRead();
                stream.Read(buffer);
                stream.Dispose();

                var kvs = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);

                SrcEncoding = Utils.MakeSureCharsetIsDefault(ref buffer);
                var deserialized = kvs.Deserialize(new MemoryStream(buffer));

                if (deserialized != null && deserialized.Name.Equals("AddonList", StringComparison.OrdinalIgnoreCase))
                {
                    KeyValue.Clear();
                    foreach (var v in deserialized)
                    {
                        if (v.Value != null)
                        {
                            KeyValue.Add(v.Name, v.Value.ToInt32(CultureInfo.CurrentCulture));
                        }
                    }
                }
                
            }
        }

        public void SetEnable(string fileName, bool enable = true)
        {
            KeyValue[fileName] = enable ? 1 : 0;
        }

        public bool? IsEnabled(string fileName)
        {
            if (KeyValue.TryGetValue(fileName, out int value))
            {
                return value == 1;
            }
            return null;
        }

        public void Save(string gameDir)
        {
            var file = GetFileInfo(gameDir);
            StreamWriter writer = new(file.FullName, false, SrcEncoding);

            writer.WriteLine("\"AddonList\"\n{");
            foreach(var v in KeyValue)
            {
                writer.WriteLine($"\t\"{v.Key}\"\t\"{v.Value}\"");
            }
            writer.WriteLine("}");
            writer.Close();
        }

        protected static FileInfo GetFileInfo(string gameDir) => new(Path.Join(gameDir, "addonlist.txt"));

    }
}
