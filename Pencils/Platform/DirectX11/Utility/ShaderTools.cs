using System;
using Pencils.RendererApi;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Direct3D11.Shader;
using Vortice.DXGI;

namespace Pencils.Platform.DirectX11.Utility;

public class ShaderConstant(uint slot, ID3D11Buffer buffer)
{
    public readonly uint slot = slot;
    public readonly ID3D11Buffer constantBuffer = buffer;
}

public class ShaderTools
{
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
    
    public static ID3D11InputLayout ConfigVertexAttrib(ReadOnlyMemory<byte> vsShaderByte, ID3D11ShaderReflection reflection)
    {
        InputElementDescription[] inputElements = new InputElementDescription[reflection.InputParameters.Length];

        uint offset = 0;
        uint slot = 0;
        for (int i = 0; i < inputElements.Length; i++)
        {
            ref InputElementDescription refInputElement = ref inputElements[i];
            var inputParameter = reflection.InputParameters[i];

            if (slot != inputParameter.Stream)
                offset = 0;

            uint paddingCount = inputParameter.UsageMask switch
            {
                RegisterComponentMaskFlags.ComponentX => 1,
                RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY => 2,
                RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ => 3,
                RegisterComponentMaskFlags.All => 4,
                _ => throw new ArgumentOutOfRangeException()
            };

            (Format format, uint padding) = (inputParameter.ComponentType , paddingCount) switch
            {
                (RegisterComponentType.Float32, 1) => (Format.R32_Float, 4u),
                (RegisterComponentType.Float32, 2) => (Format.R32G32_Float, 8u),
                (RegisterComponentType.Float32, 3) => (Format.R32G32B32_Float, 12u),
                (RegisterComponentType.Float32, 4) => (Format.R32G32B32A32_Float, 16u),
                
                (RegisterComponentType.Float16, 1) => (Format.R16_Float, 2u),
                (RegisterComponentType.Float16, 2) => (Format.R16G16_Float, 4u),
                // (RegisterComponentType.Float16, 3) => (Format.R16G16B16A16_Float, 6u),
                (RegisterComponentType.Float16, 4) => (Format.R16G16B16A16_Float, 8u),
                
                (RegisterComponentType.UInt32, 1) => (Format.R32_UInt, 4u),
                (RegisterComponentType.UInt32, 2) => (Format.R32G32_UInt, 8u),
                (RegisterComponentType.UInt32, 3) => (Format.R32G32B32_UInt, 12u),
                (RegisterComponentType.UInt32, 4) => (Format.R32G32B32A32_UInt, 16u),
                
                (RegisterComponentType.UInt16, 1) => (Format.R16_UInt, 2u),
                (RegisterComponentType.UInt16, 2) => (Format.R16G16_UInt, 4u),
                // (RegisterComponentType.UInt16, 3) => (Format.R16G16B16_UInt, 6u),
                (RegisterComponentType.UInt16, 4) => (Format.R16G16B16A16_UInt, 8u),
                
                (RegisterComponentType.SInt32, 1) => (Format.R32_SInt, 4u),
                (RegisterComponentType.SInt32, 2) => (Format.R32G32_SInt, 8u),
                (RegisterComponentType.SInt32, 3) => (Format.R32G32B32_SInt, 12u),
                (RegisterComponentType.SInt32, 4) => (Format.R32G32B32A32_SInt, 16u),
                
                (RegisterComponentType.SInt16, 1) => (Format.R16_SInt, 2u),
                (RegisterComponentType.SInt16, 2) => (Format.R16G16_SInt, 4u),
                // (RegisterComponentType.SInt16, 3) => (Format.R32G32B32_Float, 6u),
                (RegisterComponentType.SInt16, 4) => (Format.R32G32B32A32_SInt, 8u),
            };

            refInputElement = new InputElementDescription(inputParameter.SemanticName, inputParameter.SemanticIndex, format, offset, inputParameter.Stream);
            offset += padding;
            slot = inputParameter.Stream;
        }
    
        ID3D11InputLayout inputLayout = DxContext.Context.Device.CreateInputLayout(inputElements, vsShaderByte.Span);
        
        return inputLayout;
    }

    public static ID3D11InputLayout ConfigVertexAttrib(ReadOnlyMemory<byte> vsShaderByte,
        VertexAttrib[] vertexAttributes)
    {
        InputElementDescription[] inputElements = new InputElementDescription[vertexAttributes.Length];

        uint offset = 0;
        uint slot = 0;
        for (int i = 0; i < inputElements.Length; i++)
        {
            ref InputElementDescription element = ref inputElements[i];
            VertexAttrib vertexAttrib = vertexAttributes[i];
            
            if (slot != vertexAttrib.Slot)
                offset = 0;
            
            (string name, Format format, uint padding) = vertexAttrib.Type switch {
                VertexAttribType.Position2 => ("POSITION", Format.R32G32_Float, 8u),
                VertexAttribType.Position3 => ("POSITION", Format.R32G32B32_Float, 12u),
                VertexAttribType.Color3F => ("COLOR", Format.R32G32B32_Float, 12u),
                VertexAttribType.Color4F => ("COLOR", Format.R32G32B32A32_Float, 16u),
                VertexAttribType.Color4U => ("COLOR", Format.R8G8B8A8_UNorm, 8u),
                VertexAttribType.Normal => ("NORMAL", Format.R32G32B32A32_Float, 16u),
                VertexAttribType.TexCoord => ("TEXCOORD", Format.R32G32_Float, 8u),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            element = new InputElementDescription(name, 0, format, offset, vertexAttrib.Slot);

            offset += padding;
            slot = vertexAttrib.Slot;
        }
        
        ID3D11InputLayout inputLayout = DxContext.Context.Device.CreateInputLayout(inputElements, vsShaderByte.Span);
        return inputLayout;
    }
}