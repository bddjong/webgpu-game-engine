using Silk.NET.Maths;
using SkiaSharp;
using SourEngine.Utils;

namespace SourEngine.Texture;

using Silk.NET.WebGPU;

public unsafe class Texture2D : IDisposable
{
    private readonly Engine _engine;
    private readonly SKImage _image;
    private byte[] _data;
    private Vector2D<uint> _size;
    
    public Texture2D(Engine engine, SKImage image, string label)
    {
        _engine = engine;
        _image = image;
        Label = label;
    }
    
    public Texture2D(Engine engine, Vector2D<uint> size, byte[] data, string label = "Texture2D")
    {
        _engine = engine;
        _data = data;
        _size = size;
        Label = label;
    }
    
    public string Label { get; private set; }
    
    public Texture* Texture { get; private set; }
    
    public TextureView* TextureView { get; private set; }
    
    public Sampler* Sampler { get; private set; }

    public void Initialize()
    {
        if (_image != null)
        {
            Texture = WebGPUUtil.Texture.Create(_engine, _image, Label);
        }
        else
        {
            Texture = WebGPUUtil.Texture.Create(_engine, _data, _size, Label);
        }
        TextureView = WebGPUUtil.TextureView.Create(_engine, Texture, Label);
        Sampler = WebGPUUtil.Sampler.Create(_engine, Label);
    }
    
    
    public static Texture2D CreateEmptyTexture(Engine engine, string label = "Empty Texture2D")
    {
        Texture2D texture = new(engine, new(1, 1), [ 255, 255, 255, 255 ], label);
        texture.Initialize();
        return texture;
    }
    
    public void Dispose()
    {
        _engine.WGPU.SamplerRelease(Sampler);
        _engine.WGPU.TextureViewRelease(TextureView);
        _engine.WGPU.TextureRelease(Texture);
    }
}