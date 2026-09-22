using System.Numerics;
using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Serilog;
using Vortice.Mathematics;

namespace Pencils;

public record struct SceneData
{
    public Matrix4x4 ViewProjMat;
}

public class Renderer(IGraphicsContext gContext)
{
    protected IGraphicsContext gContext = gContext;
    
    private IRenderCommand _rCommand = new DxRenderCommand();
    private IMesh? _firstMesh;
    
    public Viewport _viewport;
    public IRenderCommand RCommand => _rCommand;
    
    private SceneData _sceneData;
    

    public void SetViewport(float width, float height, float depth)
    {
        _viewport = new Viewport(0, 0, width, height, 0f, depth);
        _rCommand.SetViewport(_viewport);
    }

    public virtual void BeginScene(Matrix4x4 camera)
    {
        _sceneData.ViewProjMat = camera;
    }
    
    public void Submit(IShader? shader, IMesh mesh, Matrix4x4 transform)
    {
        _firstMesh?.Unbind();

        if (shader == null)
        {
            Log.Logger.Error($"shader is null");
            return;
        }
        
        shader.Use();
        shader.UploadConstantMat44("MvpMat", transform * _sceneData.ViewProjMat);
        
        mesh.Bind();
        _firstMesh = mesh;
        
        if (mesh.IndexBuffer == null)
        {
            _rCommand.DrawVertex(mesh.VertexCount);
            return;
        }
        
        _rCommand.DrawIndexed(mesh.VertexCount);
    }
    
    public virtual void EndScene()
    {
        
    }

    public void Clear()
    {
        gContext.ClearBuffer();
    }
}