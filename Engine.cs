using System.Runtime.InteropServices;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;
using Monitor = Silk.NET.Windowing.Monitor;

namespace SourEngine;

public unsafe class Engine : IDisposable
{
    private IWindow _window;
    private Instance* _instance;
    private Surface* _surface;
    private Adapter* _adapter;

    private CommandEncoder* _currentCommandEncoder;
    private SurfaceTexture _surfaceTexture;
    private TextureView* _surfaceTextureView;

    public Action OnInitialize;
    public Action OnRender;
    public Action OnDispose;

    public WebGPU WGPU { get; private set; }
    public Device* Device { get; private set; }
    public RenderPassEncoder* CurrentRenderPassEncoder { get; private set; }
    public Queue* Queue { get; private set; }
    public TextureFormat PreferredTextureFormat => TextureFormat.Bgra8Unorm;

    public void Initialize()
    {
        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.Title = "Hello, World!";
        windowOptions.Size = new Vector2D<int>(800, 600);
        windowOptions.API = GraphicsAPI.None;

        _window = Window.Create(windowOptions);

        _window.Initialize();
        _window.Monitor = Monitor.GetMonitors(null).Last();
        _window.Position = _window.Monitor.Bounds.Center - _window.Size / 2;

        // API setup
        CreateApi();
        CreateInstance();
        CreateSurface();
        CreateAdapter();
        CreateDevice();

        LogAdapterProperties();

        // Configuration
        ConfigureSurface();
        ConfigureDebugCallback();

        // Window hooks
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRenderWindow;
        _window.Move += OnMove;

        // Queue
        Queue = WGPU.DeviceGetQueue(Device);

        OnInitialize?.Invoke();

        _window.Run();
    }

    private void LogAdapterProperties()
    {
        AdapterProperties* properties = stackalloc AdapterProperties[1];

        WGPU.AdapterGetProperties(_adapter, properties);

        Console.WriteLine("Adapter properties:");
        Console.WriteLine(" - adapter: " + properties[0].AdapterType);
        Console.WriteLine(" - name: " + Marshal.PtrToStringAnsi((IntPtr)properties[0].Name));
        Console.WriteLine(" - backend: " + properties[0].BackendType);
    }

    private void CreateApi()
    {
        WGPU = WebGPU.GetApi();
        Console.WriteLine("Created WebGPU API" + WGPU);
    }

    private void CreateInstance()
    {
        InstanceDescriptor descriptor = new InstanceDescriptor();
        _instance = WGPU.CreateInstance(descriptor);
        Console.WriteLine("Created WebGPU Instance: " + _instance->ToString());
    }

    private void CreateSurface()
    {
        _surface = _window.CreateWebGPUSurface(WGPU, _instance);
        Console.WriteLine("Created WebGPU Surface: " + _surface->ToString());
    }

    private void CreateAdapter()
    {
        RequestAdapterOptions options = new RequestAdapterOptions
        {
            CompatibleSurface = _surface,
            PowerPreference = PowerPreference.HighPerformance
        };

        PfnRequestAdapterCallback callback = PfnRequestAdapterCallback.From(
            (status, wgpuAdapter, messagePtr, userDataPtr) =>
            {
                if (status == RequestAdapterStatus.Success)
                {
                    _adapter = wgpuAdapter;
                    Console.WriteLine("Created WebGPU Adapter: " + _adapter->ToString());
                }
                else
                {
                    string errorMessage = Marshal.PtrToStringAnsi((IntPtr)messagePtr) ?? string.Empty;
                    Console.Error.WriteLine("Failed to create WebGPU Adapter: " + errorMessage);
                }
            });

        WGPU.InstanceRequestAdapter(_instance, options, callback, null);
    }

    private void CreateDevice()
    {
        PfnRequestDeviceCallback callback = PfnRequestDeviceCallback.From(
            (status, wgpuDevice, messagePtr, userdataPtr) =>
            {
                if (status == RequestDeviceStatus.Success)
                {
                    Device = wgpuDevice;
                    Console.WriteLine("Created WebGPU Device: " + Device->ToString());
                }
                else
                {
                    string errorMessage = Marshal.PtrToStringAnsi((IntPtr)messagePtr) ?? string.Empty;
                    Console.Error.WriteLine("Failed to create WebGPU Device: " + errorMessage);
                }
            });

        DeviceDescriptor descriptor = new DeviceDescriptor();
        WGPU.AdapterRequestDevice(_adapter, descriptor, callback, null);
    }

    private void ConfigureSurface()
    {
        SurfaceConfiguration configuration = new SurfaceConfiguration
        {
            Device = Device,
            Width = (uint)_window.Size.X,
            Height = (uint)_window.Size.Y,
            Format = PreferredTextureFormat,
            PresentMode = PresentMode.Fifo,
            Usage = TextureUsage.RenderAttachment
        };

        WGPU.SurfaceConfigure(_surface, configuration);
    }

    private void ConfigureDebugCallback()
    {
        PfnErrorCallback errorCallback = PfnErrorCallback.From((type, messagePtr, userdataPtr) =>
        {
            string errorMessage = Marshal.PtrToStringAnsi((IntPtr)messagePtr) ?? string.Empty;
            Console.Error.WriteLine("WebGPU Error: " + errorMessage);
        });

        WGPU.DeviceSetUncapturedErrorCallback(Device, errorCallback, null);
    }

    public void OnLoad()
    {
    }

    public void OnUpdate(double deltaTime)
    {
    }

    public void OnMove(Vector2D<int> vector2D)
    {
        
    }


    public void OnRenderWindow(double deltaTime)
    {
        BeforeRender();

        OnRender?.Invoke();

        AfterRender();
    }

    private void BeforeRender()
    {
        // Queue
        Queue = WGPU.DeviceGetQueue(Device);

        // Command encoder
        _currentCommandEncoder = WGPU.DeviceCreateCommandEncoder(Device, null);

        // Surface texture prep
        WGPU.SurfaceGetCurrentTexture(_surface, ref _surfaceTexture);
        _surfaceTextureView = WGPU.TextureCreateView(_surfaceTexture.Texture, null);

        // Render pass encoder
        RenderPassColorAttachment* colorAttachments = stackalloc RenderPassColorAttachment[1];
        colorAttachments[0].View = _surfaceTextureView;
        colorAttachments[0].LoadOp = LoadOp.Clear;
        colorAttachments[0].ClearValue = new Color(0.1, 0.9, 0.9, 1.0);
        colorAttachments[0].StoreOp = StoreOp.Store;

        RenderPassDescriptor descriptor = new RenderPassDescriptor
        {
            ColorAttachments = colorAttachments,
            ColorAttachmentCount = 1
        };

        CurrentRenderPassEncoder = WGPU.CommandEncoderBeginRenderPass(_currentCommandEncoder, descriptor);
    }

    private void AfterRender()
    {
        WGPU.RenderPassEncoderEnd(CurrentRenderPassEncoder);

        CommandBuffer* commandBuffer = WGPU.CommandEncoderFinish(_currentCommandEncoder, null);

        WGPU.QueueSubmit(Queue, 1, &commandBuffer);

        WGPU.SurfacePresent(_surface);

        // Dispose of resources
        WGPU.TextureRelease(_surfaceTexture.Texture);
        WGPU.TextureViewRelease(_surfaceTextureView);
        WGPU.CommandBufferRelease(commandBuffer);
        WGPU.CommandEncoderRelease(_currentCommandEncoder);
        WGPU.RenderPassEncoderRelease(CurrentRenderPassEncoder);
    }

    public void Dispose()
    {
        OnDispose?.Invoke();

        Console.WriteLine("Disposing WebGPU resources");
        WGPU.DeviceDestroy(Device);
        Console.WriteLine("Destroyed WebGPU Device");
        WGPU.SurfaceRelease(_surface);
        Console.WriteLine("Released WebGPU Surface");
        WGPU.AdapterRelease(_adapter);
        Console.WriteLine("Released WebGPU Adapter");
        WGPU.InstanceRelease(_instance);
        Console.WriteLine("Released WebGPU Instance");
        WGPU.Dispose();
        Console.WriteLine("Completed disposing WebGPU resources");
    }
}