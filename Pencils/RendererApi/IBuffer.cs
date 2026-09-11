namespace Pencils.RendererApi;

public interface IBuffer
{
    uint Count { get; }
    void Bind();
    void Unbind();
}