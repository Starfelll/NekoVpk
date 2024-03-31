using System;
using System.Threading.Tasks;
using Avalonia;
using NLog;

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

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
#if !DEBUG
        catch (Exception e)
        {
            Logger.Error(e);



        }
#endif
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();


}
