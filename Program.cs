namespace SourEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();
            
            var unlitRenderPipeline = new Pipelines.UnlitRenderPipeline(engine);
            engine.OnInitialize += unlitRenderPipeline.Initialize;
            
            engine.Initialize();
            engine.Dispose();
        }
    }
}
