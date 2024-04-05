using System;
using Avalonia;
using NLog;
using AutoUpdaterDotNET;
using System.Reflection;
using System.Drawing;
using Avalonia.Controls.ApplicationLifetimes;
using NekoVpk.Desktop.Properties;

namespace NekoVpk.Desktop;

class Program
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {

#if !DEBUG
        try
#endif
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, 
                new Action<IClassicDesktopStyleApplicationLifetime>(v => v.Startup += OnStartup)
            );
        }
#if !DEBUG
        catch (Exception e)
        {
            Logger.Error(e);
        }
#endif

    }

    private static void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
#if !DEBUG
        CheckUpdate();
#endif
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    static void CheckUpdate()
    {
#if DEBUG
        //AutoUpdater.AppCastURL = @"https://raw.githubusercontent.com/Starfelll/NekoVpk_release/main/AppUpdate.xml";
#else
        AutoUpdater.AppCastURL = @"https://raw.githubusercontent.com/Starfelll/NekoVpk/main/AppUpdate.xml";
#endif
        AutoUpdater.UpdateMode = Mode.Normal;
        //var path = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        AutoUpdater.Icon = new Bitmap(Resource.update_icon, new(64, 64));
        
        AutoUpdater.TopMost = true;
        //AutoUpdater.Synchronous = true;
        
        AutoUpdater.Start(Assembly.GetAssembly(typeof(App)));
    }
}
