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
using SevenZip;
using System.Diagnostics;
using DotNet.Globbing;

namespace NekoVpk.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        if (NekoSettings.Default.GameDir == "")
        {
            NekoSettings.Default.GameDir = TryToFindGameDir() ?? NekoSettings.Default.GameDir;
        }

        _Addons = new(_addonList) { Filter = AddonsFilter };
    }

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
        if (workshopDir.Exists)
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
            try
            {
                pak.Read(fileInfo.FullName);
            } catch(Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Failed to read vpk",
                    $"Could not open: \n{fileInfo.FullName}\n\n{ex.Message}", 
                    ButtonEnum.Ok);
                await box.ShowAsync();
                continue;
            }
            

            PackageEntry? addonInfoEntry = null;

            List<AssetTag> tags = [];

            Func<string, bool, bool> checkPath = (p, isHidden) => {
                if (TaggedAssets.GetAssetTag(p, isHidden) is AssetTag tag)
                {
                    if (!tags.Contains(tag))
                        tags.Add(tag);
                }
                return false;
            };

            if (pak.Version != 1 && pak.Version != 2)
            {
                string glob = $"VPK-Version-{pak.Version}";
                AssetTag? versionTag = TaggedAssets.GetAssetTag(glob, false);
                if (versionTag is null)
                {
                    TaggedAssets.Tags.Add(new AssetTagProperty("0x" + Convert.ToString(pak.Version, 16).ToUpper(), [Glob.Parse(glob)]));
                    versionTag = new AssetTag(TaggedAssets.Tags.Count - 1, false);
                }
                tags.Add(versionTag);
            }

            var entties = pak.Entries;
            foreach (var entity in entties)
            {
                foreach (var file in entity.Value)
                {
                    var path = file.GetFullPath();
                    if (path == "addoninfo.txt")
                        addonInfoEntry = file;
                    else if (file.TypeName == "neko7z" && file.FileName == "0")
                    {
                        pak.ReadEntry(file, out byte[] neko7zBytes);
                        SevenZipExtractor extractor = new(new MemoryStream(neko7zBytes));
                        var archiveFileNames = extractor.ArchiveFileNames;
                        foreach (var zipFile in archiveFileNames)
                        {
                            checkPath(zipFile, false);
                        }
                    }
                    else
                    {
                        checkPath(path, true);
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
            

            string types = string.Empty;
            foreach (var t in tags)
            {
                if (t.Type is null) { continue; }
                foreach (var t2 in t.Type)
                {
                    if (!types.Contains(t2))
                    {
                        if (types.Length > 0)
                            types += $", {t2}";
                        else
                            types = t2;
                    }
                }
            }

            AddonAttribute newItem = new(addonEnabled, fileInfo.Name, addonSource, addonInfo, types);
            newItem.Tags = [.. tags.OrderBy(x => x.Name)];


            var baseName = Path.ChangeExtension(fileInfo.Name, null);
            if (newItem.IsSubscribed || baseName.All(char.IsDigit))
            {
                newItem.WorkShopID = baseName;
            }

            newItem.ModificationTime = fileInfo.LastWriteTime;
            newItem.CreationTime = fileInfo.CreationTime;
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

                if (match >= keywordList.Count) return true;
                if (att.FileName.Contains(str, StringComparison.OrdinalIgnoreCase))
                {
                    match++; continue;
                }

                if (match >= keywordList.Count) return true;
                if (att.Type.Contains(str, StringComparison.OrdinalIgnoreCase))
                {
                    match++; continue;
                }
            }

            if (match >= keywordList.Count) return true;

        }
        return false;
    }
}
