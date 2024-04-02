using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using NekoVpk.Core;
using NekoVpk.ViewModels;
using SteamDatabase.ValvePak;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Avalonia.Controls.Primitives;

namespace NekoVpk.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void DataGrid_CurrentCellChanged(object? sender, System.EventArgs e)
    {
        CancelAssetTagChange();
        if (sender is DataGrid dg && dg.SelectedItem is AddonAttribute att && GameDir.Text is not null)
        {
            Package pak = att.LoadPackage(GameDir.Text);

            var entry = pak.FindEntry("addonimage.jpg");
            if (entry != null)
            {
                pak.ReadEntry(entry, out byte[] output);
                AddonImage.Source = Bitmap.DecodeToHeight(new System.IO.MemoryStream(output), 128);
            }
            else
            {
                FileInfo jpg = new(Path.ChangeExtension(att.GetAbsolutePath(GameDir.Text), "jpg"));
                if (jpg.Exists)
                {
                    AddonImage.Source = Bitmap.DecodeToHeight(jpg.OpenRead(), 128);
                } else
                {
                    AddonImage.Source = null;
                }
            }
            pak.Dispose();

            return;
        }
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            AddonDetailPanel.IsVisible = true;
            vm.LoadAddons();
            AddonList.SelectedIndex = 0;
            AddonList.Columns[2].ClearSort();
            AddonList.Columns[2].Sort();
        }

    }
    
    private async void Browser_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;
        var storageProvider = topLevel.StorageProvider;
        if (storageProvider is null) return;

        IStorageFolder? suggestedStartLocation = null;
        if (NekoSettings.Default.GameDir != "")
        {
            suggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(new Uri(NekoSettings.Default.GameDir));
        }
        var result = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() { 
            Title = "Select Game Directory", 
            AllowMultiple = false, 
           SuggestedStartLocation = suggestedStartLocation
        });

        
        if (result is not null && result.Count == 1) {
            var dirInfo = new DirectoryInfo(result[0].Path.LocalPath);
            var dirs = dirInfo.GetDirectories("addons");
            if (dirs.Length == 0) {
                dirs = dirInfo.GetDirectories("left4dead2");
                if (dirs.Length != 0)
                {
                    GameDir.Text = dirs[0].FullName;
                    return;
                }
            }
            GameDir.Text = dirInfo.FullName;
        }
    }

    private void DataGrid_BeginningEdit(object? sender, Avalonia.Controls.DataGridBeginningEditEventArgs e)
    {
        if (e.Row.DataContext is AddonAttribute { Source: AddonSource.WorkShop })
        {
            e.Cancel = true;
        }
    }

    private void DataGrid_CellEditEnded(object? sender, Avalonia.Controls.DataGridCellEditEndedEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit && GameDir.Text != null)
        {
            AddonList addonList = new();
            addonList.Load(GameDir.Text);
            bool modified = false;
            foreach (var v in AddonAttribute.dirty)
            {
                if (v.Enable != null)
                {
                    modified = true;
                    addonList.SetEnable(v.FileName, (bool)v.Enable);
                }
            }

            if (modified)
            {
                addonList.Save(GameDir.Text);
            }
        }
    }

    private void AddonList_Menu_LocateFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (AddonList.SelectedItem is AddonAttribute att)
        {
            FileInfo fileInfo = new(att.GetAbsolutePath(GameDir.Text));
            Process.Start(new ProcessStartInfo() {
                FileName = "explorer.exe",
                Arguments = $"/select, \"{fileInfo.FullName}\"",
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }

    private void AddonList_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        return;
        foreach (var item in AddonList.SelectedItems)
        {
            if (item is AddonAttribute att)
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = att.GetAbsolutePath(GameDir.Text),
                    UseShellExecute = true,
                    Verb = "open",
                });
            }

        }
    }

    private void AssetTag_Label_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Label label)
        {
            if (label.DataContext is AssetTag tag)
            {
                label.Classes.Add(tag.Color);
            }
        }
        //"Red", "Pink", "Purple", "Violet", "Indigo",
        //"Blue", "LightBlue", "Cyan", "Teal", "Green",
        //"LightGreen", "Lime", "Yellow", "Amber", "Orange",
        //"Grey"
    }


    private readonly Dictionary<AssetTag, bool> ModifiedAssetTags = [];

    private void AssetTag_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (sender is Label label && label.DataContext is AssetTag tag)
        {
            switch(tag.Name)
            {
                case "bill":
                case "coach":
                case "ellis":
                case "francis":
                case "louis":
                case "nick":
                case "rochelle":
                case "zoey":
                    break;
                default:
                    return;
            }
            if (!ModifiedAssetTags.ContainsKey(tag))
            {
                ModifiedAssetTags[tag] = tag.Enable;
            }
            tag.Enable = !tag.Enable;
            AssetTagModifiedPanel.IsVisible = true;
        }
    }

    private void Button_AssetTagModifiedPanel_Apply(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        if (AddonList.SelectedItem is AddonAttribute att)
        {
            Package pkg = att.LoadPackage(GameDir.Text);

            string? nekoDir = null;
            foreach (var entry in pkg.Entries)
            {
                foreach (var f in entry.Value)
                {
                    if (f.IsNekoDir(out nekoDir)) break;
                }
            }
            if (nekoDir is null || nekoDir.Length == 0)
                nekoDir = pkg.GenNekoDir();


            List<KeyValuePair<PackageEntry, string>> modified = [];
            foreach (var entry in pkg.Entries)
            {
                foreach (var f in entry.Value)
                {
                    foreach (var tag in ModifiedAssetTags.Keys)
                    {
                        string? dstPath = null;
                        if (!tag.Proporty.IsMatch(f, out dstPath, ref nekoDir)) continue;

                        if (!tag.Enable)
                        {
                            dstPath = nekoDir + dstPath;
                        }

                        Debug.Assert(dstPath.Length > 0);
                        if (pkg.FindEntry(dstPath) is null)
                        {
                            modified.Add(new(f, dstPath));
                        }
                    }
                }
            }
            

            foreach (var mod in modified)
            {
                if (pkg.FindEntry(mod.Value) is null)
                {
                    pkg.ReadEntry(mod.Key, out byte[] data);
                    if (pkg.RemoveFile(mod.Key))
                        pkg.AddFile(mod.Value, data);
                }
            }
            modified.Clear();

            FileInfo tmpFile = new FileInfo(pkg.FileName + "_nekotmp.vpk");
            FileInfo srcFile = new(pkg.FileName + ".vpk");
            if (tmpFile.Exists)
            {
                pkg.Dispose();
                CancelAssetTagChange();
                return;
            }

            pkg.Write(tmpFile.FullName, 1);
            pkg.Dispose();

            tmpFile.Refresh();
            tmpFile.MoveTo(srcFile.FullName, true);
            

            ModifiedAssetTags.Clear();
            AssetTagModifiedPanel.IsVisible = false;
        }
    }

    private void CancelAssetTagChange()
    {
        AssetTagModifiedPanel.IsVisible = false;
        foreach (var tag in ModifiedAssetTags)
        {
            tag.Key.Enable = tag.Value;
        }
        ModifiedAssetTags.Clear();
    }

    private void Button_AssetTagModifiedPanel_Cancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CancelAssetTagChange();
    }

}
