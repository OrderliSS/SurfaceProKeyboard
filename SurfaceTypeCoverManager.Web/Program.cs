using SurfaceTypeCoverManager.Services.Services;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

namespace SurfaceTypeCoverManager.Web;

/// <summary>
/// Hosts the Kestrel web server. Can be started in-process from the WPF UI 
/// or standalone via `dotnet run`.
/// </summary>
public static class WebServerHost
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const byte VK_KBD_BRIGHTNESS_UP = 0x97;
    private const byte VK_KBD_BRIGHTNESS_DOWN = 0x98;
    private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    public static WebApplication BuildApp(string? webRootOverride = null)
    {
        var builder = WebApplication.CreateBuilder();

        // Resolve wwwroot: prefer override, then assembly-relative, then default
        if (webRootOverride != null && Directory.Exists(webRootOverride))
        {
            builder.Environment.WebRootPath = webRootOverride;
        }
        else
        {
            var assemblyDir = Path.GetDirectoryName(typeof(WebServerHost).Assembly.Location)!;
            var wwwrootPath = Path.Combine(assemblyDir, "wwwroot");
            if (Directory.Exists(wwwrootPath))
            {
                builder.Environment.WebRootPath = wwwrootPath;
            }
        }

        var app = builder.Build();

        app.UseStaticFiles();

        app.MapGet("/api/device", async () =>
        {
            var service = new SurfaceService();
            var details = await service.DetectSurfaceDeviceAsync();
            return Results.Json(details);
        });

        app.MapPost("/api/lighting", (LightingRequest request) =>
        {
            Console.WriteLine($"Lighting Update Received: Power={request.Power}, Brightness={request.Brightness}%, Color={request.Color}");

            try
            {
                byte vk = request.Power && request.Brightness > 50 ? VK_KBD_BRIGHTNESS_UP : VK_KBD_BRIGHTNESS_DOWN;
                keybd_event(vk, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
                keybd_event(vk, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            catch { }

            return Results.Ok(new { success = true });
        });

        app.MapFallbackToFile("index.html");

        return app;
    }

    public static async Task StartAsync(string? webRootOverride = null, CancellationToken cancellationToken = default)
    {
        var app = BuildApp(webRootOverride);
        await app.RunAsync("http://localhost:5000");
    }
}

public class LightingRequest
{
    public bool Power { get; set; }
    public int Brightness { get; set; }
    public string Color { get; set; } = string.Empty;
}

// Standalone entry point for `dotnet run`
public class Program
{
    public static async Task Main(string[] args)
    {
        await WebServerHost.StartAsync();
    }
}
