using Silk.NET.WebGPU;
using Buffer = Silk.NET.WebGPU.Buffer;

namespace SourEngine.Utils;

public unsafe class BufferUtil
{
    public Buffer* Create(Engine engine, float[] data)
    {
        uint size = (uint) data.Length * sizeof(float);
        BufferDescriptor bufferDescriptor = new BufferDescriptor
        {
            MappedAtCreation = false,
            Size = size,
            Usage = BufferUsage.Vertex | BufferUsage.CopyDst
        };
        
        Buffer* buffer = engine.WGPU.DeviceCreateBuffer(engine.Device, bufferDescriptor);
        
        fixed (float* dataPtr = data)
        {
            engine.WGPU.QueueWriteBuffer(engine.Queue, buffer, 0, dataPtr, size);
        }
        
        return buffer;
    }
}