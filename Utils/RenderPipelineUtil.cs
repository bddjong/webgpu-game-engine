using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace SourEngine.Utils;

public unsafe class RenderPipelineUtil
{
    public RenderPipeline* Create(Engine engine, ShaderModule* shaderModule, string vertexFnName = "main_vs", string fragmentFnName = "main_fs")
    {
        VertexAttribute* vertexAttributes = stackalloc VertexAttribute[2];
        vertexAttributes[0].Format = VertexFormat.Float32x3;
        vertexAttributes[0].ShaderLocation = 0;
        vertexAttributes[0].Offset = 0;
        vertexAttributes[1].Format = VertexFormat.Float32x4;
        vertexAttributes[1].ShaderLocation = 1;
        vertexAttributes[1].Offset = sizeof(float) * 3;
        
        VertexBufferLayout layout = new VertexBufferLayout
        {
            StepMode = VertexStepMode.Vertex,
            Attributes = vertexAttributes,
            AttributeCount = 2,
            ArrayStride = 7 * sizeof(float)
        };
        
        VertexState vertexState = new VertexState
        {
            Module = shaderModule,
            EntryPoint = (byte*)Marshal.StringToHGlobalAnsi(vertexFnName),
            Buffers = &layout,
            BufferCount = 1
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
        colorTargetState[0].Format = engine.PreferredTextureFormat;
        
        FragmentState fragmentState = new FragmentState
        {
            Module = shaderModule,
            EntryPoint = (byte*)Marshal.StringToHGlobalAnsi(fragmentFnName),
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
        
        
        RenderPipeline* renderPipeline = engine.WGPU.DeviceCreateRenderPipeline(engine.Device, pipelineDescriptor);
        Console.WriteLine("Created render pipeline");
        return renderPipeline;
    }

}