using System;
using System.Runtime.CompilerServices;
using Pencils.Platform.DirectX11.Utility;
using Pencils.RendererApi;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Pencils.Platform.DirectX11;

public unsafe class DxResourcesFactory : IResourcesFactory
{
    private readonly ID3D11Device _device;

    public DxResourcesFactory(IGraphicsContext context)
    {
        if (context.Api != GraphicsApi.DirectX11)
            throw new Exception("GraphicsApi not is DirectX11.");
        
        _device = ((DxContext)context).Device;
    }

    public long CreateVertexBuffer(uint size)
    {
        BufferDescription desc = new BufferDescription(size, BindFlags.VertexBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);
        return CreateBuffer(desc);
    }
    
    public long CreateVertexBuffer(nint dataPtr, uint size)
    {
        BufferDescription desc = new BufferDescription(size, BindFlags.VertexBuffer);
        SubresourceData data = new SubresourceData(dataPtr);
        return CreateBuffer(desc, data);
    }

    public long CreateIndexBuffer(uint count)
    {
        BufferDescription desc = new BufferDescription((uint)(count * Unsafe.SizeOf<ushort>()), BindFlags.IndexBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);
        return CreateBuffer(desc);
    }

    public long CreateIndexBuffer(ReadOnlySpan<ushort> indices)
    {
        BufferDescription desc = new BufferDescription((uint)(indices.Length * Unsafe.SizeOf<ushort>()), BindFlags.IndexBuffer);
        SubresourceData data = new SubresourceData(indices.GetPointerUnsafe());
        return CreateBuffer(desc, data);
    }

    public long CreateConstantBuffer(uint size)
    {
        BufferDescription buffer =
            new BufferDescription(size, BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);

        return CreateBuffer(buffer);
    }

    private long CreateBuffer(BufferDescription desc, SubresourceData? data = null)
    {
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

    public long CreateTexture2D(string path, out uint width, out uint height, Texture2DFormat format)
    {
        var textureData = Texture2DLoad.Load(path, out width, out height, out Format textureFormat);
        
        Texture2DDescription textureDesc = new(textureFormat, width, height, 1, 1);
        ShaderResourceViewDescription srvDesc = new(ShaderResourceViewDimension.Texture2D, textureDesc.Format);
        
        SubresourceData subresourceData = new SubresourceData(textureData.Span.GetPointerUnsafe(), (uint)textureData.Length / height);
        var texture2D = _device.CreateTexture2D(textureDesc, subresourceData);

        var srv = _device.CreateShaderResourceView(texture2D, srvDesc);

        return srv.NativePointer.ToInt64();
    }

    public long CreateTexture2D(uint width, uint height, Texture2DFormat format, uint arraySize)
    {
        Texture2DDescription textureDesc = new(Format.R8G8B8A8_UNorm, width, height, arraySize, 1);
        ShaderResourceViewDimension dimension = arraySize >= 1 ? ShaderResourceViewDimension.Texture2DArray : ShaderResourceViewDimension.Texture2D;
        ShaderResourceViewDescription srvDesc = new(dimension, textureDesc.Format);
        
        var texture2D = _device.CreateTexture2D(textureDesc);
        var srv = _device.CreateShaderResourceView(texture2D, srvDesc);

        return srv.NativePointer.ToInt64();
    }

    public long CreateRasterizerState(Cull cull, Fill fill)
    {
        RasterizerDescription rasterizerDesc = new((CullMode)cull, (FillMode)fill)
        {
            ScissorEnable = true,
            DepthClipEnable = true
        };
        var state = _device.CreateRasterizerState(rasterizerDesc);
        
        return state.NativePointer.ToInt64();
    }

    public long CreateDepthStencilState(DepthStencilInfo info)
    {
        DepthStencilDescription depthStencilInfo = new()
        {
            DepthEnable = info.depthEnable,
            StencilEnable = info.stencilEnable,
            DepthFunc = (ComparisonFunction)info.depthFunc,
            DepthWriteMask = (DepthWriteMask)info.DepthWriteMask,
        };
        
        var depthStencilState = _device.CreateDepthStencilState(depthStencilInfo);
        
        return depthStencilState.NativePointer.ToInt64();
    }

    public long CreateBlendState(BlendStateInfo info)
    {
        BlendDescription desc = new();
        ref RenderTargetBlendDescription rt = ref desc.RenderTarget[0];
        rt.BlendEnable = info.enabled;
        
        rt.SourceBlend = (Blend)info.srcFactor;
        rt.DestinationBlend = (Blend)info.dstFactor;
        rt.BlendOperation = (BlendOperation)info.blendOp;
        rt.SourceBlendAlpha = (Blend)info.srcAlphaFactor;
        rt.DestinationBlendAlpha = (Blend)info.dstAlphaFactor;
        rt.BlendOperationAlpha = (BlendOperation)info.blendOpAlpha;
        rt.RenderTargetWriteMask = ColorWriteEnable.All;
        
        var blendState = _device.CreateBlendState(desc);
        return blendState.NativePointer.ToInt64();
    }
}