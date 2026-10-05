using System;
using Pencils.RendererApi;
using SharpGen.Runtime;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Pencils.Platform.DirectX11;

public class DxTexture2D : ITexture2D
{
    private string? _path;
    protected readonly ID3D11Resource resource;
    protected readonly ID3D11ShaderResourceView textureSrv;
    
    public uint Width { get; }
    public uint Height { get; }
    public long Id { get; }


    protected DxTexture2D(string path)
    {
        _path = path;
        
        textureSrv = new ID3D11ShaderResourceView((nint)DxContext.ResourcesFactory.CreateTexture2D(path, out uint width, out uint height));
        resource = textureSrv.Resource;

        Id = resource.NativePointer.ToInt64();
        
        Width = width;
        Height = height;
    }
    
    protected DxTexture2D(uint width, uint height, uint arraySize = 1)
    {
        textureSrv = new ID3D11ShaderResourceView((nint)DxContext.ResourcesFactory.CreateTexture2D(width, height, arraySize: arraySize));
        resource = textureSrv.Resource;
        
        Id = resource.NativePointer.ToInt64();
        
        Width = width;
        Height = height;
    }
    
    public void Bind(uint slot)
    {
        /*
         * TODO:
         * 添加 Sampler 绑定
         * 使纹理自动生成多级纹理
        */
        
        // DxContext.Context.GenerateMips(_textureSrv);
        DxContext.Context.PSSetShaderResource(slot, textureSrv);
    }
    
    public void Unbind(uint slot) => DxContext.Context.PSSetShaderResource(slot, null!);
    public void SetData(ReadOnlySpan<byte> data) => SetData(data, new Viewport(0, 0, Width, Height));
    public unsafe void SetData(ReadOnlySpan<byte> data, Viewport viewport) => SetData((nint)data.GetPointerUnsafe(), (uint)data.Length / Height, viewport);
    public void SetData(IntPtr data, uint pitch) => SetData(data, pitch, new Viewport(0, 0, Width, Height));

    public void SetData(IntPtr data, uint pitch, Viewport viewport)
    {
        Box subresource = new Box((int)viewport.X, (int)viewport.Y, 0, (int)viewport.Width, (int)viewport.Height, 1);
        
        DxContext.Context.UpdateSubresource(resource, 0, subresource, data, pitch, 0);
    }

    public static ITexture2D Create(string path)
    {
        return new DxTexture2D(path);
    }

    public static ITexture2D Create(uint width, uint height, uint arraySize = 1)
    {
        return new DxTexture2D(width, height);
    }

    public bool Equals(ITexture? other) => Id.Equals(other?.Id);
}