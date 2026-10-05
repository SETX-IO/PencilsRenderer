using System.Collections.Generic;
using System.Linq;
using Pencils.RendererApi;
using Vortice.Direct3D11;

namespace Pencils.Platform.DirectX11;

public class DxTexture2DArray(uint width, uint height) : DxTexture2D(width, height, ITexture2DArray.MaxCount), ITexture2DArray
{
    private int index;

    private readonly ITexture2D[] _textures = new ITexture2D[ITexture2DArray.MaxCount];
    private readonly Dictionary<ITexture, float> _sliceDict = new();

    public float AddTexture(ITexture2D texture)
    {
        if (texture.Height != Height && texture.Width != Width)
            if (_textures.Any<ITexture?>(texture.Equals))
                return _sliceDict[texture];
        
        _textures[index] =  texture;
        _sliceDict.Add(texture, index);
        CopyToArray(texture);
        
        return index++;
    }

    private void CopyToArray(ITexture2D texture)
    {
        var ctx = DxContext.Context;
        var source = new ID3D11Resource((nint)texture.Id);
        
        ctx.CopySubresourceRegion(resource, (uint)index, 0, 0, 0, source, 0);
    }
}