namespace Pencils.RendererApi;

public interface IResourcesFactory
{
    /// <summary>
    /// DirectX is resource ptr
    /// OpenGl is buffer id;
    /// </summary>
    /// <returns>
    /// retune VertexBuffer ptr or id
    /// </returns>
    long CreateVertexBuffer(uint size);
    /// <summary>
    /// DirectX is resource ptr
    /// OpenGl is buffer id;
    /// </summary>
    /// <returns>
    /// retune VertexBuffer ptr or id
    /// </returns>
    long CreateVertexBuffer(nint dataPtr, uint size);
    /// <summary>
    /// DirectX is resource ptr
    /// OpenGl is buffer id;
    /// </summary>
    /// <returns>
    /// retune IndexBuffer ptr or id
    /// </returns>
    long CreateIndexBuffer(uint count);
    /// <summary>
    /// DirectX is resource ptr
    /// OpenGl is buffer id;
    /// </summary>
    /// <returns>
    /// retune IndexBuffer ptr or id
    /// </returns>
    long CreateIndexBuffer(ReadOnlySpan<ushort> indices);
    
    // 
    long CreateShader(ShaderType shaderType, ReadOnlySpan<byte> shaderIl);
    long CreateTexture2D(string path, out uint width, out uint height, Texture2DFormat format = Texture2DFormat.RGBA8);
}