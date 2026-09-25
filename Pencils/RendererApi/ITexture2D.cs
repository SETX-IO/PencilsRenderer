namespace Pencils.RendererApi;

public interface ITexture2D : ITexture
{
    static abstract ITexture2D Create(IResourcesFactory factory, string path);
    static abstract ITexture2D Create(IResourcesFactory factory, uint width, uint height);
}