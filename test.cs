using System;
using System.Reflection;
using System.Threading.Tasks;

class Program {
    static void Main() {
        var asm = Assembly.LoadFrom(@"c:\Users\matth\Documents\Projects\SurfaceProKeyboardClient\SurfaceTypeCoverManager.Services\bin\Debug\net8.0-windows10.0.19041.0\SurfaceTypeCoverManager.Services.dll");
        var type = asm.GetType("SurfaceTypeCoverManager.Services.Services.SurfaceService");
        var svc = Activator.CreateInstance(type);
        var method = type.GetMethod("DetectSurfaceDeviceAsync");
        var task = (Task)method.Invoke(svc, null);
        task.Wait();
        var resultProp = task.GetType().GetProperty("Result");
        var result = resultProp.GetValue(task);
        
        foreach(var prop in result.GetType().GetProperties()) {
            Console.WriteLine(prop.Name + ": " + prop.GetValue(result));
        }
    }
}
