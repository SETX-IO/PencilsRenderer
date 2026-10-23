using System.Numerics;
using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Serilog;
using Vortice;
using Vortice.Mathematics;

namespace Pencils;

public record struct SceneData
{
    public MvpData mvp;
}

public class Renderer(IGraphicsContext gContext)
{
    protected IGraphicsContext gContext = gContext;
    protected SceneData sceneData;
    
    private IRenderCommand _rCommand = new DxRenderCommand();
    private IMesh? _firstMesh;
    
    public Viewport _viewport;
    public IRenderCommand RCommand => _rCommand;
    
    
    public void SetViewport(float width, float height, float depth)
    {
        _viewport = new Viewport(0, 0, width, height, 0f, depth);
        _rCommand.SetViewport(_viewport);
    }

    public virtual void BeginScene(MvpData data, IFrameBuffer? renderTarget = null)
    {
        sceneData.mvp = data;
        
        if (renderTarget == null) return;
        
        renderTarget.Use();
        renderTarget.Clear();
    }
    
    public void Submit(IShader? shader, IMesh mesh, Matrix4x4 transform = default)
    {
        _firstMesh?.Unbind();

        if (shader == null)
        {
            Log.Logger.Error("shader is null");
            return;
        }
        
        sceneData.mvp.modelMatrix = transform == default ? Matrix4x4.Identity : transform;
        
        shader.Use();
        shader.UploadConstantStruct("MvpMat", sceneData.mvp);
        
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

    public void SetDefaultRenderState()
    {
        _rCommand.DefaultPrimitiveTopology();
        _rCommand.SetBlendState();
        _rCommand.SetDepthStStencilState();
        _rCommand.SetFillAndCull();
    }

    public void SetScissor(int left, int top, int right, int bottom)
    {
        RawRect scissor = new(left, top, right, bottom);
        _rCommand.SetScissor(scissor);
    }
}