using Uno.UI.Hosting;

namespace Agentic_Desktop;

/// <summary>
/// Entry point for the Skia desktop (net10.0-desktop) target.
/// The Windows (WinAppSDK) target uses the XAML-generated Main instead.
/// </summary>
public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseX11()
            .UseLinuxFrameBuffer()
            .UseMacOS()
            .UseWin32()
            .Build();

        host.Run();
    }
}
