using System;

namespace Pencils.RendererApi;

public interface ITexture2D : ITexture, IEquatable<ITexture>
{
    static abstract ITexture2D Create(string path);
    static abstract ITexture2D Create(uint width, uint height, uint arraySize = 1);
}