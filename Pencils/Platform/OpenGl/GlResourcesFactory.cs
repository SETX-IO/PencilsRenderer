using Pencils.RendererApi;

namespace Pencils.Platform.OpenGl;

public class GlResourcesFactory
{
    public long CreateVertexBuffer(uint size)
    {
        return 0;
    }

    public long CreateIndexBuffer(uint size)
    {
        return 0;
    }

    public long CreateIndexBuffer(ReadOnlySpan<ushort> indices)
    {
        return 0;
    }

    public long CreateVertexBuffer(IntPtr dataPtr, uint size)
    {
        throw new NotImplementedException();
    }
}