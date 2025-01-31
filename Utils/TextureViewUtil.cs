using System.Runtime.InteropServices;

namespace SourEngine.Utils;

using Silk.NET.WebGPU;

public unsafe class TextureViewUtil
{
    public TextureView* Create(Engine engine, Texture* texture, string label = "TextureView")
    {
        var labelPtr = Marshal.StringToHGlobalAnsi(label);
        
        var descriptor = new TextureViewDescriptor
        {
            Format = engine.PreferredTextureFormat,
            Dimension = TextureViewDimension.Dimension2D,
            Aspect = TextureAspect.All,
            BaseMipLevel = 0,
            MipLevelCount = 1,
            BaseArrayLayer = 0,
            ArrayLayerCount = 1,
            Label = (byte*)labelPtr
        };
        
        var textureView = engine.WGPU.TextureCreateView(texture, descriptor);
        
        Marshal.FreeHGlobal(labelPtr);

        return textureView;
    }
}