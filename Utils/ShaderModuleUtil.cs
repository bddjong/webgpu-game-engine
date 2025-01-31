using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace SourEngine.Utils;

public unsafe class ShaderModuleUtil
{
    public ShaderModule* CreateShaderModule(Engine engine, string filePath = "Shaders/unlit.wgsl", string label = "Unlit Shader Module")
    {
        string shaderCode = File.ReadAllText("Shaders/unlit.wgsl");
        
        engine.WGPU.DevicePushErrorScope(engine.Device, ErrorFilter.Validation);
        
        var shaderCodePtr = Marshal.StringToHGlobalAnsi(shaderCode);
        var labelPtr = Marshal.StringToHGlobalAnsi($"{label} ({filePath})");
        
        ShaderModuleWGSLDescriptor wgslDescriptor = new ShaderModuleWGSLDescriptor
        {
            Code = (byte*)shaderCodePtr,
            Chain =
            {
                SType = SType.ShaderModuleWgslDescriptor,
            },
        };

        ShaderModuleDescriptor descriptor = new ShaderModuleDescriptor();
        descriptor.NextInChain = &wgslDescriptor.Chain;
        descriptor.Label = (byte*)labelPtr;
        ShaderModule* shaderModule = engine.WGPU.DeviceCreateShaderModule(engine.Device, descriptor);
        
        engine.WGPU.DevicePopErrorScope(engine.Device, PfnErrorCallback.From((errorType, messagePtr, userData) =>
        {
            if(errorType == ErrorType.NoError)
            {
                return;
            }
            
            string message = Marshal.PtrToStringAnsi((IntPtr)messagePtr);
            Console.WriteLine($"Error: {errorType}, {message}");
            throw new InvalidOperationException(message);
        }), null);
        
        Console.WriteLine("Created shader module: " + wgslDescriptor.Chain.SType);
        
        Marshal.FreeHGlobal(shaderCodePtr);
        Marshal.FreeHGlobal(labelPtr);
        
        return shaderModule;
    }
}