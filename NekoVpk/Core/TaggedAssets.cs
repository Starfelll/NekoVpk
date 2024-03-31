using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NekoVpk.Core
{
    public static class TaggedAssets
    {
        public static Dictionary<string, int> TaggedFiles { get; } = [];
        public static List<AssetTag> Tags { get; } = [];

        public static void Load()
        {
            if (TaggedFiles.Count > 0 && Tags.Count > 0) return;
            TaggedFiles.Clear();
            Tags.Clear();

            FileInfo file = new("TaggedAssets.json");
            if (!file.Exists) return;

            JObject? deserialized = JsonConvert.DeserializeObject<JObject>(file.OpenText().ReadToEnd());
            if (deserialized == null || !deserialized.HasValues) return;

            foreach (var kv in deserialized)
            {
                if (kv.Value is JObject obj)
                {
                    JToken? token = obj["files"];
                    if (token is JArray array)
                    {
                        var files = new string[array.Count];
                        int i = 0;
                        for (; i < files.Length; ++i)
                        {
                            files[i] = array[i].ToString();
                            if (TaggedFiles.ContainsKey(files[i]))
                                break;
                        }
                        if (i == files.Length)
                        {
                            foreach (var v in files)
                            {
                                TaggedFiles.Add(v, Tags.Count);
                            }
                            Tags.Add(new(kv.Key));
                        }
                        else
                        {
                            break;
                        }
                    }

                    token = obj["color"];
                    if (token is JValue color)
                    {
                        Tags.Last().Color = color.ToString();
                    }
                }

            }
            return;
        }
        
    }
}
