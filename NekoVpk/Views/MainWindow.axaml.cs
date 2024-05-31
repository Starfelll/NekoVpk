using Avalonia.Controls;
using System;
using OSVersionExt;
using OSVersionExtension;
using Avalonia.Media;
using Jot;
using System.Linq;

namespace NekoVpk.Views;

public partial class MainWindow : Window
{
    static Jot.Tracker tracker = new Tracker();

    public MainWindow()
    {
        InitializeComponent();
        Title = $"NekoVpk {App.Version}{App.VersionSuffix}    bilibili@Starfelll";
        if (Background is SolidColorBrush brush)
        {
            if (ActualTransparencyLevel == WindowTransparencyLevel.AcrylicBlur || ActualTransparencyLevel == WindowTransparencyLevel.Blur)
            {
                brush.Opacity = 0.6;
            }
            else if (ActualTransparencyLevel == WindowTransparencyLevel.None)
            {
                brush.Opacity = 1;
            }
            else
            {
                brush.Opacity = 0;
            }
        }


        var initialPosition = this.Position;
        var trackerNamespace = string.Join("_", Screens.All.Select(s => s.WorkingArea.Size.ToString()));
        trackerNamespace += "__" + Environment.ProcessPath?.Replace("/", "_").Replace("\\", "_"); // <-- Add this if you want multiple copies of the same app to have different configurations.
        tracker.Configure<Window>()
            .Id(w => w.Name, trackerNamespace)
            .Properties(w => new { w.Position, w.Width, w.Height })
            .PersistOn(nameof(Window.Closing))
            .StopTrackingOn(nameof(Window.Closing));
        tracker.Track(this);

        if (this.Position.X < -1 || this.Position.Y < -1) // -1 is used by Windows11 when docking windows on the side.
        {
            this.Position = initialPosition;
        }
    }

    private void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}
