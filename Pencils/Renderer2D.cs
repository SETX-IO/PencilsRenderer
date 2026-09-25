using System.Numerics;
using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Vortice.Mathematics;
using Color = System.Drawing.Color;

namespace Pencils;

public struct Renderer2DStorage
{ 
    public IMesh quadMesh;
    public IShader textureShader;
    public ITexture2D whiteTexture;
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
        
        _data.textureShader = DxShader.Create(_context.ResourcesFactory, "Shader/Texture.hlsl");
        _data.whiteTexture = DxTexture2D.Create(_context.ResourcesFactory, 1, 1);
        _data.whiteTexture.SetData([0xff, 0xff, 0xff, 0xff]);
    }

    public override void BeginScene(Matrix4x4 cameraMat)
    {
        _data.textureShader.Use();
        _data.textureShader.UploadConstantMat44("MvpMat", cameraMat);
    }

    public override void EndScene()
    {
        _data.quadMesh.Unbind();
        _data.quadMesh.Unbind();
    }
    
    public void DrawQuad(Vector2 position,  Vector2 size, Color color)
        => DrawQuad(Vector3.Create(position, 0f), size, color);
    
    public void DrawQuad(Vector2 position,  Vector2 size, ITexture2D texture, float tilingFactor = 1f)
        => DrawQuad(Vector3.Create(position, 0f), size, texture, tilingFactor);
    
    public void DrawQuad(Vector3 position,  Vector2 size, Color color)
    {
        Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 1f)) * Matrix4x4.CreateTranslation(position);
        _data.textureShader.UploadConstantFloat3("Render2DData", new Vector3(color.R / 255f, color.G / 255f, color.B / 255f), ShaderType.Pixel);
        _data.textureShader.UploadConstantMat44("Transform", transform);
        
        _data.whiteTexture.Bind(0);
        _data.quadMesh.Bind();
        RCommand.DrawIndexed(_data.quadMesh.VertexCount);
    }
    
    public void DrawQuad(Vector3 position,  Vector2 size, ITexture2D texture, float tilingFactor)
    {
        Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 1f)) * Matrix4x4.CreateTranslation(position);
        _data.textureShader.UploadConstantMat44("Transform", transform);
        _data.textureShader.UploadConstantFloat("TextureInfo", tilingFactor, ShaderType.Pixel);
        _data.textureShader.UploadConstantFloat3("Render2DData", Vector3.One, ShaderType.Pixel);
        
        texture.Bind(0);
        _data.quadMesh.Bind();
        RCommand.DrawIndexed(_data.quadMesh.VertexCount);
    }
}