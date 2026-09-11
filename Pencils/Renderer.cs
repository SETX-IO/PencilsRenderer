using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Vortice.Mathematics;

namespace Pencils;

public class Renderer(IGraphicsContext gContext)
{
    private IRenderCommand _rCommand = new DxRenderCommand();
    private IMesh? _firstMesh;
    
    public Viewport _viewport;
    public IRenderCommand RCommand => _rCommand;
    

    public void SetViewport(float width, float height, float depth)
    {
        _viewport = new Viewport(0, 0, width, height, 0f, depth);
        _rCommand.SetViewport(_viewport);
    }

    public void BeginScene()
    {
        
    }
    
    public void Submit(IMesh mesh)
    {
        _firstMesh?.Unbind();
        
        mesh.Bind();
        _firstMesh = mesh;
        
        if (mesh.IndexBuffer == null)
        {
            _rCommand.DrawVertex(mesh.VertexCount);
            return;
        }
        
        _rCommand.DrawIndexed(mesh.IndexBuffer.Count);
    }
    
    public void EndScene()
    {
        
    }

    public void Clear()
    {
        gContext.ClearBuffer();
    }
}