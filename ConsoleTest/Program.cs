using System;
using System.Text.Json;
using System.Threading.Tasks;
using SurfaceTypeCoverManager.Services.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Running Hardware Detection Logic...");
        var service = new SurfaceService();
        var details = await service.DetectSurfaceDeviceAsync();
        
        Console.WriteLine("\n--- DETECTION RESULTS ---");
        var options = new JsonSerializerOptions { WriteIndented = true };
        Console.WriteLine(JsonSerializer.Serialize(details, options));
    }
}
