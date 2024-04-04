using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using NekoVpk.ViewModels;
using NekoVpk.Views;

namespace NekoVpk;

public partial class App : Application
{
    public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public const string Version = "0.0.7.0";

    public const string VersionSuffix = "-Beta";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            desktop.Startup += OnStartup;
            desktop.Exit += OnExit;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    private void OnStartup(object? s, ControlledApplicationLifetimeStartupEventArgs e)
    {

    }
    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        //NekoSettings.Default.Reset();
        NekoSettings.Default.Save();
    }
}
