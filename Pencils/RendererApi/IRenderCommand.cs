using Vortice;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Pencils.RendererApi;

public interface IRenderCommand
{
    /// <summary>
    /// Default PrimitiveTopology = TriangleList
    /// </summary>
    void DefaultPrimitiveTopology();
    
    void DrawIndexed(uint count, uint indexOffset = 0, uint vertexOffset = 0);
    void DrawVertex(uint count ,uint vertexOffset = 0);
    void SetViewport(Viewport viewport);
    void SetScissor(RawRect scissor);
    void SetFillAndCull(Fill fillMode = Fill.Solid, Cull cullMode = Cull.Back);
    void SetDepthStStencilState(DepthStencilInfo info = default);
    void SetBlendState(BlendStateInfo blendInfo = default);
}