using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Spine;

namespace Pencils.Extension.Spine;

public class PencilsTextureLoader : TextureLoader
{
    public void Load(AtlasPage page, string path)
    {
        page.rendererObject = DxTexture2D.Create(path);
    }

    public void Unload(object texture)
    {
        // ((ITexture)texture).Unbind();
    }
}