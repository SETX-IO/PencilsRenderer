using System.Drawing;

namespace Pencils.RendererApi;

public interface IFrameBuffer : ITexture
{
    bool IsClearColor { get; }
    
    Color ClearColor { get; }
    
    void Use();
    void Clear();
    
    void ReSize(uint width, uint height);
}