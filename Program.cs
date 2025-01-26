namespace SourEngine
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();

            var unlitRenderPipeline = new Pipelines.UnlitRenderPipeline(engine);
            var vertexBuffer = new Buffers.VertexBuffer(engine);

            engine.OnInitialize += () =>
            {
                unlitRenderPipeline.Initialize();
                vertexBuffer.Initialize([
                    -0.5f, -0.5f, 0f, 1, 0, 0, 1,
                    0.5f, -0.5f, 0f, 0, 1, 0, 1,
                    0.0f, 0.5f, 0f, 0, 0, 1, 1
                ]);
            };
            engine.OnRender += () =>
            {
                unlitRenderPipeline.Render(vertexBuffer);
            };

            engine.OnDispose += unlitRenderPipeline.Dispose;
            
            engine.Initialize();
            engine.Dispose();
        }
    }
}
