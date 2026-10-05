using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Pencils.Platform.DirectX11.Utility;
using Pencils.RendererApi;
using Serilog;
using Vortice.Direct3D11;
using Vortice.D3DCompiler;
using Vortice.Direct3D11.Shader;

namespace Pencils.Platform.DirectX11;

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
    private readonly ID3D11InputLayout? _inputLayout;
    
    private readonly Dictionary<ShaderType, ID3D11ShaderReflection> _shaderReflection = new();
    private readonly Dictionary<ShaderType, Dictionary<string, ShaderConstant>> _constantBuffers;
    
    public string Name { get; }

    private DxShader(string shaderPath, VertexAttrib[]? vertexAttributes)
    {
        _isBaseShader = true;
        Name = Path.GetFileNameWithoutExtension(shaderPath);
        _constantBuffers = new Dictionary<ShaderType, Dictionary<string, ShaderConstant>>();

        var shaderBytes = ShaderTools.CompileForFile(ShaderType.Vertex, shaderPath);
        var vsShaderBytes = shaderBytes;
        var factory = DxContext.ResourcesFactory;
        
        _shaderReflection.Add(ShaderType.Vertex, Compiler.Reflect<ID3D11ShaderReflection>(shaderBytes.Span));
        _vertexShader = new ID3D11VertexShader((nint)factory.CreateShader(ShaderType.Vertex, shaderBytes.Span));
        
        shaderBytes = ShaderTools.CompileForFile(ShaderType.Pixel, shaderPath);
        _pixelShader = new ID3D11PixelShader((nint)factory.CreateShader(ShaderType.Pixel, shaderBytes.Span));
        _shaderReflection.Add(ShaderType.Pixel, Compiler.Reflect<ID3D11ShaderReflection>(shaderBytes.Span));
        
        _inputLayout = vertexAttributes == null
            ? ShaderTools.ConfigVertexAttrib(vsShaderBytes, _shaderReflection[ShaderType.Vertex])
            : ShaderTools.ConfigVertexAttrib(vsShaderBytes, vertexAttributes);
        
         InitConstantBuffer();
    }
    
    private DxShader(string shaderCode, bool isBaseShader, VertexAttrib[]? vertexAttributes)
    {
        var factory = DxContext.ResourcesFactory;
        _isBaseShader = isBaseShader;
        _constantBuffers = new Dictionary<ShaderType, Dictionary<string, ShaderConstant>>(); 

        switch (isBaseShader)
        {
            case true when !shaderCode.Contains("vert") && !shaderCode.Contains("frag"):
                throw new ArgumentException(shaderCode);
            case true:
            {
                var il = ShaderTools.Compile(ShaderType.Vertex, shaderCode);
                ReadOnlyMemory<byte> vsShaderBytes = il;
                
                _vertexShader = new ID3D11VertexShader((nint)factory.CreateShader(ShaderType.Vertex, il.Span));
                _shaderReflection[ShaderType.Vertex] = Compiler.Reflect<ID3D11ShaderReflection>(il.Span);
                
                il = ShaderTools.Compile(ShaderType.Pixel, shaderCode);
                _pixelShader = new ID3D11PixelShader((nint)factory.CreateShader(ShaderType.Pixel, il.Span));
                _shaderReflection.Add(ShaderType.Pixel, Compiler.Reflect<ID3D11ShaderReflection>(il.Span));
                
                _inputLayout = vertexAttributes == null
                    ? ShaderTools.ConfigVertexAttrib(vsShaderBytes, _shaderReflection[ShaderType.Vertex])
                    : ShaderTools.ConfigVertexAttrib(vsShaderBytes, vertexAttributes);
                
                break;
            }
        }

        if (shaderCode.Contains("hull"))
        {
            var il = ShaderTools.Compile(ShaderType.Hull, shaderCode);
            _hullShader = new ID3D11HullShader((nint)factory.CreateShader(ShaderType.Hull, il.Span));
            _shaderReflection.Add(ShaderType.Hull, Compiler.Reflect<ID3D11ShaderReflection>(il.Span));
        }
        
        if (shaderCode.Contains("domain"))
        {
            var il = ShaderTools.Compile(ShaderType.Domain, shaderCode);
            _domainShader = new ID3D11DomainShader((nint)factory.CreateShader(ShaderType.Domain, il.Span));
            _shaderReflection.Add(ShaderType.Domain, Compiler.Reflect<ID3D11ShaderReflection>(il.Span));
        }
        
        if (shaderCode.Contains("geometry"))
        {
            var il = ShaderTools.Compile(ShaderType.Geometry, shaderCode);
            _geometryShader = new ID3D11GeometryShader((nint)factory.CreateShader(ShaderType.Geometry, il.Span));
            _shaderReflection.Add(ShaderType.Geometry, Compiler.Reflect<ID3D11ShaderReflection>(il.Span));
        }
        
        if (shaderCode.Contains("compute"))
        {
            var il = ShaderTools.Compile(ShaderType.Compute, shaderCode);
            _computeShader = new ID3D11ComputeShader((nint)factory.CreateShader(ShaderType.Compute, il.Span));
            _shaderReflection.Add(ShaderType.Compute, Compiler.Reflect<ID3D11ShaderReflection>(il.Span));
        }
        
        InitConstantBuffer();
    }
    
    public void Use()
    {
        var rCmd = DxContext.Context;

        if (_isBaseShader)
        {
            if (_inputLayout != null)
                rCmd.IASetInputLayout(_inputLayout);
            
            rCmd.VSSetShader(_vertexShader);
            rCmd.PSSetShader(_pixelShader);
        }
        
        rCmd.HSSetShader(_hullShader);
        rCmd.DSSetShader(_domainShader);
        rCmd.GSSetShader(_geometryShader);
        rCmd.CSSetShader(_computeShader);
    }
    
    public void UploadConstantStruct<T>(string constantName, T value, ShaderType visibleShader = ShaderType.Vertex) where T : struct
        => UploadContextData(constantName, ref value, visibleShader);

    public void UploadConstantMat44(string constantName, Matrix4x4 mat, bool isTranspose, ShaderType visibleShader)
    {
        mat = isTranspose ? Matrix4x4.Transpose(mat) : mat;
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

    private void InitConstantBuffer()
    {
        foreach (var reflection in _shaderReflection)
        {
            Dictionary<string, ShaderConstant> constants = new();
            for (int i = 0; i < reflection.Value.ConstantBuffers.Length; i++)
            {
                ConstantBufferDescription info = reflection.Value.ConstantBuffers[i].Description;

                var constant = new ID3D11Buffer((nint)DxContext.ResourcesFactory.CreateConstantBuffer(info.Size));
                constants.Add(info.Name, new ShaderConstant((uint)i, constant));
            }
            _constantBuffers.Add(reflection.Key, constants);
        }
    }

    public static IShader Create(string shaderPath, VertexAttrib[]? vertexAttributes = null)
    {
        if (shaderPath.Contains(".hlsl"))
            return new DxShader(shaderPath, vertexAttributes);
        
        return new DxShader(shaderPath, true, vertexAttributes);
    }
}