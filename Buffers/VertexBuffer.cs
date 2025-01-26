using SourEngine.Utils;
using Buffer = Silk.NET.WebGPU.Buffer;

namespace SourEngine.Buffers;

public unsafe class VertexBuffer : IDisposable
{
    private readonly Engine _engine;

    public uint VertexCount { get; private set; }

    public VertexBuffer(Engine engine)
    {
        _engine = engine;
    }
    
    public Buffer* Buffer { get; private set; }
    
    public uint Size { get; private set; }

    public void Initialize(float[] data, uint vertexCount = 0)
    {
        Size = (uint) data.Length * sizeof(float);
        Buffer = WebGPUUtil.Buffer.Create(_engine, data);
        VertexCount = vertexCount;
    }

    public void Dispose()
    {
        _engine.WGPU.BufferDestroy(Buffer);
    }
}