using Pencils.RendererApi;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Pencils.Platform.DirectX11;

public class DxRenderCommand : IRenderCommand
{
    private readonly ID3D11DeviceContext _d3DContext = DxContext.Context;
    
    public void DefaultPrimitiveTopology() => _d3DContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

    public void DrawIndexed(uint count, uint indexOffset = 0, uint vertexOffset = 0) => _d3DContext.DrawIndexed(count, indexOffset, (int)vertexOffset);
    public void DrawVertex(uint count, uint vertexOffset = 0) => _d3DContext.Draw(count, vertexOffset);
    public void SetViewport(Viewport viewport) => _d3DContext.RSSetViewport(viewport);
}