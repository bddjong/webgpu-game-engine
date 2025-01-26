using System.Runtime.InteropServices;
using Silk.NET.WebGPU;
using SourEngine.Buffers;
using SourEngine.Utils;
using Buffer = Silk.NET.WebGPU.Buffer;

namespace SourEngine.Pipelines;

public unsafe class UnlitRenderPipeline : IDisposable
{
    private readonly Engine _engine;
    private RenderPipeline* _renderPipeline;

    public UnlitRenderPipeline(Engine engine)
    {
        _engine = engine;
    }

    public void Initialize()
    {
        ShaderModule* shaderModule = WebGPUUtil.ShaderModule.CreateShaderModule(_engine, "Shaders/unlit.wgsl");
        _renderPipeline = WebGPUUtil.RenderPipeline.Create(_engine, shaderModule);
        
        _engine.WGPU.ShaderModuleRelease(shaderModule);
    }

    public void Render(VertexBuffer vertexBuffer)
    {
        _engine.WGPU.RenderPassEncoderSetPipeline(_engine.CurrentRenderPassEncoder, _renderPipeline);
        
        // Set buffers
        _engine.WGPU.RenderPassEncoderSetVertexBuffer(_engine.CurrentRenderPassEncoder, 0, vertexBuffer.Buffer, 0, vertexBuffer.Size);
        
        _engine.WGPU.RenderPassEncoderDraw(_engine.CurrentRenderPassEncoder, 3, 1, 0, 0);
    }

    public void Dispose()
    {
        _engine.WGPU.RenderPipelineRelease(_renderPipeline);
    }
}