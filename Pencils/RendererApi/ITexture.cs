using System;

namespace Pencils.RendererApi;

public interface ITexture
{
    uint Width { get; }
    uint Height { get; }

    void Bind(uint slot);
    void Unbind(uint slot);

    void SetData(ReadOnlySpan<byte> data);
}