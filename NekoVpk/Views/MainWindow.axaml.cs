using Avalonia.Controls;
using System;
using OSVersionExt;
using OSVersionExtension;
using Avalonia.Media;

namespace NekoVpk.Views;

public partial class MainWindow : Window
{
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


    }

}
