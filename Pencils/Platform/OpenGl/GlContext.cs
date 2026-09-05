using System.Drawing;
using Pencils.RendererApi;

namespace Pencils.Platform.OpenGl;

public class GlContext : IGraphicsContext
{
    public IResourcesFactory ResourcesFactory { get; }
    public void Init()
    {
        throw new NotImplementedException();
    }

    public void SetBufferColor(Color color)
    {
        throw new NotImplementedException();
    }

    public void ReSizeBuffer(uint width, uint height)
    {
        throw new NotImplementedException();
    }

    public void SwapBuffers()
    {
        throw new NotImplementedException();
    }

    public void ClearBuffer()
    {
        throw new NotImplementedException();
    }
}