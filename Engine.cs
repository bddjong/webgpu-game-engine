using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace SourEngine;

public class Engine
{
    public void Initialize()
    {
        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.Title = "Hello, World!";
        windowOptions.Size = new Vector2D<int>(800, 600);

        IWindow window = Window.Create(WindowOptions.Default);

        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;

        window.Run();
    }

    public void OnLoad()
    {
    }

    public void OnUpdate(double deltaTime)
    {
    }

    public void OnRender(double deltaTime)
    {
    }
}