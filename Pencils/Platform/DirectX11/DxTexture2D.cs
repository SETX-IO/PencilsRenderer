using System;
using Pencils.RendererApi;
using SharpGen.Runtime;
using Vortice.Direct3D11;

namespace Pencils.Platform.DirectX11;

public class DxTexture2D : ITexture2D
{
    private string _path;
    private ID3D11Texture2D _texture;
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

    public static ITexture2D Create(IGraphicsContext context, string path)
    {
        return new DxTexture2D(context.ResourcesFactory, path);
    }

    public static ITexture2D Create(IGraphicsContext graphicsContext)
    {
        throw new NotImplementedException();
    }
}