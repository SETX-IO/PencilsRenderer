using System.Collections.Generic;
using System.Numerics;

namespace Pencils.RendererApi;

public interface IShader
{
    string Name { get; }
    
    void Use();
    void SetVertexAttrib(List<VertexAttrib> vertexAttribs);

    void UploadConstantMat44(string constantName, Matrix4x4 mat, ShaderType visibleShader = ShaderType.Vertex);
    void UploadConstantFloat3(string constantName, Vector3 vec3, ShaderType visibleShader = ShaderType.Vertex);
}