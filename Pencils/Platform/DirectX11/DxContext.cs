using System;
using System.Drawing;
using Pencils.RendererApi;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Pencils.Platform.DirectX11;

public class DxContext : IGraphicsContext
{
    private uint _backBufferCount;
    
    private ID3D11Device device;
    private ID3D11DeviceContext context;
    private IDXGISwapChain swapChain;
    private ID3D11RenderTargetView _rtv;
    private Vortice.Mathematics.Color _clearColor;
    private readonly nint hwnd;

    public static bool IsInitialized;
    public ID3D11Device Device => device;

    public static ID3D11DeviceContext Context
    {
        get => !IsInitialized ? throw new InvalidOperationException("DxContext.Context not initialized") : field;
        private set;
    }

    public IResourcesFactory ResourcesFactory { get; private set; }
    public GraphicsApi Api { get; }

    public DxContext(nint hwnd)
    {
        Api = GraphicsApi.DirectX11;
        
        this.hwnd = hwnd;
    }
    
    public void Init()
    {
        SwapChainDescription swDesc = new SwapChainDescription
        {
            BufferCount = 2,
            BufferDescription = new ModeDescription(800, 600),
            Flags = SwapChainFlags.AllowModeSwitch,
            BufferUsage =  Usage.RenderTargetOutput,
            OutputWindow = hwnd,
            SampleDescription = new SampleDescription(1, 0),
            Windowed = true,
            SwapEffect = SwapEffect.FlipDiscard
        };
        
        D3D11.D3D11CreateDeviceAndSwapChain(null, DriverType.Hardware, DeviceCreationFlags.VideoSupport, [FeatureLevel.Level_11_1], swDesc, out var sw, out ID3D11Device? device, out _, out var context);

        this.device = device;
        this.context = context;
        Context = context;
        swapChain = sw;

        using var surface = swapChain.GetBuffer<ID3D11Resource>(0);
        _rtv = device.CreateRenderTargetView(surface);
        
        ResourcesFactory = new DxResourcesFactory(this);
        IsInitialized = true;
    }

    public void SetBufferColor(Color color) => _clearColor = new Vortice.Mathematics.Color(color.R, color.G, color.B, color.A);
    
    public void ClearBuffer() => context.ClearRenderTargetView(_rtv, _clearColor);
    

    public void ReSizeBuffer(uint width, uint height)
    {
        if (width == 0 || height == 0)
            return;
        
        _rtv.Dispose();
        
        if (!swapChain.ResizeBuffers(0, width, height).Success)
            return;
        
        using var surface = swapChain.GetBuffer<ID3D11Resource>(0);
        _rtv = device.CreateRenderTargetView(surface);
    }

    public void SwapBuffers()
    {
        swapChain.Present(0, PresentFlags.None);
        
        context.OMSetRenderTargets(_rtv);
        context.ClearRenderTargetView(_rtv, _clearColor);
    }
}