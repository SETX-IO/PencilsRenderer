using System;

namespace Pencils.RendererApi;

public interface IBuffer : IDisposable
{
    uint Count { get; }
    void Bind();
    void Unbind();
    
    nint Map();
    void CloseMap();
    
    void SetData<T>(Span<T> data) where T : struct;
}