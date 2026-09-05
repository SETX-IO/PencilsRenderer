using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Pencils;

public class Renderer
{
    private IGraphicsContext gContext;
    private ID3D11DeviceContext _context;
    private Viewport _viewport;
    
    public Renderer()
    {
        _context = DxContext.Context;
    }
    
    public void SetViewport(float width, float height, float depth)
    {
        _viewport = new Viewport(0, 0, width, height, 0f, depth);
    }

    public void BeginScene()
    {
        
    }
    
    public void Submit(IMesh mesh)
    {
        
    }
    
    public void EndScene()
    {
        
    }

    public void Clear()
    {
        gContext.ClearBuffer();
    }
}