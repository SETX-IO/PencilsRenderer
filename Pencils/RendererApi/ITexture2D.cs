namespace Pencils.RendererApi;

public interface ITexture2D : ITexture
{
    static abstract ITexture2D Create(IGraphicsContext graphicsContext, string path);
    static abstract ITexture2D Create(IGraphicsContext graphicsContext);
}