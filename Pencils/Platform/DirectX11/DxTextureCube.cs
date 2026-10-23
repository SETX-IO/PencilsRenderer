using System.Collections.Generic;
using System.IO;
using Pencils.Platform.DirectX11.Utility;
using Pencils.RendererApi;
using Vortice.Direct3D11;

namespace Pencils.Platform.DirectX11;

public class DxTextureCube : DxBaseTexture, ITextureCube
{
    private Dictionary<TextureCubeDirection, ITexture> _faces = new();
    
    protected DxTextureCube(string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException(path);
        
        string[] skyboxDir = Directory.GetFiles(path);
        byte[][] textureData = new byte[skyboxDir.Length][];

        uint width = 0;
        uint height = 0;
        
        for (int i = 0; i < textureData.Length; i++)
        {
            ref byte[] texture = ref textureData[i];
            string texturePath = skyboxDir[i];

            texture = Texture2DLoad.Load(texturePath, out width, out height, out _).ToArray();
        }

        InitTextureCube(width, height);

        for (int i = 0; i < textureData.Length; i++)
        {
            SetData(textureData[i], (uint)i);
        }
    }

    protected DxTextureCube(uint width, uint height) => InitTextureCube(width, height);

    public void AddTexture(ITexture texture, TextureCubeDirection direction)
    {
        if (texture.Height != Height && texture.Width != Width) 
            return;
    }

    private void InitTextureCube(uint width, uint height)
    {
        textureSrv = new ID3D11ShaderResourceView((nint)DxContext.ResourcesFactory.CreateTextureCube(width, height));
        resource = textureSrv.Resource;
        Id = resource.NativePointer.ToInt64();
        
        Width = width;
        Height = height;
    }
    
    public static ITextureCube Create(string path)
    {
        return new DxTextureCube(path);
    }
}