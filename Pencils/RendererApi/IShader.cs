using System.Collections.Generic;
using System.Numerics;

namespace Pencils.RendererApi;

public interface IShader
{
    string Name { get; }
    
    void Use();
    void SetVertexAttrib(List<VertexAttrib> vertexAttribs);

    void UploadConstantStruct<T>(string constantName, T value, ShaderType visibleShader = ShaderType.Vertex) where T : struct;
    
    void UploadConstantMat44(string constantName, Matrix4x4 value, ShaderType visibleShader = ShaderType.Vertex);
    
    void UploadConstantFloat(string constantName, float value, ShaderType visibleShader = ShaderType.Vertex);
    void UploadConstantFloat2(string constantName, Vector2 value, ShaderType visibleShader = ShaderType.Vertex);
    void UploadConstantFloat3(string constantName, Vector3 value, ShaderType visibleShader = ShaderType.Vertex);
    void UploadConstantFloat4(string constantName, Vector4 value, ShaderType visibleShader = ShaderType.Vertex);
}