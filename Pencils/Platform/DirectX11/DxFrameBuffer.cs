using Pencils.RendererApi;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using Color = System.Drawing.Color;

namespace Pencils.Platform.DirectX11;

public class DxFrameBuffer : DxBaseTexture, IFrameBuffer
{
    protected ID3D11RenderTargetView rtv;

    public bool IsClearColor { get; }
    public Color ClearColor { get; set; }
    
    public DxFrameBuffer(ITexture tetxure, bool clearColor = true)
    {
        IsClearColor = clearColor;
        ClearColor = Color.Black;
    }

    public DxFrameBuffer(uint width, uint height, bool clearColor = true)
    {
        textureSrv = new ID3D11ShaderResourceView((nint)DxContext.ResourcesFactory.CreateRenderTargetTexture(width, height, out long rtvPtr));
        rtv = new ID3D11RenderTargetView((nint)rtvPtr);
        resource = textureSrv.Resource;
        
        Width = width;
        Height = height;
        
        IsClearColor = clearColor;
        ClearColor = Color.Black;
    }
    
    public void Use()
    {
        DxContext.Context.OMSetRenderTargets(rtv);
    }

    public void Clear()
    {
        Color4 color = new  Color4(ClearColor.R / 255f, ClearColor.G / 255f, ClearColor.B / 255f);
        
        DxContext.Context.ClearRenderTargetView(rtv, color);
    }

    public void ReSize(uint width, uint height)
    {
        throw new System.NotImplementedException();
    }
}