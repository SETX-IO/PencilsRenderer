namespace Pencils.RendererApi;

public interface IShaderLibrary
{
    IShader this[string name] { get; set; }

    IShader Load(string shaderPath);
    IShader Load(string shaderName, string shaderCode);
}