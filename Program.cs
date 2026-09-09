using System;
using Avalonia;

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
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
