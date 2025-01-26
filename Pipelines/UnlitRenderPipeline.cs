using System.Runtime.InteropServices;
using Silk.NET.WebGPU;
using SourEngine.Utils;

namespace SourEngine.Pipelines;

public unsafe class UnlitRenderPipeline
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

        VertexState vertexState = new VertexState
        {
            Module = shaderModule,
            EntryPoint = (byte*)Marshal.StringToHGlobalAnsi("main_vs")
        };

        BlendState* blendState = stackalloc BlendState[1];
        blendState[0].Color = new BlendComponent
        {
            SrcFactor = BlendFactor.One,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        };
        blendState[0].Alpha = new BlendComponent
        {
            SrcFactor = BlendFactor.One,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        };
        
        ColorTargetState* colorTargetState = stackalloc ColorTargetState[1];
        colorTargetState[0].WriteMask = ColorWriteMask.All;
        colorTargetState[0].Blend = blendState;
        colorTargetState[0].Format = _engine.PreferredTextureFormat;
        
        FragmentState fragmentState = new FragmentState
        {
            Module = shaderModule,
            EntryPoint = (byte*)Marshal.StringToHGlobalAnsi("main_fs"),
            Targets = colorTargetState,
            TargetCount = 1
        };

        RenderPipelineDescriptor pipelineDescriptor = new RenderPipelineDescriptor
        {
            Vertex = vertexState,
            Fragment = &fragmentState,
            Multisample = new MultisampleState
            {
                Mask = 0xFFFFFFFF,
                Count = 1,
                AlphaToCoverageEnabled = false
            },
            Primitive = new PrimitiveState
            {
                CullMode = CullMode.Back,
                FrontFace = FrontFace.Ccw,
                Topology = PrimitiveTopology.TriangleList,
            }
        };
        
        _renderPipeline = _engine.WGPU.DeviceCreateRenderPipeline(_engine.Device, pipelineDescriptor);
        
        Console.WriteLine("Created render pipeline");
        
        _engine.WGPU.ShaderModuleRelease(shaderModule);
    }

    public void Render()
    {
        _engine.WGPU.RenderPassEncoderSetPipeline(_engine.CurrentRenderPassEncoder, _renderPipeline);
        _engine.WGPU.RenderPassEncoderDraw(_engine.CurrentRenderPassEncoder, 3, 1, 0, 0);
    }
}