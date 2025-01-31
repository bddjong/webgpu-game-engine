using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace SourEngine.Utils;

public unsafe class SamplerUtil
{
    public Sampler* Create(Engine engine, string label = "Sampler")
    {
        var labelPtr = Marshal.StringToHGlobalAnsi(label);

        SamplerDescriptor desc = new SamplerDescriptor();
        SamplerDescriptor descriptor = new()
        {
            Label = (byte*)labelPtr,
            MaxAnisotropy = 1,
            MinFilter = FilterMode.Linear,
            MagFilter = FilterMode.Linear,
            AddressModeU = AddressMode.Repeat,
            AddressModeV = AddressMode.Repeat,
        };
        
        var sampler = engine.WGPU.DeviceCreateSampler(engine.Device, descriptor);
        
        Marshal.FreeHGlobal(labelPtr);
        
        return sampler;
    }
}