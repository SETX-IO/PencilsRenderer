using Pencils.RendererApi;
using Vortice.Direct3D11;
using Vortice.D3DCompiler;
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

    private ReadOnlyMemory<byte> _vsBytes;
    
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
                
                il = Compile(ShaderType.Pixel, shaderCode);
                _pixelShader = new ID3D11PixelShader((nint)factory.CreateShader(ShaderType.Pixel, il.Span));
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
            _hullShader = new ID3D11HullShader((nint)factory.CreateShader(ShaderType.Hull, il.Span));
        }
        
        if (shaderCode.Contains("geometry"))
        {
            var il = Compile(ShaderType.Hull, shaderCode);
            _hullShader = new ID3D11HullShader((nint)factory.CreateShader(ShaderType.Hull, il.Span));
        }
        
        if (shaderCode.Contains("geometry"))
        {
            var il = Compile(ShaderType.Hull, shaderCode);
            _hullShader = new ID3D11HullShader((nint)factory.CreateShader(ShaderType.Hull, il.Span));
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

    public void UnUse()
    {
        
    }

    public void SetVertexAttrib(List<VertexAttrib> vertexAttribs)
    {
        InputElementDescription[] inputElements = new InputElementDescription[vertexAttribs.Count];

        uint slot = 0;
        uint offset = 0;
        for (int i = 0; i < inputElements.Length; i++)
        {
            var vertexAttrib = vertexAttribs[i];

            if (slot != vertexAttrib.Slot)
                offset = 0;
            
            (string name, Format format, uint offset) attrib;
            switch (vertexAttrib.Type)
            {
                case VertexAttribType.Position2:
                    attrib = ("POSITION", Format.R32G32_Float ,offset);
                    offset += 8;
                    break;
                case VertexAttribType.Position3:
                    attrib = ("POSITION", Format.R32G32B32_Float ,offset);
                    offset += 12;
                    break;
                case VertexAttribType.Color3:
                    attrib = ("COLOR", Format.R32G32B32_Float ,offset);
                    offset += 12;
                    break;
                case VertexAttribType.Color4:
                    attrib = ("COLOR", Format.R32G32B32A32_Float ,offset);
                    offset += 16;
                    break;
                case VertexAttribType.Normal:
                    attrib = ("NORMAL", Format.R32G32B32A32_Float ,offset);
                    offset += 16;
                    break;
                case VertexAttribType.TexCoord:
                    attrib = ("TEXCOORD", Format.R32G32_Float ,offset);
                    offset += 8;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            slot = vertexAttrib.Slot;
            inputElements[i] = new InputElementDescription(attrib.name, 0, attrib.format, attrib.offset, vertexAttrib.Slot);
        }
        
        _inputLayout = DxContext.Context.Device.CreateInputLayout(inputElements, _vsBytes.Span);
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