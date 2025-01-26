namespace SourEngine
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();
            
            var unlitRenderPipeline = new Pipelines.UnlitRenderPipeline(engine);
            engine.OnInitialize += unlitRenderPipeline.Initialize;
            engine.OnRender += unlitRenderPipeline.Render;
            engine.OnDispose += unlitRenderPipeline.Dispose;
            
            engine.Initialize();
            engine.Dispose();
        }
    }
}
