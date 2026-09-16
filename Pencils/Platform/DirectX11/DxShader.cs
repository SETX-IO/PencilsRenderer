using System.Numerics;
using System.Runtime.CompilerServices;
using Pencils.RendererApi;
using Vortice.Direct3D11;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11.Shader;
using Vortice.DXGI;

namespace Pencils.Platform.DirectX11;

public class DxShader : IShader
{
    // baseShader
    private ID3D11VertexShader? _vertexShader;
    private ID3D11PixelShader? _pixelShader;
    
    private ID3D11HullShader? _hullShader;
    private ID3D11DomainShader? _domainShader;
    private ID3D11GeometryShader? _geometryShader;
    private ID3D11ComputeShader? _computeShader;

    private bool _isBaseShader;
    private ID3D11InputLayout? _inputLayout;

    private readonly ReadOnlyMemory<byte> _vsBytes;
    private readonly ID3D11ShaderReflection _vsReflection;
    private Dictionary<string, (uint slot, ID3D11Buffer buffer)> _constantBuffer = new();
    
    private DxShader(IResourcesFactory factory, string shaderCode, bool isBaseShader)
    {
        _isBaseShader = isBaseShader;
        
        switch (isBaseShader)
        {
            case true when !shaderCode.Contains("vert") && !shaderCode.Contains("frag"):
                throw new ArgumentException(shaderCode);
            case true:
            {
                var il = Compile(ShaderType.Vertex, shaderCode);
                _vertexShader = new ID3D11VertexShader((nint)factory.CreateShader(ShaderType.Vertex, il.Span));
                _vsBytes = il;
                
                _vsReflection = Compiler.Reflect<ID3D11ShaderReflection>(_vsBytes.Span);
                
                il = Compile(ShaderType.Pixel, shaderCode);
                _pixelShader = new ID3D11PixelShader((nint)factory.CreateShader(ShaderType.Pixel, il.Span));

                for (int i = 0; i < _vsReflection.ConstantBuffers.Length; i++)
                {
                    ConstantBufferDescription info = _vsReflection.ConstantBuffers[i].Description;

                    var constant = DxContext.Context.Device.CreateBuffer(new BufferDescription(info.Size, BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));
                    _constantBuffer.Add(info.Name, ((uint)i, constant));
                }
                
                break;
            }
        }

        if (shaderCode.Contains("hull"))
        {
            var il = Compile(ShaderType.Hull, shaderCode);
            _hullShader = new ID3D11HullShader((nint)factory.CreateShader(ShaderType.Hull, il.Span));
        }
        
        if (shaderCode.Contains("domain"))
        {
            var il = Compile(ShaderType.Hull, shaderCode);
            _domainShader = new ID3D11DomainShader((nint)factory.CreateShader(ShaderType.Domain, il.Span));
        }
        
        if (shaderCode.Contains("geometry"))
        {
            var il = Compile(ShaderType.Hull, shaderCode);
            _geometryShader = new ID3D11GeometryShader((nint)factory.CreateShader(ShaderType.Geometry, il.Span));
        }
        
        if (shaderCode.Contains("compute"))
        {
            var il = Compile(ShaderType.Hull, shaderCode);
            _computeShader = new ID3D11ComputeShader((nint)factory.CreateShader(ShaderType.Compute, il.Span));
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

    public void SetVertexAttrib(List<VertexAttrib> vertexAttribs)
    {
        InputElementDescription[] inputElements = new InputElementDescription[_vsReflection.InputParameters.Length];

        uint offset = 0;
        uint slot = 0;
        for (int i = 0; i < inputElements.Length; i++)
        {
            ref InputElementDescription refInputElement = ref inputElements[i];
            var inputParameter = _vsReflection.InputParameters[i];

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
    
        _inputLayout = DxContext.Context.Device.CreateInputLayout(inputElements, _vsBytes.Span);
    }

    public void UploadConstantMat44(string constantName, Matrix4x4 mat, ShaderType visibleShader)
    {
        mat = Matrix4x4.Transpose(mat);
        UploadContextData(constantName, ref mat, visibleShader);
    }

    public void UploadConstantFloat3(string constantName, Vector3 vec3, ShaderType visibleShader = ShaderType.Vertex) =>
        UploadContextData(constantName, ref vec3, visibleShader);

    private unsafe void UploadContextData<T>(string constantName, ref T data, ShaderType visibleShader) where T : struct
    {
        var ctx = DxContext.Context;
        
        if (!_constantBuffer.TryGetValue(constantName, out var constantBuffer))
            return;
        
        switch (visibleShader)
        {
            case ShaderType.Vertex:
                ctx.VSSetConstantBuffer(constantBuffer.slot, constantBuffer.buffer);
                break;
            case ShaderType.Pixel:
                ctx.PSSetConstantBuffer(constantBuffer.slot, constantBuffer.buffer);
                break;
            case ShaderType.Hull:
                ctx.HSSetConstantBuffer(constantBuffer.slot, constantBuffer.buffer);
                break;
            case ShaderType.Domain:
                ctx.DSSetConstantBuffer(constantBuffer.slot, constantBuffer.buffer);
                break;
            case ShaderType.Geometry:
                ctx.GSSetConstantBuffer(constantBuffer.slot, constantBuffer.buffer);
                break;
            case ShaderType.Compute:
                ctx.CSSetConstantBuffer(constantBuffer.slot, constantBuffer.buffer);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(visibleShader), visibleShader, null);
        }
        
        var dataPtr = ctx.Map(constantBuffer.buffer, MapMode.WriteDiscard).DataPointer;
        
        Unsafe.Copy((void*)dataPtr, ref data);
    }

    public static DxShader Create(IGraphicsContext context, string shaderCode, bool isBaseShader = true) =>
        new(context.ResourcesFactory, shaderCode, isBaseShader);

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
}