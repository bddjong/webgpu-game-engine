using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace SourEngine.Utils;

public unsafe class ShaderModuleUtil
{
    public ShaderModule* CreateShaderModule(Engine engine, string filePath = "Shaders/unlit.wgsl")
    {
        string shaderCode = File.ReadAllText("Shaders/unlit.wgsl");
        
        var shaderCodePtr = Marshal.StringToHGlobalAnsi(shaderCode);
        
        ShaderModuleWGSLDescriptor wgslDescriptor = new ShaderModuleWGSLDescriptor
        {
            Code = (byte*)shaderCodePtr,
            Chain =
            {
                SType = SType.ShaderModuleWgslDescriptor
            }
        };

        ShaderModuleDescriptor descriptor = new ShaderModuleDescriptor();
        descriptor.NextInChain = &wgslDescriptor.Chain;
        ShaderModule* shaderModule = engine.WGPU.DeviceCreateShaderModule(engine.Device, descriptor);
        Console.WriteLine("Created shader module: " + wgslDescriptor.Chain.SType);
        
        Marshal.FreeHGlobal(shaderCodePtr);
        
        return shaderModule;
    }
}