using Silk.NET.WebGPU;
using Buffer = Silk.NET.WebGPU.Buffer;

namespace SourEngine.Utils;

public unsafe class BufferUtil
{
    public Buffer* CreateVertexBuffer(Engine engine, float[] data)
    {
        uint size = (uint)data.Length * sizeof(float);
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

    public Buffer* CreateIndexBuffer(Engine engine, ushort[] data)
    {
        uint size = (uint)data.Length * sizeof(ushort);
        BufferDescriptor bufferDescriptor = new BufferDescriptor
        {
            MappedAtCreation = false,
            Size = size,
            Usage = BufferUsage.Index | BufferUsage.CopyDst
        };

        Buffer* buffer = engine.WGPU.DeviceCreateBuffer(engine.Device, bufferDescriptor);

        fixed (ushort* dataPtr = data)
        {
            engine.WGPU.QueueWriteBuffer(engine.Queue, buffer, 0, dataPtr, size);
        }

        return buffer;
    }

    public Buffer* CreateUniformBuffer<T>(Engine engine, T data) where T : unmanaged
    {
        uint size = (uint)sizeof(T);
        BufferDescriptor bufferDescriptor = new BufferDescriptor
        {
            MappedAtCreation = false,
            Size = size,
            Usage = BufferUsage.Uniform | BufferUsage.CopyDst
        };

        Buffer* buffer = engine.WGPU.DeviceCreateBuffer(engine.Device, bufferDescriptor);

        engine.WGPU.QueueWriteBuffer(engine.Queue, buffer, 0, data, size);

        return buffer;
    }
    
    public void WriteUniformBuffer<T>(Engine engine, Buffer* buffer, T data) where T : unmanaged
    {
        engine.WGPU.QueueWriteBuffer(engine.Queue, buffer, 0, data, (uint)sizeof(T));
    }
}