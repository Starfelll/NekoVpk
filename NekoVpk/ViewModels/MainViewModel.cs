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
using ReactiveUI;
using Avalonia.Collections;
using DynamicData;
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
                NekoSettings.Default.GameDir = value;
                this.RaisePropertyChanged(nameof(GameDir));
            }
        }
    }

    private List<AddonAttribute> _addonList = [];

    private DataGridCollectionView _Addons;

    public DataGridCollectionView Addons => _Addons;

    string? _SearchKeywords = "";

    public string? SearchKeywords { get => _SearchKeywords; set => this.RaiseAndSetIfChanged(ref _SearchKeywords, value); }

    public MainViewModel() {
        if (NekoSettings.Default.GameDir == "")
        {
            NekoSettings.Default.GameDir = TryToFindGameDir() ?? NekoSettings.Default.GameDir;
        }

        _Addons = new(_addonList) { Filter = AddonsFilter };
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

    public async void LoadAddons()
    {
        TaggedAssets.Load();
        var addonDir = new DirectoryInfo(Path.Join(GameDir, "addons"));
        var workshopDir = new DirectoryInfo(Path.Join(GameDir, "addons", "workshop"));

        if (!addonDir.Exists)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Failed",
                "Game directory incorrect.",
                ButtonEnum.Ok);
            await box.ShowAsync();
            return;
        }


        var files = addonDir.GetFiles("*.vpk").ToList();
        if (addonDir.Exists)
            files.AddRange(workshopDir.GetFiles("*.vpk"));

        AddonList addonList = new();
        try
        {
            addonList.Load(GameDir);
        }
        catch (Exception ex)
        {
            App.Logger.Error(ex);
        }

        _addonList.Clear();
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
                        if (TaggedAssets.GetAssetTag(file) is AssetTag tag)
                        {
                            if (!tags.Contains(tag))
                            {
                                tags.Add(tag);
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
            AddonAttribute newItem = new(addonEnabled, fileInfo.Name, addonSource, addonInfo);

            tags.Reverse();
            newItem.Tags = [.. tags];


            var baseName = Path.ChangeExtension(fileInfo.Name, null);
            if (newItem.IsSubscribed || baseName.All(char.IsDigit))
            {
                newItem.WorkShopID = baseName;
            }


            _addonList.Add(newItem);
        }
        Addons.Refresh();
    }

    bool AddonsFilter(object obj)
    {
        if (String.IsNullOrEmpty(SearchKeywords))
        {
            return true;
        }
        if (obj is AddonAttribute att)
        {
            var keywordList = new List<string>(SearchKeywords.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            int match = 0;
            foreach (var str in keywordList)
            {
                foreach(var tag in att.Tags)
                {
                    if (tag.Name.Contains(str, StringComparison.OrdinalIgnoreCase))
                    {
                        match++; continue;
                    } 
                }

                if (match >= keywordList.Count) return true;
                if (att.Title.Contains(str, StringComparison.OrdinalIgnoreCase))
                {
                    match++; continue;
                }

                if (match >= keywordList.Count) return true;
                if (att.Author is not null 
                    && att.Author.Contains(str, StringComparison.OrdinalIgnoreCase))
                {
                    match++; continue;
                }
            }

            if (match >= keywordList.Count) return true;

        }
        return false;
    }
}
