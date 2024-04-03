using DotNet.Globbing;
using NekoVpk.ViewModels;
using ReactiveUI;
using SteamDatabase.ValvePak;

namespace NekoVpk.Core
{
    public class AssetTagProperty
    {
        public string Name { get; set; }

        public string Color { get; set; }

        public Glob[] Globs { get; set; }

        public AssetTagProperty(string name, Glob[] globs, string color = "") { 
            Name = name; Color = color; Globs = globs;
        }

        public AssetTagProperty(AssetTagProperty obj)
        {
            Name = obj.Name; Color = obj.Color; Globs = obj.Globs;
        }

        public override bool Equals(object? obj)
        {
            if (obj is AssetTagProperty tag) {
                return Name == tag.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public bool IsMatch(PackageEntry entry, out string? originPath, ref string? nekoDir)
        {

            foreach (var glob in Globs)
            {
                var path = entry.GetFullPath();
                if (nekoDir is null)
                {
                    entry.IsNekoDir(out nekoDir);
                }

                if (nekoDir != null && nekoDir != "")
                    path = path.Replace(nekoDir, "");
                if (glob.IsMatch(path))
                {
                    originPath = path;
                    return true;
                }

            }
            originPath = null;
            return false;
        }

        public bool IsMatch(PackageEntry entry, out string? originPath)
        {
            string? nekoDir = null;
            return IsMatch(entry, out originPath, ref nekoDir);
        }

    }

    public class AssetTag: ViewModelBase
    {
        public readonly int Index;

        public AssetTagProperty Proporty { get => TaggedAssets.Tags[Index]; }

        public string Name { get => TaggedAssets.Tags[Index].Name; }

        public string Color { get => TaggedAssets.Tags[Index].Color; }

        public Glob[] Globs { get => TaggedAssets.Tags[Index].Globs; }

        bool _Enable;

        public bool Enable
        {
            get => _Enable; set => this.RaiseAndSetIfChanged(ref _Enable, value);
        }

        public AssetTag(int index, bool enable = true)
        {
            _Enable = enable;
            Index = index;
        }

        public override bool Equals(object? obj)
        {
            if (obj is AssetTag tag && tag.Index == Index)
            {
                return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Index;
        }
    

    }
}
