using System;

namespace Pencils.RendererApi;

public interface IVertexBuffer : IBuffer
{
    VertexAttribType[] AttribType { get; }
    void SetVertexAttribs(params VertexAttribType[] position3);

    nint Map<T>() where T : struct;
    void CloseMap();
    
    void SetData<T>(Span<T> data) where T : struct;
}