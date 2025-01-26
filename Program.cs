using Silk.NET.Maths;

namespace SourEngine
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();

            var unlitRenderPipeline = new Pipelines.UnlitRenderPipeline(engine);
            var vertexBuffer = new Buffers.VertexBuffer(engine);
            var indexBuffer = new Buffers.IndexBuffer(engine);

            float scale = 1f;
            int scaleDirection = 1;

            engine.OnInitialize += () =>
            {
                unlitRenderPipeline.Initialize();
                vertexBuffer.Initialize([
                    // First triangle
                    -0.5f, -0.5f, 0f, 1, 0, 0, 1,
                    0.5f, -0.5f, 0f, 0, 1, 0, 1,
                    0.5f, 0.5f, 0f, 0, 0, 1, 1,
                    -0.5f, 0.5f, 0f, 1, 0, 1, 1,
                ], 6);
                indexBuffer.Initialize([
                    0, 1, 2,
                    2, 3, 0,
                ]);
            };
            engine.OnRender += () =>
            {
                if (scale > 2)
                {
                    scaleDirection = -1;
                } else if (scale < 0.5)
                {
                    scaleDirection = 1;
                }
                
                scale += 0.001f * scaleDirection;
                
                unlitRenderPipeline.Transform = Matrix4X4.CreateScale(scale, scale, 1.0f);
                unlitRenderPipeline.Render(vertexBuffer, indexBuffer);
            };

            engine.OnDispose += () =>
            {
                unlitRenderPipeline.Dispose();
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
            };

            engine.Initialize();
            engine.Dispose();
        }
    }
}