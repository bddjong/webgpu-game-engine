using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;

namespace SourEngine;

public unsafe class Engine : IDisposable
{
    private readonly IWindow _window;
    private WebGPU _wgpu;
    private Instance* _instance;
    private Surface* _surface;
    private Adapter* _adapter;
    private Device* _device;

    private Queue* _queue;
    private CommandEncoder* _currentCommandEncoder;
    private RenderPassEncoder* _currentRenderPassEncoder;
    private SurfaceTexture _surfaceTexture;
    private TextureView* _surfaceTextureView;

    public Engine()
    {
        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.Title = "Hello, World!";
        windowOptions.Size = new Vector2D<int>(800, 600);

        _window = Window.Create(WindowOptions.Default);

        _window.Initialize();

        // API setup
        CreateApi();
        CreateInstance();
        CreateSurface();
        CreateAdapter();
        CreateDevice();
        
        // Configuration
        ConfigureSurface();
        ConfigureDebugCallback();

        // Window hooks
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;

        _window.Run();
    }

    private void CreateApi()
    {
        _wgpu = WebGPU.GetApi();
        Console.WriteLine("Created WebGPU API" + _wgpu);
    }

    private void CreateInstance()
    {
        InstanceDescriptor descriptor = new InstanceDescriptor();
        _instance = _wgpu.CreateInstance(descriptor);
        Console.WriteLine("Created WebGPU Instance: " + _instance->ToString());
    }

    private void CreateSurface()
    {
        _surface = _window.CreateWebGPUSurface(_wgpu, _instance);
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

        _wgpu.InstanceRequestAdapter(_instance, options, callback, null);
    }

    private void CreateDevice()
    {
        PfnRequestDeviceCallback callback = PfnRequestDeviceCallback.From((status, wgpuDevice, messagePtr, userdataPtr) =>
        {
            if (status == RequestDeviceStatus.Success)
            {
                _device = wgpuDevice;
                Console.WriteLine("Created WebGPU Device: " + _device->ToString());
            }
            else
            {
                string errorMessage = Marshal.PtrToStringAnsi((IntPtr)messagePtr) ?? string.Empty;
                Console.Error.WriteLine("Failed to create WebGPU Device: " + errorMessage);
            }
        });

        DeviceDescriptor descriptor = new DeviceDescriptor();
        _wgpu.AdapterRequestDevice(_adapter, descriptor, callback, null);
    }

    private void ConfigureSurface()
    {
        SurfaceConfiguration configuration = new SurfaceConfiguration
        {
            Device = _device,
            Width = (uint)_window.Size.X,
            Height = (uint)_window.Size.Y,
            Format = TextureFormat.Bgra8Unorm,
            PresentMode = PresentMode.Fifo,
            Usage = TextureUsage.RenderAttachment
        };
        
        _wgpu.SurfaceConfigure(_surface, configuration);
    }

    private void ConfigureDebugCallback()
    {
        PfnErrorCallback errorCallback = PfnErrorCallback.From((type, messagePtr, userdataPtr) =>
        {
            string errorMessage = Marshal.PtrToStringAnsi((IntPtr)messagePtr) ?? string.Empty;
            Console.Error.WriteLine("WebGPU Error: " + errorMessage);
        });
        
        _wgpu.DeviceSetUncapturedErrorCallback(_device, errorCallback, null);
    }

    public void OnLoad()
    {
    }

    public void OnUpdate(double deltaTime)
    {
    }
    

    public void OnRender(double deltaTime)
    {
        BeforeRender();
        AfterRender();
    }
    
    private void BeforeRender()
    { 
        // Queue
        _queue = _wgpu.DeviceGetQueue(_device);
        
        // Command encoder
        _currentCommandEncoder = _wgpu.DeviceCreateCommandEncoder(_device, null);

        // Surface texture prep
        _wgpu.SurfaceGetCurrentTexture(_surface, ref _surfaceTexture);
        _surfaceTextureView = _wgpu.TextureCreateView(_surfaceTexture.Texture, null);
        
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
        
        _currentRenderPassEncoder = _wgpu.CommandEncoderBeginRenderPass(_currentCommandEncoder, descriptor);
    }

    private void AfterRender()
    {
        _wgpu.RenderPassEncoderEnd(_currentRenderPassEncoder);
        
        CommandBuffer* commandBuffer = _wgpu.CommandEncoderFinish(_currentCommandEncoder, null);
        
        _wgpu.QueueSubmit(_queue, 1, &commandBuffer);
        
        _wgpu.SurfacePresent(_surface);
        
        // Dispose of resources
        _wgpu.TextureRelease(_surfaceTexture.Texture);
        _wgpu.TextureViewRelease(_surfaceTextureView);
        _wgpu.CommandBufferRelease(commandBuffer);
        _wgpu.CommandEncoderRelease(_currentCommandEncoder);
        _wgpu.RenderPassEncoderRelease(_currentRenderPassEncoder);
    }

    public void Dispose()
    {
        Console.WriteLine("Disposing WebGPU resources");
        _wgpu.DeviceDestroy(_device);
        Console.WriteLine("Destroyed WebGPU Device");
        _wgpu.SurfaceRelease(_surface);
        Console.WriteLine("Released WebGPU Surface");
        _wgpu.AdapterRelease(_adapter);
        Console.WriteLine("Released WebGPU Adapter");
        _wgpu.InstanceRelease(_instance);
        Console.WriteLine("Released WebGPU Instance");
        _wgpu.Dispose();
        Console.WriteLine("Completed disposing WebGPU resources");
    }
}