using System;
using Pencils.RendererApi;
using SharpGen.Runtime;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Pencils.Platform.DirectX11;

public class DxTexture2D : DxBaseTexture, ITexture2D
{
    private string? _path;

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