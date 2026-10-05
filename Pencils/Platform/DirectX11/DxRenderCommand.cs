using System;
using System.Collections.Generic;
using Pencils.RendererApi;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Pencils.Platform.DirectX11;

public class DxRenderCommand : IRenderCommand
{
    private readonly ID3D11DeviceContext _d3DContext = DxContext.Context;
    private readonly Dictionary<long , ID3D11RasterizerState> _rasterizerStates = new();
    private readonly Dictionary<DepthStencilInfo , ID3D11DepthStencilState> _depthStencilStates = new();
    private readonly Dictionary<BlendStateInfo , ID3D11BlendState> _blendStates = new();
    
    
    public void DefaultPrimitiveTopology() => _d3DContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

    public void DrawIndexed(uint count, uint indexOffset = 0, uint vertexOffset = 0) => _d3DContext.DrawIndexed(count, indexOffset, (int)vertexOffset);
    public void DrawVertex(uint count, uint vertexOffset = 0) => _d3DContext.Draw(count, vertexOffset);
    public void SetViewport(Viewport viewport) => _d3DContext.RSSetViewport(viewport);
    public void SetScissor(RawRect scissor) => _d3DContext.RSSetScissorRect(scissor);
    
    public void SetFillAndCull(Fill fillMode, Cull cullMode)
    {
        long vp = ((long)fillMode << 32) | ((uint)cullMode & 0xFFFFFFFFL);

        if (!_rasterizerStates.TryGetValue(vp, out var rasterizerState))
        {
            rasterizerState = new ID3D11RasterizerState((nint)DxContext.ResourcesFactory.CreateRasterizerState(cullMode, fillMode));
            _rasterizerStates.Add(vp, rasterizerState);
        }
        
        _d3DContext.RSSetState(rasterizerState);
    }

    public void SetDepthStStencilState(DepthStencilInfo info)
    {
        info = info == default ? new DepthStencilInfo() : info;
        
        if (!_depthStencilStates.TryGetValue(info, out var depthState))
        {
            depthState = new ID3D11DepthStencilState((nint)DxContext.ResourcesFactory.CreateDepthStencilState(info));
            _depthStencilStates.Add(info, depthState);
        }
        _d3DContext.OMSetDepthStencilState(depthState);
    }

    public void SetBlendState(BlendStateInfo blendInfo)
    {
        blendInfo = blendInfo == default ? new BlendStateInfo() : blendInfo;
        if (!_blendStates.TryGetValue(blendInfo, out var blendState))
        {
            blendState = new ID3D11BlendState((nint)DxContext.ResourcesFactory.CreateBlendState(blendInfo));
            _blendStates.Add(blendInfo, blendState);
        }
        _d3DContext.OMSetBlendState(blendState);
    }
}