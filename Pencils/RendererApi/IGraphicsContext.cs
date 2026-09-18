using System.Drawing;

namespace Pencils.RendererApi;

public interface IGraphicsContext
{
    IResourcesFactory ResourcesFactory { get; }
    GraphicsApi Api { get; }
    
    void Init();
    void SetBufferColor(Color color);
    void ReSizeBuffer(uint width, uint height);
    void SwapBuffers();
    void ClearBuffer();
}