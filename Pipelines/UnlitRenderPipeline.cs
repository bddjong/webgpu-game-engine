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

    public void Render(VertexBuffer vertexBuffer, IndexBuffer? indexBuffer = null)
    {
        _engine.WGPU.RenderPassEncoderSetPipeline(_engine.CurrentRenderPassEncoder, _renderPipeline);

        _engine.WGPU.RenderPassEncoderSetVertexBuffer(_engine.CurrentRenderPassEncoder,
            0,
            vertexBuffer.Buffer,
            0,
            vertexBuffer.Size);

        if (indexBuffer != null)
        {
            _engine.WGPU.RenderPassEncoderSetIndexBuffer(_engine.CurrentRenderPassEncoder,
                indexBuffer.Buffer,
                IndexFormat.Uint16,
                0,
                indexBuffer.Size);
            
            _engine.WGPU.RenderPassEncoderDrawIndexed(_engine.CurrentRenderPassEncoder,
                indexBuffer.IndicesCount,
                1,
                0,
                0,
                0);
        }
        else
        {
            _engine.WGPU.RenderPassEncoderDraw(_engine.CurrentRenderPassEncoder,
                vertexBuffer.VertexCount,
                1,
                0,
                0);
        }
    }

    public void Dispose()
    {
        _engine.WGPU.RenderPipelineRelease(_renderPipeline);
    }
}