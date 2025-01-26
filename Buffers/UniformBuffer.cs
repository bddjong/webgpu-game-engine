using SourEngine.Utils;
using Buffer = Silk.NET.WebGPU.Buffer;

namespace SourEngine.Buffers;

public unsafe class UniformBuffer<T> : IDisposable where T : unmanaged
{
    private readonly Engine _engine;
    
    public UniformBuffer(Engine engine)
    {
        _engine = engine;
    }
    
    public Buffer* Buffer { get; private set; }
    
    public uint Size { get; private set; }

    public void Initialize(T data)
    {
        Size = (uint) sizeof(T);
        Buffer = WebGPUUtil.Buffer.CreateUniformBuffer(_engine, data);
    }

    public void Update(T data)
    {
        WebGPUUtil.Buffer.WriteUniformBuffer(_engine, Buffer, data);
    }

    public void Dispose()
    {
        _engine.WGPU.BufferDestroy(Buffer);
    }
}