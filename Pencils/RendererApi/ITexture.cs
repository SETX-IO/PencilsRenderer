using System;
using Vortice.Mathematics;

namespace Pencils.RendererApi;

public interface ITexture
{
    uint Width { get; }
    uint Height { get; }
    
    long Id { get; }

    void Bind(uint slot = 0);
    void Unbind(uint slot = 0);
    void SetData(ReadOnlySpan<byte> data);
    void SetData(nint data, uint pitch);
    void SetData(nint data, uint pitch, Viewport viewport);
    void SetData(ReadOnlySpan<byte> data, Viewport viewport);
}