using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace SourEngine.Utils;

public unsafe class RenderPipelineUtil
{
    public RenderPipeline* Create(Engine engine, 
        ShaderModule* shaderModule, 
        PipelineLayout* pipelineLayout,
        string vertexFnName = "main_vs", 
        string fragmentFnName = "main_fs")
    {
        var vertexFnNamePtr = Marshal.StringToHGlobalAnsi(vertexFnName);
        var fragmentFnNamePtr = Marshal.StringToHGlobalAnsi(fragmentFnName);
        
        VertexAttribute* vertexAttributes = stackalloc VertexAttribute[3];
        // Vertex pos
        vertexAttributes[0].Format = VertexFormat.Float32x3;
        vertexAttributes[0].ShaderLocation = 0;
        vertexAttributes[0].Offset = 0;
        //Vertex colors
        vertexAttributes[1].Format = VertexFormat.Float32x4;
        vertexAttributes[1].ShaderLocation = 1;
        vertexAttributes[1].Offset = sizeof(float) * 3;
        //Vertex UV
        vertexAttributes[2].Format = VertexFormat.Float32x2;
        vertexAttributes[2].ShaderLocation = 2;
        vertexAttributes[2].Offset = sizeof(float) * (3 + 4);
        
        VertexBufferLayout layout = new VertexBufferLayout
        {
            StepMode = VertexStepMode.Vertex,
            Attributes = vertexAttributes,
            AttributeCount = 3,
            ArrayStride = 9 * sizeof(float) // memory size of positions + colors + uvs (all floats)
        };
        
        VertexState vertexState = new VertexState
        {
            Module = shaderModule,
            EntryPoint = (byte*)vertexFnNamePtr,
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
            EntryPoint = (byte*)fragmentFnNamePtr,
            Targets = colorTargetState,
            TargetCount = 1
        };

        RenderPipelineDescriptor pipelineDescriptor = new RenderPipelineDescriptor
        {
            Layout = pipelineLayout,
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
        
        Marshal.FreeHGlobal(vertexFnNamePtr);
        Marshal.FreeHGlobal(fragmentFnNamePtr);
        
        return renderPipeline;
    }

}