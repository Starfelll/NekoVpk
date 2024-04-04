using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UtfUnknown;
using ValveKeyValue;
using System.Globalization;


namespace NekoVpk.Core
{
    public class AddonList
    {
        KVDocument KeyValue;

        readonly KVSerializerOptions SerializerOptions;
        KVCollectionValue Collection { get => (KVCollectionValue)KeyValue.Value; }

        public AddonList()
        {
            SerializerOptions = new KVSerializerOptions
            {
                HasEscapeSequences = false,
            };
            KeyValue = new("AddonList", new KVCollectionValue());
        }

        public void Load(string gameDir)
        {
            FileInfo file = GetFileInfo(gameDir);
            if (file.Exists)
            {
                FileStream stream = file.OpenRead();

                SerializerOptions.Encoding = CharsetDetector.DetectFromStream(stream).Detected.Encoding;
                stream.Position = 0;

                var kvs = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);

                var deserialized = kvs.Deserialize(stream, SerializerOptions);

                if (deserialized != null 
                    && deserialized.Name.Equals("AddonList", StringComparison.OrdinalIgnoreCase) 
                    && deserialized.Value is KVCollectionValue)
                {
                    KeyValue = deserialized;
                }

            }
        }

        public void SetEnable(string fileName, bool enable = true)
        {
            Collection.Set(fileName, enable ? 1:0);
        }

        public bool? IsEnabled(string fileName)
        {
            KVValue? val = Collection[fileName];
            if (val != null)
            {
                return val.ToInt32(CultureInfo.CurrentCulture) == 1;
            }
            return null;
        }

        public void Save(string gameDir)
        {
            var file = GetFileInfo(gameDir);
            var kvs = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            var writeStream = file.Open(FileMode.Open);

            kvs.Serialize(writeStream, KeyValue, SerializerOptions);
            writeStream.Close();
        }

        protected static FileInfo GetFileInfo(string gameDir) => new(Path.Join(gameDir, "addonlist.txt"));

    }
}
