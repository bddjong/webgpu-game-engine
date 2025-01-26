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
            engine.OnRender += () => { unlitRenderPipeline.Render(vertexBuffer, indexBuffer); };

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