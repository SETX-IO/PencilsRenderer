namespace Pencils.RendererApi;

public interface ITextureCube : ITexture
{
    void AddTexture(ITexture texture, TextureCubeDirection direction);
}