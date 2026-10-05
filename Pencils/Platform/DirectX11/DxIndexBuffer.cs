using System;
using System.Runtime.CompilerServices;
using Pencils.RendererApi;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Pencils.Platform.DirectX11;

public class DxIndexBuffer : IIndexBuffer
{
    private ID3D11Buffer _buffer;
    
    public uint Count { get; }
    
    private DxIndexBuffer(uint count)
    {
        Count = count;
        _buffer = new ID3D11Buffer((nint)DxContext.ResourcesFactory.CreateIndexBuffer(Count));
    }
    
    private DxIndexBuffer(ReadOnlySpan<ushort> indices)
    {
        Count = (uint)indices.Length;
        _buffer = new ID3D11Buffer((nint)DxContext.ResourcesFactory.CreateIndexBuffer(indices));
    }
    
    public void Bind()
    {
        DxContext.Context.IASetIndexBuffer(_buffer, Format.R16_UInt, 0);
    }

    public void Unbind()
    {
        DxContext.Context.IASetIndexBuffer(null, Format.R16_UInt, 0);
    }

    public nint Map()=> DxContext.Context.Map(_buffer, MapMode.WriteDiscard).DataPointer;


    public void CloseMap() => DxContext.Context.Unmap(_buffer);

    public unsafe void SetData<T>(Span<T> data) where T : struct
    {
        var dataPtr = Map();
        
        Unsafe.Copy((void*)dataPtr, ref data.GetPinnableReference());
        
        CloseMap();
    }

    public static IIndexBuffer Create(uint count)
    {
        return new DxIndexBuffer(count);
    }
    
    public static IIndexBuffer Create(ReadOnlySpan<ushort> indices)
    {
        return new DxIndexBuffer(indices);
    }

    public void Dispose() =>_buffer.Dispose();
    
}