using Vortice.DXGI;

namespace Pencils.Platform.DirectX11;

public struct DxTextureCreateInfo
{
    public uint width;
    public uint height;

    public bool isRttTexture;
    
    public Format format;
    public uint arraySize;
    public uint mipLevels;
}