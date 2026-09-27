using System;

namespace Pencils.RendererApi;

public interface ITexture
{
    uint Width { get; }
    uint Height { get; }
    
    long Id { get; }

    void Bind(uint slot = 0);
    void SetData(ReadOnlySpan<byte> data);
}