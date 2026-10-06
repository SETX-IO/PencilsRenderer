using System;
using System.Runtime.CompilerServices;
using Pencils.RendererApi;
using Vortice.Direct3D11;

namespace Pencils.Platform.DirectX11;

public abstract class DxBaseBuffer : IBuffer
{
    protected ID3D11Buffer buffer;

    public uint Count { get; protected init; }
    
    public virtual void Bind(uint slot = 0) { }

    public virtual void Unbind(uint slot = 0) { }

    public nint Map() => DxContext.Context.Map(buffer, MapMode.WriteDiscard).DataPointer;
    
    public void CloseMap() => DxContext.Context.Unmap(buffer);

    public unsafe void SetData<T>(Span<T> data) where T : struct
    {
        var dataPtr = Map();
        Unsafe.Copy((void*)dataPtr, ref data.GetPinnableReference());
        CloseMap();
    }

    public virtual void Dispose() {}
}