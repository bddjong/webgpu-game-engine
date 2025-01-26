using SourEngine.Utils;
using Buffer = Silk.NET.WebGPU.Buffer;

namespace SourEngine.Buffers;

public unsafe class IndexBuffer
{
    private readonly Engine _engine;
    
    public Buffer* Buffer { get; private set; }
    
    public uint Size { get; private set; }
    public uint IndicesCount { get; private set; }
    
    public IndexBuffer(Engine engine)
    {
        _engine = engine;
    }

    public void Initialize(ushort[] data)
    {
        Size = (uint)data.Length * sizeof(ushort);
        Buffer = WebGPUUtil.Buffer.CreateIndexBuffer(_engine, data);
        IndicesCount = (uint)data.Length;
    }

    public void Dispose()
    {
        _engine.WGPU.BufferDestroy(Buffer);
    }
}