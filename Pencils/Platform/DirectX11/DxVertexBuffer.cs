using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Pencils.RendererApi;
using SharpGen.Runtime;
using Vortice.Direct3D11;

namespace Pencils.Platform.DirectX11;

public class DxVertexBuffer : IVertexBuffer
{
    private const uint MaxSlotCount = ID3D11DeviceContext.InputAssemblerVertexInputResourceSlotCount;
    private static uint CurrentSlot;
    
    private readonly ID3D11Buffer _buffer;
    private readonly uint _stride;
    
    public uint Count { get; }
    
    public VertexAttribType[] AttribType { get; private set; }
    
    private DxVertexBuffer(uint stride, uint count)
    {
        _stride = stride;
        Count = count;
        _buffer = new ID3D11Buffer((nint)DxContext.ResourcesFactory.CreateVertexBuffer(stride * count));

        AttribType = [];
    }

    private DxVertexBuffer(nint dataPtr, uint stride, uint count)
    {
        _stride = stride;
        Count = count;
        _buffer = new ID3D11Buffer((nint)DxContext.ResourcesFactory.CreateVertexBuffer(dataPtr, stride * count));
        
        AttribType = [];
    }
    
    public void SetVertexAttribs(params VertexAttribType[] vertexAttris) => AttribType = vertexAttris;
    
    public nint Map<T>() where T : struct => DxContext.Context.Map(_buffer, MapMode.WriteDiscard).DataPointer;


    public void CloseMap() => DxContext.Context.Unmap(_buffer);

    public unsafe void SetData<T>(Span<T> data) where T : struct
    {
        var ctx = DxContext.Context;

        var dataPtr = ctx.Map(_buffer, MapMode.WriteDiscard).DataPointer;
        Unsafe.Copy((void*)dataPtr, ref data.GetPinnableReference());
        ctx.Unmap(_buffer);
    }

    public void Bind()
    {
        if (CurrentSlot >= MaxSlotCount)
            throw new AbandonedMutexException();
        DxContext.Context.IASetVertexBuffer(CurrentSlot, _buffer, _stride);
        CurrentSlot++;
    }

    public void Unbind()
    {
        DxContext.Context.IASetVertexBuffer(CurrentSlot, null!, _stride);
        CurrentSlot--;
    }
    
    public static IVertexBuffer Create<T>(uint count) where T : unmanaged
    {
        return new DxVertexBuffer((uint)Unsafe.SizeOf<T>(), count);
    }

    public static unsafe IVertexBuffer Create<T>(T[] vertex) where T : unmanaged
    {
        return new DxVertexBuffer((nint)vertex.GetPointerUnsafe(), (uint)Unsafe.SizeOf<T>(), (uint)vertex.Length);
    }
}