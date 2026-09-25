using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Pencils.RendererApi;
using Serilog;
using Silk.NET.OpenGL;
using Vortice.Direct3D11;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11.Shader;
using Vortice.DXGI;
using ShaderType = Pencils.RendererApi.ShaderType;

namespace Pencils.Platform.DirectX11;

public class ShaderConstant(uint slot, ID3D11Buffer buffer)
{
    public uint slot = slot;
    public ID3D11Buffer constantBuffer = buffer;
}

public class DxShader : IShader
{
    // baseShader
    private readonly ID3D11VertexShader? _vertexShader;
    private readonly ID3D11PixelShader? _pixelShader;
    
    private readonly ID3D11HullShader? _hullShader;
    private readonly ID3D11DomainShader? _domainShader;
    private readonly ID3D11GeometryShader? _geometryShader;
    private readonly ID3D11ComputeShader? _computeShader;

    private readonly bool _isBaseShader;
    private ID3D11InputLayout? _inputLayout;
    
    private readonly Dictionary<ShaderType, ID3D11ShaderReflection> _shaderReflection = new();

    private readonly Dictionary<ShaderType, Dictionary<string, ShaderConstant>> _constantBuffers;
    
    public string Name { get; }

    private DxShader(IResourcesFactory factory, string shaderPath)
    {
        _isBaseShader = true;
        Name = Path.GetFileNameWithoutExtension(shaderPath);
        _constantBuffers = new Dictionary<ShaderType, Dictionary<string, ShaderConstant>>();

        var shaderBytes = CompileForFile(ShaderType.Vertex, shaderPath);
        var vsShaderBytes = shaderBytes;
        
        _shaderReflection.Add(ShaderType.Vertex, Compiler.Reflect<ID3D11ShaderReflection>(shaderBytes.Span));
        _vertexShader = new ID3D11VertexShader((nint)factory.CreateShader(ShaderType.Vertex, shaderBytes.Span));
        
        shaderBytes = CompileForFile(ShaderType.Pixel, shaderPath);
        _pixelShader = new ID3D11PixelShader((nint)factory.CreateShader(ShaderType.Pixel, shaderBytes.Span));
        _shaderReflection.Add(ShaderType.Pixel, Compiler.Reflect<ID3D11ShaderReflection>(shaderBytes.Span));

        foreach (var reflection in _shaderReflection)
        {
            Dictionary<string, ShaderConstant> constants = new();
            for (int i = 0; i < reflection.Value.ConstantBuffers.Length; i++)
            {
                ConstantBufferDescription info = reflection.Value.ConstantBuffers[i].Description;

                var constant = DxContext.Context.Device.CreateBuffer(new BufferDescription(info.Size, BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));
                constants.Add(info.Name, new ShaderConstant((uint)i, constant));
            }
            _constantBuffers.Add(reflection.Key, constants);
        }

        ConfigVertexAttrib(vsShaderBytes);
    }
    
    private DxShader(IResourcesFactory factory, string shaderCode, bool isBaseShader)
    {
        _isBaseShader = isBaseShader;
        _constantBuffers = new Dictionary<ShaderType, Dictionary<string, ShaderConstant>>();

        switch (isBaseShader)
        {
            case true when !shaderCode.Contains("vert") && !shaderCode.Contains("frag"):
                throw new ArgumentException(shaderCode);
            case true:
            {
                var il = Compile(ShaderType.Vertex, shaderCode);
                ReadOnlyMemory<byte> vsShaderBytes = il;
                
                _vertexShader = new ID3D11VertexShader((nint)factory.CreateShader(ShaderType.Vertex, il.Span));
                _shaderReflection[ShaderType.Vertex] = Compiler.Reflect<ID3D11ShaderReflection>(il.Span);
                
                il = Compile(ShaderType.Pixel, shaderCode);
                _pixelShader = new ID3D11PixelShader((nint)factory.CreateShader(ShaderType.Pixel, il.Span));
                _shaderReflection.Add(ShaderType.Pixel, Compiler.Reflect<ID3D11ShaderReflection>(il.Span));
                
                ConfigVertexAttrib(vsShaderBytes);
                
                break;
            }
        }

        if (shaderCode.Contains("hull"))
        {
            var il = Compile(ShaderType.Hull, shaderCode);
            _hullShader = new ID3D11HullShader((nint)factory.CreateShader(ShaderType.Hull, il.Span));
            _shaderReflection.Add(ShaderType.Hull, Compiler.Reflect<ID3D11ShaderReflection>(il.Span));
        }
        
        if (shaderCode.Contains("domain"))
        {
            var il = Compile(ShaderType.Domain, shaderCode);
            _domainShader = new ID3D11DomainShader((nint)factory.CreateShader(ShaderType.Domain, il.Span));
            _shaderReflection.Add(ShaderType.Domain, Compiler.Reflect<ID3D11ShaderReflection>(il.Span));
        }
        
        if (shaderCode.Contains("geometry"))
        {
            var il = Compile(ShaderType.Geometry, shaderCode);
            _geometryShader = new ID3D11GeometryShader((nint)factory.CreateShader(ShaderType.Geometry, il.Span));
            _shaderReflection.Add(ShaderType.Geometry, Compiler.Reflect<ID3D11ShaderReflection>(il.Span));
        }
        
        if (shaderCode.Contains("compute"))
        {
            var il = Compile(ShaderType.Compute, shaderCode);
            _computeShader = new ID3D11ComputeShader((nint)factory.CreateShader(ShaderType.Compute, il.Span));
            _shaderReflection.Add(ShaderType.Compute, Compiler.Reflect<ID3D11ShaderReflection>(il.Span));
        }
        
        foreach (var reflection in _shaderReflection)
        {
            Dictionary<string, ShaderConstant> constants = new();
            for (int i = 0; i < reflection.Value.ConstantBuffers.Length; i++)
            {
                ConstantBufferDescription info = reflection.Value.ConstantBuffers[i].Description;

                var constant = DxContext.Context.Device.CreateBuffer(new BufferDescription(info.Size, BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));
                constants.Add(info.Name, new ShaderConstant((uint)i, constant));
            }
            _constantBuffers.Add(reflection.Key, constants);
        }
    }
    
    public void Use()
    {
        var ctx = DxContext.Context;

        if (_isBaseShader)
        {
            if (_inputLayout != null)
                ctx.IASetInputLayout(_inputLayout);
            
            ctx.VSSetShader(_vertexShader);
            ctx.PSSetShader(_pixelShader);
        }
        
        ctx.HSSetShader(_hullShader);
        ctx.DSSetShader(_domainShader);
        ctx.GSSetShader(_geometryShader);
        ctx.CSSetShader(_computeShader);
    }

    public void SetVertexAttrib(List<VertexAttrib> vertexAttribs) { }
    
    public void ConfigVertexAttrib(ReadOnlyMemory<byte> vsShaderByte)
    {
        ID3D11ShaderReflection reflection = _shaderReflection[ShaderType.Vertex];
        InputElementDescription[] inputElements = new InputElementDescription[reflection.InputParameters.Length];

        uint offset = 0;
        uint slot = 0;
        for (int i = 0; i < inputElements.Length; i++)
        {
            ref InputElementDescription refInputElement = ref inputElements[i];
            var inputParameter = reflection.InputParameters[i];

            if (slot != inputParameter.Stream)
                offset = 0;
            
            uint padding = inputParameter.ComponentType switch
            {
                RegisterComponentType.UInt32 or RegisterComponentType.SInt32 or RegisterComponentType.Float32 => 4,
                RegisterComponentType.UInt16 or RegisterComponentType.SInt16 or RegisterComponentType.Float16 => 2,
                RegisterComponentType.UInt64 or RegisterComponentType.SInt64 or RegisterComponentType.Float64 => 8,
                RegisterComponentType.Unknown => throw new ArgumentOutOfRangeException()
            };

            uint paddingCount = inputParameter.UsageMask switch
            {
                RegisterComponentMaskFlags.ComponentX => 1,
                RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY => 2,
                RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ => 3,
                RegisterComponentMaskFlags.All => 4,
                _ => throw new ArgumentOutOfRangeException()
            };

            Format format = (padding * paddingCount) switch
            {
                2 => Format.R16_Float,
                4 => Format.R32_Float,
                8 => Format.R32G32_Float,
                12 => Format.R32G32B32_Float,
                16 => Format.R32G32B32A32_Float
            };

            refInputElement = new InputElementDescription(inputParameter.SemanticName, inputParameter.SemanticIndex, format, offset, inputParameter.Stream);
            offset += padding * paddingCount;
            slot = inputParameter.Stream;
        }
    
        _inputLayout = DxContext.Context.Device.CreateInputLayout(inputElements, vsShaderByte.Span);
    }

    public void UploadConstantStruct<T>(string constantName, T value, ShaderType visibleShader = ShaderType.Vertex) where T : struct
        => UploadContextData(constantName, ref value, visibleShader);

    public void UploadConstantMat44(string constantName, Matrix4x4 mat, ShaderType visibleShader)
    {
        mat = Matrix4x4.Transpose(mat);
        UploadContextData(constantName, ref mat, visibleShader);
    }

    public void UploadConstantFloat(string constantName, float value, ShaderType visibleShader = ShaderType.Vertex)
        => UploadContextData(constantName, ref value, visibleShader);

    public void UploadConstantFloat2(string constantName, Vector2 value, ShaderType visibleShader = ShaderType.Vertex)
        => UploadContextData(constantName, ref value, visibleShader);

    public void UploadConstantFloat3(string constantName, Vector3 value, ShaderType visibleShader = ShaderType.Vertex) =>
        UploadContextData(constantName, ref value, visibleShader);

    public void UploadConstantFloat4(string constantName, Vector4 value, ShaderType visibleShader = ShaderType.Vertex) 
        => UploadContextData(constantName, ref value, visibleShader);


    private unsafe void UploadContextData<T>(string constantName, ref T data, ShaderType visibleShader) where T : struct
    {
        var ctx = DxContext.Context;
        if (!_constantBuffers.TryGetValue(visibleShader, out var buffer))
        {
            Log.Logger.Error($"Shader {constantName} does not exist");
            return;
        }
        
        if (!buffer.TryGetValue(constantName, out var constantBuffer))
            return;
        
        switch (visibleShader)
        {
            case ShaderType.Vertex:
                ctx.VSSetConstantBuffer(constantBuffer.slot, constantBuffer.constantBuffer);
                break;
            case ShaderType.Pixel:
                ctx.PSSetConstantBuffer(constantBuffer.slot, constantBuffer.constantBuffer);
                break;
            case ShaderType.Hull:
                ctx.HSSetConstantBuffer(constantBuffer.slot, constantBuffer.constantBuffer);
                break;
            case ShaderType.Domain:
                ctx.DSSetConstantBuffer(constantBuffer.slot, constantBuffer.constantBuffer);
                break;
            case ShaderType.Geometry:
                ctx.GSSetConstantBuffer(constantBuffer.slot, constantBuffer.constantBuffer);
                break;
            case ShaderType.Compute:
                ctx.CSSetConstantBuffer(constantBuffer.slot, constantBuffer.constantBuffer);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(visibleShader), visibleShader, null);
        }
        
        var dataPtr = ctx.Map(constantBuffer.constantBuffer, MapMode.WriteDiscard).DataPointer;
        Unsafe.Copy((void*)dataPtr, ref data);
        ctx.Unmap(constantBuffer.constantBuffer);
    }

    public static IShader Create(IResourcesFactory factory, string shaderCode, bool isBaseShader = true) =>
        new DxShader(factory, shaderCode, isBaseShader);

    public static IShader Create(IResourcesFactory factory, string shaderPath)
    {
        return new DxShader(factory, shaderPath);
    }

    public static ReadOnlyMemory<byte> Compile(ShaderType type, string shaderCode)
    {
        (string entryPoitn, string profile) shaderProfile = type switch
        {
            ShaderType.Vertex => ("vert", "vs"),
            ShaderType.Pixel => ("frag", "ps"),
            ShaderType.Hull => ("hull", "hs"),
            ShaderType.Domain => ("domain", "ds"),
            ShaderType.Geometry => ("geometry", "gs"),
            ShaderType.Compute => ("compute", "cs"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        var il = Compiler.Compile(shaderCode, shaderProfile.entryPoitn, shaderProfile.entryPoitn, shaderProfile.profile + "_5_0");

        return il;
    }

    public static ReadOnlyMemory<byte> CompileForFile(ShaderType type, string shaderPath)
    {
        (string entryPoitn, string profile) shaderProfile = type switch
        {
            ShaderType.Vertex => ("vert", "vs"),
            ShaderType.Pixel => ("frag", "ps"),
            ShaderType.Hull => ("hull", "hs"),
            ShaderType.Domain => ("domain", "ds"),
            ShaderType.Geometry => ("geometry", "gs"),
            ShaderType.Compute => ("compute", "cs"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        var il = Compiler.CompileFromFile(shaderPath, shaderProfile.entryPoitn, shaderProfile.profile + "_5_0");
        
        return il;
    }
}