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
            vm.LoadAddons();
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
    }

    private void AssetTag_ToggleButton_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            if (toggle.DataContext is AssetTag tag)
            {
                toggle.Classes.Add(tag.Color);
            }
        }
    }
}
