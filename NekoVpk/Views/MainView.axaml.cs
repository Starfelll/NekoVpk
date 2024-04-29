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
using SevenZip;
using System.Linq;
using ReactiveUI;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace NekoVpk.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    public override void EndInit()
    {
        //ReloadAddonList();
        base.EndInit();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        ReloadAddonList();
        base.OnDataContextChanged(e);
    }

    private void DataGrid_CurrentCellChanged(object? sender, System.EventArgs e)
    {
        CancelAssetTagChange();
        if (sender is DataGrid dg && dg.SelectedItem is AddonAttribute att)
        {
            AddonDetailPanel.IsVisible = true;
            Package? pak = null;
            try
            {
                pak = att.LoadPackage(NekoSettings.Default.GameDir);
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
                FileInfo jpg = new(Path.ChangeExtension(att.GetAbsolutePath(NekoSettings.Default.GameDir), "jpg"));
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
        ReloadAddonList();
    }

    private void ReloadAddonList()
    {
        if (DataContext is MainViewModel vm)
        {
            Dispatcher.UIThread.Post(() => {
                vm.LoadAddons();
            }, DispatcherPriority.Background);
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
        if (e.EditAction == DataGridEditAction.Commit)
        {
            AddonList addonList = new();
            addonList.Load(NekoSettings.Default.GameDir);
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
                addonList.Save(NekoSettings.Default.GameDir);
            }
        }
    }

    private void AddonList_Menu_LocateFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (AddonList.SelectedItem is AddonAttribute att)
        {
            FileInfo fileInfo = new(att.GetAbsolutePath(NekoSettings.Default.GameDir));
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
                    FileName = att.GetAbsolutePath(NekoSettings.Default.GameDir),
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
                case "Bill":
                case "Coach":
                case "Ellis":
                case "Francis":
                case "Louis":
                case "Nick":
                case "Rochelle":
                case "Zoey":
                case "BillDeathPose":
                case "FrancisLight":
                case "ZoeyLight":
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

                FileInfo tmpFile = new(Path.Join(tmpDir.FullName, att.FileName + ".nekotmp"));

                SevenZipExtractor? extractor = null;
                SevenZipCompressor? compressor = null;

                // find neko7z
                foreach (var entry in pkg.Entries)
                {
                    if (entry.Key == "neko7z" && entry.Value[0].FileName == "0")
                    {
                        pkg.ExtratFile(entry.Value[0], tmpFile);
                        tmpFile.Refresh();
                        pkg.RemoveFile(entry.Value[0]);
                        extractor = new(tmpFile.FullName);
                        break;
                    }
                }

                if (!tmpFile.Exists) tmpFile.Create().Close();
                tmpFile.Attributes |= FileAttributes.Temporary;

                Dictionary<int, string> disableZipFiles = [];
                List<PackageEntry> disableEntries = [];
                List<string> vpkFiles = [];
                Dictionary<string, string> zipFiles = [];

                if (extractor is not null)
                {
                    foreach (var zipFile in extractor.ArchiveFileData)
                    {
                        if (zipFile.IsDirectory) continue;
                        foreach (var tag in ModifiedAssetTags.Keys)
                        {
                            if (tag.Enable && tag.Proporty.IsMatch(zipFile.FileName))
                            {
                                disableZipFiles.Add(zipFile.Index, null);
                                vpkFiles.Add(zipFile.FileName);
                                break;
                            }
                        }
                    }
                }
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


                compressor = new()
                {
                    CompressionMode = extractor is null ? CompressionMode.Create : CompressionMode.Append,
                    ArchiveFormat = OutArchiveFormat.SevenZip,
                    //CompressionLevel = (CompressionLevel)NekoSettings.Default.CompressionLevel,
                    CompressionLevel = CompressionLevel.Ultra,
                    CompressionMethod = CompressionMethod.Lzma2,
                };


                // move zip files to vpk
                if (extractor != null && disableZipFiles.Count > 0) {
                    extractor.ExtractFiles(tmpDir.FullName, disableZipFiles.Keys.ToArray());
                    foreach (var v in vpkFiles)
                    {
                        FileInfo file = new (Path.Join(tmpDir.FullName, v));
                        if (pkg.AddFile(v, file) != null)
                        {
                        }
                    }
                    // delete file in archive
                    compressor.ModifyArchive(tmpFile.FullName, disableZipFiles);
                }

                // move vpk files to zip
                foreach (var entry in disableEntries)
                {
                    FileInfo outFile = new(Path.Join(tmpDir.FullName, entry.GetFullPath()));
                    pkg.ExtratFile(entry, outFile);

                    zipFiles.Add(entry.GetFullPath(), outFile.FullName);
                    pkg.RemoveFile(entry);
                }
                if (zipFiles.Count > 0)
                {
                    compressor.CompressFileDictionary(zipFiles, tmpFile.FullName);
                }

                
                if (zipFiles.Count > 0 || (extractor != null && disableZipFiles.Count < extractor.ArchiveFileData.Count))
                {
                    tmpFile.Refresh();
                    pkg.AddFile(pkg.GenNekoDir() + "0.neko7z", tmpFile);
                    tmpFile.Delete();
                }

                string originFilePath = pkg.FileName + ".vpk";
                FileInfo srcPakFile = new(originFilePath);
                pkg.Write(tmpFile.FullName, 1);
                pkg.Dispose();

                srcPakFile.MoveTo(Path.ChangeExtension(originFilePath, ".vpk.nekobak"), true);

                // overwrite origin file
                tmpFile.Refresh();
                tmpFile.LastWriteTime = srcPakFile.LastWriteTime;
                tmpFile.CreationTime = srcPakFile.CreationTime;
                tmpFile.MoveTo(originFilePath, true);


                // update UI
                ModifiedAssetTags.Clear();
                AssetTagModifiedPanel.IsVisible = false;

            clean:
                extractor?.Dispose();
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

    private void DataGrid_Sorting(object? sender, Avalonia.Controls.DataGridColumnEventArgs e)
    {
        
    }

    private async void Settings_Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.VisualRoot is Window window)
        {
            var settingsWindow = new SettingsWindow()
            {
                DataContext = new ViewModels.Settings(),
                Background = window.Background,
            };
            await settingsWindow.ShowDialog(window);
        }
        
    }
}
