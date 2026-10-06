using System.Collections.Generic;
using Pencils.RendererApi;

namespace Pencils.Platform.DirectX11;

public class DxMesh : IMesh
{
    private uint _vertexCount;
    public IIndexBuffer? IndexBuffer { get; private set; }
    public List<IVertexBuffer> VertexBuffers { get; }
    public List<VertexAttrib> VertexAttribs { get; }

    public uint VertexCount => IndexBuffer?.Count ?? _vertexCount;

    public DxMesh()
    {
        VertexBuffers = [];
        VertexAttribs = [];
    }

    public void AddVertexBuffer(IVertexBuffer buffer)
    {
        foreach (var attribType in buffer.AttribType)
            VertexAttribs.Add(new VertexAttrib(attribType, (uint)VertexBuffers.Count));

        _vertexCount += buffer.Count;
        VertexBuffers.Add(buffer);
    }

    public void SetVertexBuffer(int index, IVertexBuffer buffer)
    {
        if (index == VertexBuffers.Count)
        {
            AddVertexBuffer(buffer);
            return;
        }
        
        VertexBuffers[index] = buffer;
    }

    public void SetIndexBuffer(IIndexBuffer buffer) => IndexBuffer = buffer;
    
    public void Bind()
    {
        IndexBuffer?.Bind();

        for (int i = 0; i < VertexBuffers.Count; i++)
        {
            IVertexBuffer vBuffer = VertexBuffers[i];
            vBuffer.Bind((uint)i);
        }
    }

    public void Unbind()
    {
        IndexBuffer?.Unbind();
        
        for (int i = 0; i < VertexBuffers.Count; i++)
        {
            IVertexBuffer vBuffer = VertexBuffers[i];
            vBuffer.Unbind((uint)i);
        }
    }

    public static IMesh Create()
    {
        return new DxMesh();
    }
}