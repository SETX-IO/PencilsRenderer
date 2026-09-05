using System.Runtime.CompilerServices;
using Pencils.RendererApi;
using SharpGen.Runtime;
using Vortice.Direct3D11;

namespace Pencils.Platform.DirectX11;

public unsafe class DxResourcesFactory(DxContext context) : IResourcesFactory
{
    private ID3D11Device _device = context.Device;

    public long CreateVertexBuffer(uint size)
    {
        BufferDescription desc = new BufferDescription(size, BindFlags.VertexBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);
        var buffer = _device.CreateBuffer(desc);

        return buffer.NativePointer.ToInt64();
    }
    
    public long CreateVertexBuffer(nint dataPtr, uint size)
    {
        BufferDescription desc = new BufferDescription(size, BindFlags.VertexBuffer);
        SubresourceData data = new SubresourceData(dataPtr);
        var buffer = _device.CreateBuffer(desc, data);

        return buffer.NativePointer.ToInt64();
    }

    public long CreateIndexBuffer(uint count)
    {
        BufferDescription desc = new BufferDescription((uint)(count * Unsafe.SizeOf<ushort>()), BindFlags.IndexBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);
        
        var buffer = _device.CreateBuffer(desc);

        return buffer.NativePointer.ToInt64();
    }

    public long CreateIndexBuffer(ReadOnlySpan<ushort> indices)
    {
        BufferDescription desc = new BufferDescription((uint)(indices.Length * Unsafe.SizeOf<ushort>()), BindFlags.IndexBuffer);
        
        SubresourceData data = new SubresourceData(indices.GetPointerUnsafe());
        var buffer = _device.CreateBuffer(desc, data);

        return buffer.NativePointer.ToInt64();
    }

    public long CreateShader(ShaderType shaderType, ReadOnlySpan<byte> shaderIl)
    {
        ID3D11DeviceChild shader = shaderType switch
        {
            ShaderType.Vertex => _device.CreateVertexShader(shaderIl),
            ShaderType.Pixel => _device.CreatePixelShader(shaderIl),
            ShaderType.Hull => _device.CreateHullShader(shaderIl),
            ShaderType.Domain => _device.CreateDomainShader(shaderIl),
            ShaderType.Geometry => _device.CreateGeometryShader(shaderIl),
            ShaderType.Compute => _device.CreateComputeShader(shaderIl),
            _ => throw new ArgumentOutOfRangeException(nameof(shaderType), shaderType, null)
        };

        return shader.NativePointer.ToInt64();
    }
}