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

        public string[]? Alias { get; set; }

        public AssetTagProperty(string name, Glob[] globs, string color = "", string[]? alias = null) {
            Name = name; Color = color; Globs = globs; Alias = alias;
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

        public bool IsMatch(string path)
        {

            foreach (var glob in Globs)
            {
                if (glob.IsMatch(path))
                    return true;
            }
            return false;
        }

    }

    public class AssetTag: ViewModelBase
    {
        public readonly int Index;

        public AssetTagProperty Proporty { get => TaggedAssets.Tags[Index]; }

        public string Name { get => TaggedAssets.Tags[Index].Name; }

        public string Color { get => TaggedAssets.Tags[Index].Color; }

        public Glob[] Globs { get => TaggedAssets.Tags[Index].Globs; }

        public string[]? Alias { get => TaggedAssets.Tags[Index]?.Alias; }

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
