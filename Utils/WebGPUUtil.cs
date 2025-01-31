namespace SourEngine.Utils;

public unsafe class WebGPUUtil
{
    public static ShaderModuleUtil ShaderModule { get; } = new ShaderModuleUtil();
    public static RenderPipelineUtil RenderPipeline { get; } = new RenderPipelineUtil();
    public static BufferUtil Buffer { get; } = new BufferUtil();
    public static TextureUtil Texture { get; } = new TextureUtil();
    public static TextureViewUtil TextureView { get; } = new TextureViewUtil();
    public static SamplerUtil Sampler { get; } = new SamplerUtil();
}