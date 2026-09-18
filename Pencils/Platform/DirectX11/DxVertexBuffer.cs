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
    
    private DxVertexBuffer(IResourcesFactory factory, uint stride, uint count)
    {
        _stride = stride;
        Count = count;
        _buffer = new ID3D11Buffer((nint)factory.CreateVertexBuffer(stride * count));

        AttribType = [];
    }

    private DxVertexBuffer(IResourcesFactory factory, nint dataPtr, uint stride, uint count)
    {
        _stride = stride;
        Count = count;
        _buffer = new ID3D11Buffer((nint)factory.CreateVertexBuffer(dataPtr, stride * count));
        
        AttribType = [];
    }
    
    public void SetVertexAttribs(params VertexAttribType[] vertexAttris) => AttribType = vertexAttris;
    
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
    
    public static IVertexBuffer Create<T>(IGraphicsContext context, uint count) where T : unmanaged
    {
        return new DxVertexBuffer(context.ResourcesFactory, (uint)Unsafe.SizeOf<T>(), count);
    }

    public static unsafe IVertexBuffer Create<T>(IGraphicsContext context, T[] vertex) where T : unmanaged
    {
        return new DxVertexBuffer(context.ResourcesFactory, (nint)vertex.GetPointerUnsafe(), (uint)Unsafe.SizeOf<T>(), (uint)vertex.Length);
    }
}