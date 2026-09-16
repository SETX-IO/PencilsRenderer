namespace Pencils.RendererApi;

public interface ITexture
{
    uint Width { get; }
    uint Height { get; }

    void Bind(uint slot);
}