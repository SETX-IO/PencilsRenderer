using System.Runtime.CompilerServices;
using System.Threading;
using Pencils.RendererApi;
using SharpGen.Runtime;
using Vortice.Direct3D11;

namespace Pencils.Platform.DirectX11;

public class DxVertexBuffer : DxBaseBuffer, IVertexBuffer
{
    private const uint MaxSlotCount = ID3D11DeviceContext.InputAssemblerVertexInputResourceSlotCount;
    private readonly uint _stride;
    
    public VertexAttribType[] AttribType { get; private set; }
    
    private DxVertexBuffer(uint stride, uint count)
    {
        _stride = stride;
        Count = count;
        buffer = new ID3D11Buffer((nint)DxContext.ResourcesFactory.CreateVertexBuffer(stride * count));

        AttribType = [];
    }

    private DxVertexBuffer(nint dataPtr, uint stride, uint count)
    {
        _stride = stride;
        Count = count;
        buffer = new ID3D11Buffer((nint)DxContext.ResourcesFactory.CreateVertexBuffer(dataPtr, stride * count));
        
        AttribType = [];
    }
    
    public void SetVertexAttribs(params VertexAttribType[] vertexAttris) => AttribType = vertexAttris;

    public override void Bind(uint slot)
    {
        if (slot >= MaxSlotCount)
            throw new AbandonedMutexException();
        DxContext.Context.IASetVertexBuffer(slot, buffer, _stride);
    }

    public override void Unbind(uint slot)
    {
        if (slot >= MaxSlotCount)
            throw new AbandonedMutexException();
        DxContext.Context.IASetVertexBuffer(slot, null!, _stride);
    }
    
    public static IVertexBuffer Create<T>(uint count) where T : unmanaged
    {
        return new DxVertexBuffer((uint)Unsafe.SizeOf<T>(), count);
    }

    public static unsafe IVertexBuffer Create<T>(T[] vertex) where T : unmanaged
    {
        return new DxVertexBuffer((nint)vertex.GetPointerUnsafe(), (uint)Unsafe.SizeOf<T>(), (uint)vertex.Length);
    }

    public override void Dispose() => buffer.Dispose();
}