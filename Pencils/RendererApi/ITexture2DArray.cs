namespace Pencils.RendererApi;

public interface ITexture2DArray : ITexture
{
    const int MaxCount = 32;
    
    float AddTexture(ITexture2D texture);
}