using System.Numerics;
using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Vortice.Mathematics;
using Color = System.Drawing.Color;

namespace Pencils;

public struct Renderer2DStorage
{ 
    public IMesh quadMesh;
    public IShader flatColorShader;
}

public struct Renderer2DData
{
    public Color3 color;
}

public class Renderer2D(IGraphicsContext context) : Renderer(context)
{
    private Renderer2DStorage _data;
    private IGraphicsContext _context = context;
    
    public void Init()
    {
        Vertex[] a = [
            new(new Vector3(-0.5f,  0.5f, 0), new Vector3(0, 1, 0)),
            new(new Vector3( 0.5f,  0.5f, 0), new Vector3(1, 1, 0)),
            new(new Vector3( 0.5f, -0.5f, 0), new Vector3(1, 0, 0)),
            new(new Vector3(-0.5f, -0.5f, 0), new Vector3(0, 0, 0)),
        ];
        
        ushort[] ii =
        [
            0, 1, 2,
            2, 3, 0
        ];
        
        _data.quadMesh = DxMesh.Create();
        _data.quadMesh.AddVertexBuffer(DxVertexBuffer.Create(context, a));
        _data.quadMesh.VertexBuffers[0].SetVertexAttribs(VertexAttribType.Position3);
        _data.quadMesh.SetIndexBuffer(DxIndexBuffer.Create(context, ii));
        
        _data.flatColorShader = DxShader.Create(_context.ResourcesFactory, "Shader/BaseColor.hlsl");
        _data.flatColorShader.SetVertexAttrib(_data.quadMesh.VertexAttribs);
    }

    public override void BeginScene(Matrix4x4 cameraMat)
    {
        _data.flatColorShader.Use();
        _data.flatColorShader.UploadConstantMat44("MvpMat" ,cameraMat);
    }

    public override void EndScene()
    {
        _data.quadMesh.Unbind();
    }
    
    public void DrawQuad(Vector2 position,  Vector2 size, Color color)
    {
        DrawQuad(new Vector3(position, 0f), new Vector2(size.X, size.Y), color);
    }
    
    public void DrawQuad(Vector3 position,  Vector2 size, Color color)
    {
        _data.flatColorShader.UploadConstantFloat3("Render2DData", new Vector3(color.R / 255f, color.G / 255f, color.B / 255f), ShaderType.Pixel);
        _data.quadMesh.Bind();
        RCommand.DrawIndexed(_data.quadMesh.VertexCount);
    }
}