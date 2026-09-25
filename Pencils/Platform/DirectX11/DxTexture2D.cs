using System;
using Pencils.RendererApi;
using SharpGen.Runtime;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Pencils.Platform.DirectX11;

public class DxTexture2D : ITexture2D
{
    private string? _path;
    private readonly ID3D11Texture2D _texture;
    private readonly ID3D11ShaderResourceView _textureSrv;
    
    public uint Width { get; }
    public uint Height { get; }
    

    private DxTexture2D(IResourcesFactory factory, string path)
    {
        _path = path;
        
        _textureSrv = new ID3D11ShaderResourceView((nint)factory.CreateTexture2D(path, out uint width, out uint height));
        _texture = _textureSrv.Resource.As<ID3D11Texture2D>();
        
        Width = width;
        Height = height;
    }
    
    private DxTexture2D(IResourcesFactory factory, uint width, uint height)
    {
        _textureSrv = new ID3D11ShaderResourceView((nint)factory.CreateTexture2D(width, height));
        _texture = _textureSrv.Resource.As<ID3D11Texture2D>();
        
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
        DxContext.Context.PSSetShaderResource(slot, _textureSrv);
    }

    public void Unbind(uint slot)
    {
        /*
         * TODO:
         * 添加 Sampler 绑定
         * 使纹理自动生成多级纹理
         */
        
        // DxContext.Context.GenerateMips(_textureSrv);
        DxContext.Context.PSSetShaderResource(slot, null!);
    }

    public void SetData(ReadOnlySpan<byte> data) => SetData(data, new Viewport(0, 0, Width, Height));
    
    
    public unsafe void SetData(ReadOnlySpan<byte> data, Viewport viewport)
    {
        Box subresource = new Box((int)viewport.X, (int)viewport.Y, 0, (int)viewport.Width, (int)viewport.Height, 1);
        DxContext.Context.UpdateSubresource(_texture, 0, subresource, (nint)data.GetPointerUnsafe(), (uint)(data.Length / viewport.Height), 0);
    }

    public static ITexture2D Create(IResourcesFactory factory, string path)
    {
        return new DxTexture2D(factory, path);
    }

    public static ITexture2D Create(IResourcesFactory factory, uint width, uint height)
    {
        return new DxTexture2D(factory, width, height);
    }
}