using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;

namespace SourEngine;

public unsafe class Engine : IDisposable
{
    private readonly IWindow window;
    private WebGPU wgpu;
    private Instance* instance;
    private Surface* surface;
    private Adapter* adapter;
    private Device* device;

    public Engine()
    {
        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.Title = "Hello, World!";
        windowOptions.Size = new Vector2D<int>(800, 600);

        window = Window.Create(WindowOptions.Default);

        window.Initialize();

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
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;

        window.Run();
    }

    private void CreateApi()
    {
        wgpu = WebGPU.GetApi();
        Console.WriteLine("Created WebGPU API" + wgpu);
    }

    private void CreateInstance()
    {
        InstanceDescriptor descriptor = new InstanceDescriptor();
        instance = wgpu.CreateInstance(descriptor);
        Console.WriteLine("Created WebGPU Instance: " + instance->ToString());
    }

    private void CreateSurface()
    {
        surface = window.CreateWebGPUSurface(wgpu, instance);
        Console.WriteLine("Created WebGPU Surface: " + surface->ToString());
    }

    private void CreateAdapter()
    {
        RequestAdapterOptions options = new RequestAdapterOptions
        {
            CompatibleSurface = surface,
            BackendType = BackendType.Metal,
            PowerPreference = PowerPreference.HighPerformance
        };

        PfnRequestAdapterCallback callback = PfnRequestAdapterCallback.From(
            (status, wgpuAdapter, messagePtr, userDataPtr) =>
            {
                if (status == RequestAdapterStatus.Success)
                {
                    adapter = wgpuAdapter;
                    Console.WriteLine("Created WebGPU Adapter: " + adapter->ToString());
                }
                else
                {
                    string errorMessage = Marshal.PtrToStringAnsi((IntPtr)messagePtr) ?? string.Empty;
                    Console.Error.WriteLine("Failed to create WebGPU Adapter: " + errorMessage);
                }
            });

        wgpu.InstanceRequestAdapter(instance, options, callback, null);
    }

    private void CreateDevice()
    {
        PfnRequestDeviceCallback callback = PfnRequestDeviceCallback.From((status, wgpuDevice, messagePtr, userdataPtr) =>
        {
            if (status == RequestDeviceStatus.Success)
            {
                device = wgpuDevice;
                Console.WriteLine("Created WebGPU Device: " + device->ToString());
            }
            else
            {
                string errorMessage = Marshal.PtrToStringAnsi((IntPtr)messagePtr) ?? string.Empty;
                Console.Error.WriteLine("Failed to create WebGPU Device: " + errorMessage);
            }
        });

        DeviceDescriptor descriptor = new DeviceDescriptor();
        wgpu.AdapterRequestDevice(adapter, descriptor, callback, null);
    }

    private void ConfigureSurface()
    {
        SurfaceConfiguration configuration = new SurfaceConfiguration
        {
            Device = device,
            Width = (uint)window.Size.X,
            Height = (uint)window.Size.Y,
            Format = TextureFormat.Bgra8Unorm,
            PresentMode = PresentMode.Fifo
        };
        
        wgpu.SurfaceConfigure(surface, configuration);
    }

    private void ConfigureDebugCallback()
    {
        PfnErrorCallback errorCallback = PfnErrorCallback.From((type, messagePtr, userdataPtr) =>
        {
            string errorMessage = Marshal.PtrToStringAnsi((IntPtr)messagePtr) ?? string.Empty;
            Console.Error.WriteLine("WebGPU Error: " + errorMessage);
        });
        
        wgpu.DeviceSetUncapturedErrorCallback(device, errorCallback, null);
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

    public void Dispose()
    {
        Console.WriteLine("Disposing WebGPU resources");
        wgpu.DeviceDestroy(device);
        Console.WriteLine("Destroyed WebGPU Device");
        wgpu.SurfaceRelease(surface);
        Console.WriteLine("Released WebGPU Surface");
        wgpu.AdapterRelease(adapter);
        Console.WriteLine("Released WebGPU Adapter");
        wgpu.InstanceRelease(instance);
        Console.WriteLine("Released WebGPU Instance");
        wgpu.Dispose();
        Console.WriteLine("Completed disposing WebGPU resources");
    }
}