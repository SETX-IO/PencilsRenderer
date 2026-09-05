namespace Pencils.RendererApi;

public enum VertexAttribType
{
    Position2,
    Position3,
    Color3,
    Color4,
    Normal,
    TexCoord,
}

public record VertexAttrib(VertexAttribType Type, uint Slot);