using System.Numerics;

namespace Pencils;

public struct Vertex
{
    public Vector3 position;
    public Vector3 color;
    public Vector3 texCoord;

    public Vertex(Vector3 position, Vector3 color, Vector3 TexCoord)
    {
        this.position = position;
        this.color = color;
        this.texCoord = TexCoord;
    }
}
