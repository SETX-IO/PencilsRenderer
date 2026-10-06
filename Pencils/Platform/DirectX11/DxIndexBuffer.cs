using System;
using Pencils.RendererApi;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Pencils.Platform.DirectX11;

public class DxIndexBuffer : DxBaseBuffer, IIndexBuffer
{
    private DxIndexBuffer(uint count)
    {
        Count = count;
        buffer = new ID3D11Buffer((nint)DxContext.ResourcesFactory.CreateIndexBuffer(Count));
    }
    
    private DxIndexBuffer(ReadOnlySpan<ushort> indices)
    {
        Count = (uint)indices.Length;
        buffer = new ID3D11Buffer((nint)DxContext.ResourcesFactory.CreateIndexBuffer(indices));
    }

    public void SetData(Span<ushort> data) => base.SetData(data);

    public override void Bind(uint slot) => DxContext.Context.IASetIndexBuffer(buffer, Format.R16_UInt, 0);

    public override void Unbind(uint slot) => DxContext.Context.IASetIndexBuffer(null, Format.R16_UInt, 0);

    public static IIndexBuffer Create(uint count)
    {
        return new DxIndexBuffer(count);
    }
    
    public static IIndexBuffer Create(ReadOnlySpan<ushort> indices)
    {
        return new DxIndexBuffer(indices);
    }

    public override void Dispose() => buffer.Dispose();
}