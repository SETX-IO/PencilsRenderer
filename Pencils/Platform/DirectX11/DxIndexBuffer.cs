using System;
using Pencils.RendererApi;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Pencils.Platform.DirectX11;

public class DxIndexBuffer : IIndexBuffer
{
    private ID3D11Buffer _buffer;
    
    public uint Count { get; }
    
    private DxIndexBuffer(IResourcesFactory factory, uint count)
    {
        Count = count;
        _buffer = new ID3D11Buffer((nint)factory.CreateIndexBuffer(Count));
    }
    
    private DxIndexBuffer(IResourcesFactory factory, ReadOnlySpan<ushort> indices)
    {
        Count = (uint)indices.Length;
        _buffer = new ID3D11Buffer((nint)factory.CreateIndexBuffer(indices));
    }
    
    public void Bind()
    {
        DxContext.Context.IASetIndexBuffer(_buffer, Format.R16_UInt, 0);
    }

    public void Unbind()
    {
        DxContext.Context.IASetIndexBuffer(null, Format.R16_UInt, 0);
    }

    public static IIndexBuffer Create(IGraphicsContext context, uint count)
    {
        return new DxIndexBuffer(context.ResourcesFactory, count);
    }
    
    public static IIndexBuffer Create(IGraphicsContext context, ReadOnlySpan<ushort> indices)
    {
        return new DxIndexBuffer(context.ResourcesFactory, indices);
    }
}