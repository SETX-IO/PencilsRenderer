namespace Pencils.RendererApi;

public interface IMesh
{
    IIndexBuffer? IndexBuffer { get; }
    List<IVertexBuffer> VertexBuffers { get; }
    List<VertexAttrib> VertexAttribs { get; }
    
    uint VertexCount { get; }
    
    void AddVertexBuffer(IVertexBuffer buffer);
    void SetIndexBuffer(IIndexBuffer buffer);
    
    void Bind();
    void Unbind();
}