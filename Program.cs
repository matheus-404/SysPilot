using System;
using Avalonia;
using Velopack;

namespace SysPilot
{
    class Program
    {
        // Avalonia's initialization entry point. Note: DO NOT call
        // AppBuilder.Configure<App>().Start(...) directly; use
        // StartWithClassicDesktopLifetime so the app behaves like a
        // normal desktop window (this is the Avalonia equivalent of
        // App.xaml's OnLaunched in WinUI).
        [STAThread]
        public static void Main(string[] args)
        {
            // Must be the very first line executed - Velopack intercepts
            // special install/update/uninstall args before any Avalonia
            // or UI code runs.
            VelopackApp.Build().Run();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}