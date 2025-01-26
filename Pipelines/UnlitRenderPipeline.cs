using Silk.NET.Maths;
using Silk.NET.WebGPU;
using SourEngine.Buffers;
using SourEngine.Utils;

namespace SourEngine.Pipelines;

public unsafe class UnlitRenderPipeline : IDisposable
{
    private readonly Engine _engine;
    private RenderPipeline* _renderPipeline;
    
    private Matrix4X4<float> _transform = Matrix4X4<float>.Identity;
    private UniformBuffer<Matrix4X4<float>> _transformBuffer;
    private BindGroupLayout* _transformBindGroupLayout;
    private BindGroup* _transformBindGroup;
    
    public UnlitRenderPipeline(Engine engine)
    {
        _engine = engine;
    }

    public Matrix4X4<float> Transform
    {
        get => _transform;
        set
        {
            _transform = value;
            _transformBuffer.Update(_transform);
        }
    }
    
    public void CreateResources()
    {
        _transformBuffer = new UniformBuffer<Matrix4X4<float>>(_engine);
        _transformBuffer.Initialize(_transform);
    }
    
    private void CreateBindGroupLayout()
    {
        BindGroupLayoutEntry* bindGroupLayoutEntry = stackalloc BindGroupLayoutEntry[1];
        bindGroupLayoutEntry[0] = new BindGroupLayoutEntry
        {
            Binding = 0,
            Visibility = ShaderStage.Vertex,
            Buffer = new BufferBindingLayout
            {
                Type = BufferBindingType.Uniform
            }
        };
        
        BindGroupLayoutDescriptor bindGroupLayoutDescriptor = new BindGroupLayoutDescriptor
        {
            Entries = bindGroupLayoutEntry,
            EntryCount = 1
        };
        
        _transformBindGroupLayout = _engine.WGPU.DeviceCreateBindGroupLayout(_engine.Device, bindGroupLayoutDescriptor);
    }

    private void CreateBindGroups()
    {
        BindGroupEntry* bindGroupEntries = stackalloc BindGroupEntry[1];
        bindGroupEntries[0] = new BindGroupEntry
        {
            Binding = 0,
            Buffer = _transformBuffer.Buffer,
            Offset = 0,
            Size = _transformBuffer.Size
        };
        
        BindGroupDescriptor descriptor = new BindGroupDescriptor
        {
            Layout = _transformBindGroupLayout,
            Entries = bindGroupEntries,
            EntryCount = 1
        };
        
        _transformBindGroup = _engine.WGPU.DeviceCreateBindGroup(_engine.Device, descriptor);
    }

    public void Initialize()
    {
        // Bind group layout
        CreateBindGroupLayout();
        
        // Shader module
        ShaderModule* shaderModule = WebGPUUtil.ShaderModule.CreateShaderModule(_engine, "Shaders/unlit.wgsl");
        
        // Pipeline layout
        BindGroupLayout** bindGroupLayout = stackalloc BindGroupLayout*[1];
        bindGroupLayout[0] = _transformBindGroupLayout;
        
        PipelineLayoutDescriptor pipelineLayoutDescriptor = new PipelineLayoutDescriptor
        {
            BindGroupLayouts = bindGroupLayout,
            BindGroupLayoutCount = 1
        };
        
        PipelineLayout* pipelineLayout = _engine.WGPU.DeviceCreatePipelineLayout(_engine.Device, pipelineLayoutDescriptor);
        
        _renderPipeline = WebGPUUtil.RenderPipeline.Create(_engine, shaderModule, pipelineLayout);
        
        // Resources
        CreateResources();
        
        // BindGroups
        CreateBindGroups();

        _engine.WGPU.ShaderModuleRelease(shaderModule);
    }

    public void Render(VertexBuffer vertexBuffer, IndexBuffer? indexBuffer = null)
    {
        _engine.WGPU.RenderPassEncoderSetPipeline(_engine.CurrentRenderPassEncoder, _renderPipeline);
        
        _engine.WGPU.RenderPassEncoderSetBindGroup(_engine.CurrentRenderPassEncoder, 0, _transformBindGroup, 0, 0);

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
        _transformBuffer.Dispose();
        _engine.WGPU.RenderPipelineRelease(_renderPipeline);
    }
}