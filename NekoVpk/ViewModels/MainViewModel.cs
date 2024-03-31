using NekoVpk.Core;
using NekoVpk.Views;
using SteamDatabase.ValvePak;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Narod.SteamGameFinder;
using ValveKeyValue;
using System.Diagnostics;

namespace NekoVpk.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string GameDir 
    {
        get => NekoSettings.Default.GameDir;
        set {
            if (NekoSettings.Default.GameDir != value)
            {
                var tmpVal = GameDir;
                NekoSettings.Default.GameDir = value;
                SetProperty(ref tmpVal, value);
                
            }
        }
    }

    public ObservableCollection<AddonAttribute> Addons { get; } = [];

    public MainViewModel() {
        if (NekoSettings.Default.GameDir == "")
        {
            NekoSettings.Default.GameDir = TryToFindGameDir() ?? NekoSettings.Default.GameDir;
        }
    }

    public static string? TryToFindGameDir()
    {
        SteamGameLocator steamGameLocator = new();
        if (steamGameLocator.getIsSteamInstalled())
        {
            SteamGameLocator.GameStruct result = steamGameLocator.getGameInfoByFolder("Left 4 Dead 2");
            if (result.steamGameLocation != null)
            {
                return Path.Join(result.steamGameLocation, "left4dead2");
            }
        }
        return null;
    }

    void Test()
    {
        KVSerializer kvs = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);

        FileInfo file = new("C://Tmp/Test.vdf");

        kvs.Deserialize(file.OpenRead());
        return;
    }

    public async void LoadAddons()
    {
        TaggedAssets.Load();
        var addonDir = new DirectoryInfo(Path.Join(GameDir, "addons"));
        var workshopDir = new DirectoryInfo(Path.Join(GameDir, "addons", "workshop"));

        if (!addonDir.Exists || !workshopDir.Exists)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Failed",
                "Game directory incorrect.",
                ButtonEnum.Ok);
            await box.ShowAsync();
            return;
        }


        var files = addonDir.GetFiles("*.vpk").ToList();
        files.AddRange(workshopDir.GetFiles("*.vpk"));

        AddonList addonList = new();
        try
        {
            addonList.Load(GameDir);
        }
        catch(Exception ex) {
            App.Logger.Error(ex);
        }

        Addons.Clear();
        foreach (FileInfo fileInfo in files)
        {
            bool? addonEnabled = null;
            AddonSource addonSource = AddonSource.Local;

            if (fileInfo.Directory!.Name == workshopDir.Name)
            {
                addonSource = AddonSource.WorkShop;
            }
            else
            {
                addonEnabled = addonList.IsEnabled(fileInfo.Name);
            }
            Package pak = new();
            pak.Read(fileInfo.FullName);

            PackageEntry? addonInfoEntry = null;

            List<AssetTag> tags = [];

            var entties = pak.Entries;
            foreach (var entity in entties)
            {
                foreach (var file in entity.Value)
                {
                    var path = file.GetFullPath();
                    if (path == "addoninfo.txt")
                        addonInfoEntry = file;
                    else
                    {
                        if (TaggedAssets.TaggedFiles.TryGetValue(path, out int v))
                        {
                            if (!tags.Contains(TaggedAssets.Tags[v]))
                            {
                                tags.Add(TaggedAssets.Tags[v]);
                            }
                            
                        }
                    }
                }
            }

            
            
            AddonInfo? addonInfo = null;
            if (addonInfoEntry != null)
            {
                try
                {
                    pak.ReadEntry(addonInfoEntry, out byte[] addonInfoContents);
                    addonInfo = AddonInfo.Load(addonInfoContents);
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex);
                }
            }
            addonInfo ??= new();
            Addons.Add(new(addonEnabled, fileInfo.Name, addonSource, addonInfo));
            Addons.Last().Tags = tags.ToArray();
        }
    }


}
