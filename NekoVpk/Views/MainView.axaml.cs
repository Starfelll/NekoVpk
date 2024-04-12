using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using NekoVpk.Core;
using NekoVpk.ViewModels;
using SteamDatabase.ValvePak;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Avalonia.Threading;
using SevenZip;

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

            Package? pak = null;
            try
            {
                pak = att.LoadPackage(GameDir.Text);
            }
            catch (FileNotFoundException ex)
            {
                AddonImage.Source = null;
                return;
            }

            if (pak == null) return;

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
                    var fileStream = jpg.OpenRead();
                    AddonImage.Source = Bitmap.DecodeToHeight(fileStream, 128);
                    fileStream.Close();
                }
                else
                {
                    AddonImage.Source = null;
                }
            }
            pak.Dispose();
            
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
                case "bill_deathpose":
                case "francis_light":
                case "zoey_light":
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

        if (AddonList.SelectedItem is AddonAttribute att && DataContext is MainViewModel vm)
        {
            Package? pkg = null;
            try
            {
                pkg = att.LoadPackage(vm.GameDir);
                DirectoryInfo tmpDir = new (pkg.FileName + "_nekotmp");

                if (tmpDir.Exists)
                    tmpDir.Delete(true);
                tmpDir.Create();
                tmpDir.Attributes |= FileAttributes.Hidden;

                SevenZipExtractor? extractor = null;
                MemoryStream? zipStream = null;

                // find neko7z
                foreach (var entry in pkg.Entries)
                {
                    if (entry.Key == "neko7z" && entry.Value[0].FileName == "0")
                    {
                        zipStream = pkg.ReadEntry(entry.Value[0]);
                        extractor = new SevenZipExtractor(zipStream);
                        pkg.RemoveFile(entry.Value[0]);
                        break;
                    }
                }

                List<string> vpkFiles = [];
                Dictionary<string, string> neko7zFiles = [];


                if (extractor is not null)
                {
                    foreach (var zipFile in extractor.ArchiveFileData)
                    {
                        if (zipFile.IsDirectory) continue;
                        bool isEnable = false;
                        foreach (var tag in ModifiedAssetTags.Keys)
                        {
                            if (tag.Enable && tag.Proporty.IsMatch(zipFile.FileName))
                            {
                                isEnable = true;
                                break;
                            }
                        }
                        if (isEnable)
                            vpkFiles.Add(zipFile.FileName);
                        else
                            neko7zFiles.Add(zipFile.FileName, Path.Join(tmpDir.FullName, zipFile.FileName));
                    }
                }
                List<PackageEntry> disableEntries = [];
                foreach (var entry in pkg.Entries)
                {
                    foreach (var f in entry.Value)
                    {
                        foreach (var tag in ModifiedAssetTags.Keys)
                        {
                            if (!tag.Enable && tag.Proporty.IsMatch(f.GetFullPath()))
                            {
                                disableEntries.Add(f);
                                break;
                            }
                        }
                    }
                }



                extractor?.ExtractArchive(tmpDir.FullName);
                foreach (var entry in disableEntries)
                {
                    FileInfo outFile = new (Path.Join(tmpDir.FullName, entry.GetFullPath()));
                    entry.ExtratFile(outFile, pkg);
                    neko7zFiles.Add(entry.GetFullPath(), outFile.FullName);
                    pkg.RemoveFile(entry);
                }
                foreach (var v in vpkFiles)
                {
                    FileInfo file = new FileInfo(Path.Join(tmpDir.FullName, v));
                    if (pkg.AddFile(v, file) != null)
                    {
                        file.Delete();
                    }
                }


                FileInfo tmpFile = new(Path.Join(tmpDir.FullName, att.FileName + "_nekotmp.vpk"));
                if (tmpFile.Exists)
                    goto cancel;
                tmpFile.Create().Close();
                tmpFile.Attributes |= FileAttributes.Temporary;


                if (neko7zFiles.Count > 0)
                {
                    SevenZipCompressor compressor = new() { 
                        CompressionMode = CompressionMode.Create,
                        ArchiveFormat = OutArchiveFormat.SevenZip,
                        CompressionLevel = CompressionLevel.Ultra,
                    };
                    compressor.CompressFileDictionary(neko7zFiles, tmpFile.FullName);
                    tmpFile.Refresh();
                    if (!tmpFile.Exists)
                        goto cancel;
                    else
                    {
                        var reader = tmpFile.OpenRead();
                        pkg.AddFile(pkg.GenNekoDir() + "0.neko7z", reader);
                        reader.Close();
                        tmpFile.Delete();
                    }
                }

                FileInfo srcPakFile = new(pkg.FileName + ".vpk");
                pkg.Write(tmpFile.FullName, 1);
                pkg.Dispose();

                // overwrite origin file
                tmpFile.Refresh();
                tmpFile.LastWriteTime = srcPakFile.LastWriteTime;
                tmpFile.CreationTime = srcPakFile.CreationTime;
                tmpFile.MoveTo(srcPakFile.FullName, true);


                // update UI
                ModifiedAssetTags.Clear();
                AssetTagModifiedPanel.IsVisible = false;

            clean:
                extractor?.Dispose();
                zipStream?.Dispose();
                if (tmpDir.Exists)
                    tmpDir.Delete(true);
                return;
            cancel:
                CancelAssetTagChange();
                goto clean;
            }
            finally
            {
                pkg?.Dispose();
            }
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

    private void SubmitAddonSearch()
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Addons.Refresh();
        }
    }

    private void Button_AddonSearch_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SubmitAddonSearch();
    }

    private void TextBox_AddonSearch_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            SubmitAddonSearch();
        }
    }

    private void TextBox_AddonSearch_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        SubmitAddonSearch();
    }
}
