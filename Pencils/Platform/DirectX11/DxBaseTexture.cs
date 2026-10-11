using System;
using Pencils.RendererApi;
using SharpGen.Runtime;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Pencils.Platform.DirectX11;

public unsafe class DxBaseTexture : ITexture
{
    protected ID3D11Resource resource;
    protected ID3D11ShaderResourceView textureSrv;
    
    public uint Width { get; protected set; }
    public uint Height { get; protected set; }
    public long Id { get; protected set; }
    
    public void Bind(uint slot = 0)
    {
        /*
         * TODO:
         * 添加 Sampler 绑定
         * 使纹理自动生成多级纹理
         */
        
        // DxContext.Context.GenerateMips(_textureSrv);
        DxContext.Context.PSSetShaderResource(slot, textureSrv);
    }

    public void Unbind(uint slot = 0) => DxContext.Context.PSSetShaderResource(slot, null!);

    public void SetData(ReadOnlySpan<byte> data, uint subresourceIndex) => SetData(data, new Viewport(0, 0, Width, Height), subresourceIndex);
    public void SetData(ReadOnlySpan<byte> data, Viewport viewport, uint subresourceIndex) => SetData((nint)data.GetPointerUnsafe(), (uint)data.Length / Height, viewport, subresourceIndex);
    public void SetData(IntPtr data, uint pitch, uint subresourceIndex) => SetData(data, pitch, new Viewport(0, 0, Width, Height), subresourceIndex);

    public void SetData(IntPtr data, uint pitch, Viewport viewport, uint subresourceIndex)
    {
        Box subresource = new Box((int)viewport.X, (int)viewport.Y, 0, (int)viewport.Width, (int)viewport.Height, 1);
        
        DxContext.Context.UpdateSubresource(resource, subresourceIndex, subresource, data, pitch, 0);
    }
}