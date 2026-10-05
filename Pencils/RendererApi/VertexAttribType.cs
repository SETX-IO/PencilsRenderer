namespace Pencils.RendererApi;

public enum VertexAttribType
{
    Position2,
    Position3,
    Color3F,
    Color4F,
    Color4U,
    Normal,
    TexCoord,
}

public record VertexAttrib(VertexAttribType Type, uint Slot = 0);