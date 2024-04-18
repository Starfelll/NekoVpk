using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.IO;
using System;

namespace NekoVpk.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            //ComboBox_CompressionLevel.ItemsSource = Enum.GetValues(typeof(SevenZip.CompressionLevel));
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
            var result = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = "Select Game Directory",
                AllowMultiple = false,
                SuggestedStartLocation = suggestedStartLocation
            });


            if (result is not null && result.Count == 1)
            {
                var dirInfo = new DirectoryInfo(result[0].Path.LocalPath);
                var dirs = dirInfo.GetDirectories("addons");
                if (dirs.Length == 0)
                {
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
    }
}
