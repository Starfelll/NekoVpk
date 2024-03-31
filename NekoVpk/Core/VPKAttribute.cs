using Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamDatabase.ValvePak;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.InteropServices;

namespace NekoVpk.Core
{
    public enum AddonSource
    {
        Unknown,
        WorkShop,
        Local
    }

    public class TaggedAsset
    {
        public static Dictionary<int, int> TaggedFiles { get; } = [];
        public static List<TaggedAsset> Tags { get; } = [];

        public string Name { get; }

        public string[] Files { get; }

        public TaggedAsset(string name, string[] files)
        {
            Name = name;
            Files = files;
        }

        public static void Tag(string name, params string[] files)
        {
            foreach (var file in files)
            {
                TaggedFiles.Add(file.GetHashCode(), Tags.Count);
            }
            Tags.Add(new TaggedAsset(name, files));
        }
    }

    public class AddonAttribute : ObservableObject
    {

        public static List<AddonAttribute> dirty = [];

        protected AddonInfo AddonInfo;

        protected bool? _Enabled;

        public bool? Enable { 
            get => _Enabled;
            set {
                if (_Enabled != value)
                {
                    _Enabled = value;
                    SetProperty(ref _Enabled, value);
                    dirty.Add(this);
                }
            }
        }
        public string FileName { get; }
        public AddonSource Source { get; }

        public string Title { get => AddonInfo.Title ?? FileName; }

        public string? Version { get => AddonInfo.Version; }
        public string? Author { get => AddonInfo.Author; }
        public string? Description { get => AddonInfo.Description; }

        public string TagsOrde
        {
            get
            {
                string result = "";

                foreach(var tag in Tags)
                {
                    result += tag.Name;
                }
                return result;
            }
        }

        public AssetTag[] Tags { get; set; } = [];

        public AddonAttribute(bool? enable, string fileName, AddonSource source, AddonInfo addonInfo) {
            _Enabled = enable;
            FileName = fileName;
            Source = source;
            AddonInfo = addonInfo;
        }

        public string GetAbsolutePath(string gameDir)
        {
            string path = Path.Join(gameDir, "addons");

            if (Source == AddonSource.WorkShop) {
                path = Path.Join(path, "workshop");
            }

            path = Path.Join(path, FileName);
            return path;
        }

        public Package LoadPackage(string gameDir)
        {
            Package package = new();
            package.Read(GetAbsolutePath(gameDir));
            return package;
        }

        public void ScanContent(Package pak)
        {

        }
    }
}
