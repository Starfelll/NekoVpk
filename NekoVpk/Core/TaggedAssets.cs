using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DotNet.Globbing;
using SteamDatabase.ValvePak;
using Avalonia.Controls.Converters;

namespace NekoVpk.Core
{
    public static class TaggedAssets
    {
        public static List<AssetTagProperty> Tags { get; } = [];

        public static void Load()
        {
            if (Tags.Count > 0) return;
            Tags.Clear();

            FileInfo file = new("TaggedAssets.jsonc");
            if (!file.Exists) return;

            JObject? deserialized = JsonConvert.DeserializeObject<JObject>(file.OpenText().ReadToEnd());
            if (deserialized == null || !deserialized.HasValues) return;

            foreach (var kv in deserialized)
            {
                if (kv.Value is JObject obj)
                {
                    JToken? token = obj["files"];
                    List<Glob> globs = [];
                    if (token is JArray array)
                    {
                        foreach (var glob in array)
                        {
                            globs.Add(Glob.Parse(glob.ToString().ToLower()));
                        }
                    }

                    if (globs.Count == 0) continue;
                    Tags.Add(new(kv.Key, [.. globs]));

                    token = obj["color"];
                    if (token is JValue color)
                    {
                        Tags.Last().Color = color.ToString();
                    }

                    token = obj["alias"];
                    if (token is JArray alias)
                    {
                        string[] aliasVal = new string[alias.Count];
                        for (int i = 0; i < aliasVal.Length; ++i)
                        {
                            aliasVal[i] = alias[i].ToString();
                        }
                        Tags.Last().Alias = aliasVal;
                    }
                }

            }
            return;
        }
        
        public static AssetTag? GetAssetTag(string path, bool isHidden)
        {
            for (int i = 0; i < Tags.Count; ++i)
            {
                if (Tags[i].IsMatch(path))
                {
                    return new (i, isHidden);
                }
            }

            return null;
        } 
    }
}
