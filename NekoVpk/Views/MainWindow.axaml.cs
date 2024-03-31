using Avalonia.Controls;
using System;
using OSVersionExt;
using OSVersionExtension;

namespace NekoVpk.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (OSVersion.GetOperatingSystem() != OSVersionExtension.OperatingSystem.Windows11)
        {
            //this.TransparencyLevelHint = "None";
        }
    }

}
