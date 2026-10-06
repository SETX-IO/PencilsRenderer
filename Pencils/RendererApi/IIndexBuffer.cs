namespace Pencils.RendererApi;

public interface IIndexBuffer : IBuffer
{
    static abstract IIndexBuffer Create(uint size);
}