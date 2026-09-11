using System.Numerics;

namespace Pencils.RendererApi;

public interface IShader
{
    void Use();
    void UnUse();
    void SetVertexAttrib(List<VertexAttrib> vertexAttribs);

    void UploadConstantMat44(string constantName, Matrix4x4 mat, ShaderType visibleShader = ShaderType.Vertex);
    void UploadConstantFloat3(string constantName, Vector3 vec3, ShaderType visibleShader = ShaderType.Vertex);
}