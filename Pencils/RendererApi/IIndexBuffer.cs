namespace Pencils.RendererApi;

public interface IIndexBuffer : IBuffer
{
    uint Count { get; }
    static abstract IIndexBuffer Create(IGraphicsContext context, uint size);
}