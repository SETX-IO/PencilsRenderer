namespace Pencils.RendererApi;

public interface IShader
{
    void Use();
    void UnUse();
    void SetVertexAttrib(List<VertexAttrib> vertexAttribs);
}