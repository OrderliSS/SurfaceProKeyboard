using SurfaceTypeCoverManager.Services.Services;
using System.Text.Json.Serialization;

using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

app.MapGet("/api/device", async () =>
{
    var service = new SurfaceService();
    var details = await service.DetectSurfaceDeviceAsync();
    return Results.Json(details);
});

[DllImport("user32.dll", SetLastError = true)]
static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

const byte VK_KBD_BRIGHTNESS_UP = 0x97; // 151
const byte VK_KBD_BRIGHTNESS_DOWN = 0x98; // 152
const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
const uint KEYEVENTF_KEYUP = 0x0002;

app.MapPost("/api/lighting", (LightingRequest request) =>
{
    Console.WriteLine($"Lighting Update Received: Power={request.Power}, Brightness={request.Brightness}%, Color={request.Color}");

    // Best-effort software hardware signal dispatch for generic generic / 3rd party keyboards
    // If the device ignores these events, it indicates hardwired physical-only firmware control.
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

app.Run("http://localhost:5000");

public class LightingRequest
{
    public bool Power { get; set; }
    public int Brightness { get; set; }
    public string Color { get; set; }
}
