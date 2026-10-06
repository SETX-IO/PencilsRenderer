using System;

namespace Pencils.RendererApi;

public interface IBuffer : IDisposable
{
    uint Count { get; }
    void Bind(uint slot = 0);
    void Unbind(uint slot = 0);
    
    nint Map();
    void CloseMap();
    
    void SetData<T>(Span<T> data) where T : struct;
}