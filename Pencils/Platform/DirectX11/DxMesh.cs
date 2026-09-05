using Pencils.RendererApi;

namespace Pencils.Platform.DirectX11;

public class DxMesh : IMesh
{
    public IIndexBuffer IndexBuffer { get; private set; }
    public List<IVertexBuffer> VertexBuffers { get; }
    public List<VertexAttrib> VertexAttribs { get; }

    public DxMesh()
    {
        VertexBuffers = [];
        VertexAttribs = [];
    }

    public void AddVertexBuffer(IVertexBuffer buffer)
    {
        foreach (var attribType in buffer.AttribType)
        {
            VertexAttribs.Add(new VertexAttrib(attribType, (uint)VertexBuffers.Count));
        }
        VertexBuffers.Add(buffer);
    } 

    public void SetIndexBuffer(IIndexBuffer buffer) => IndexBuffer = buffer;
    
    public void Bind()
    {
        IndexBuffer.Bind();
        
        foreach (var buffer in VertexBuffers)
        {
            buffer.Bind();
        }
    }

    public void Unbind()
    {
        IndexBuffer.Unbind();
        
        foreach (var buffer in VertexBuffers)
            buffer.Unbind();
    }

    public static IMesh Create()
    {
        return new DxMesh();
    }
}