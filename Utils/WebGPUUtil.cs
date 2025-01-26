namespace SourEngine.Utils;

public unsafe class WebGPUUtil
{
    public static ShaderModuleUtil ShaderModule { get; } = new ShaderModuleUtil();
    public static RenderPipelineUtil RenderPipeline { get; } = new RenderPipelineUtil();
}