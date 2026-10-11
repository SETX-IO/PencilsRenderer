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
    void SetData(ReadOnlySpan<byte> data, uint subresourceIndex = 0);
    void SetData(nint data, uint pitch, uint subresourceIndex = 0);
    void SetData(nint data, uint pitch, Viewport viewport, uint subresourceIndex = 0);
    void SetData(ReadOnlySpan<byte> data, Viewport viewport, uint subresourceIndex = 0);
}