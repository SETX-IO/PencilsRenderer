using Vortice.Mathematics;

namespace Pencils.RendererApi;

public interface IRenderCommand
{
    void DefaultPrimitiveTopology();
    
    void DrawIndexed(uint count, uint indexOffset = 0, uint vertexOffset = 0);
    void DrawVertex(uint count ,uint vertexOffset = 0);
    void SetViewport(Viewport viewport);
}