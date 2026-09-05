namespace Pencils.RendererApi;

public interface IVertexBuffer : IBuffer
{
    VertexAttribType[] AttribType { get; }
    void SetVertexAttribs(params VertexAttribType[] position3);
}