using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using SharpGen.Runtime;
using Vortice.Mathematics;
using Color = System.Drawing.Color;

namespace Pencils;

public struct Renderer2DStorage
{
    public const uint MaxQuad = 1000;
    public const uint MaxVertices = MaxQuad * 4;
    public const uint MaxIndices = MaxQuad * 6;
    
    public IMesh quadMesh;
    public IVertexBuffer quadVertexBuffer;
    public IIndexBuffer quadIndexBuffer;
    public IShader textureShader;
    public ITexture2D whiteTexture;
    
    public ITexture2DArray textureArray;

    public nint vertexBuffer;
    public uint vertexCount;
}

public class Renderer2D(IGraphicsContext context) : Renderer(context)
{
    private Renderer2DStorage _data;
    private IGraphicsContext _context = context;
    
    public void Init()
    {
        int offset = 0;
        ushort[] quadi = new  ushort[Renderer2DStorage.MaxIndices];
        for (int i = 0; i < quadi.Length; i += 6)
        {
            quadi[i + 0] = (ushort)(offset + 0);
            quadi[i + 1] = (ushort)(offset + 3);
            quadi[i + 2] = (ushort)(offset + 2);
            
            quadi[i + 3] = (ushort)(offset + 2);
            quadi[i + 4] = (ushort)(offset + 1);
            quadi[i + 5] = (ushort)(offset + 0);
            
            offset += 4;
        }

        _data.textureArray = new DxTexture2DArray(1024, 1024);
        
        _data.quadMesh = DxMesh.Create();
        
        _data.quadVertexBuffer = DxVertexBuffer.Create<Vertex>(Renderer2DStorage.MaxVertices);
        _data.quadVertexBuffer.SetVertexAttribs(VertexAttribType.Position3, VertexAttribType.TexCoord);
        _data.quadMesh.AddVertexBuffer(_data.quadVertexBuffer);

        _data.quadIndexBuffer = DxIndexBuffer.Create(quadi);
        _data.quadMesh.SetIndexBuffer(_data.quadIndexBuffer);
        
        _data.textureShader = DxShader.Create("Shader/Texture.hlsl");
        _data.whiteTexture = DxTexture2D.Create(1, 1);
        _data.whiteTexture.SetData([0xff, 0xff, 0xff, 0xff]);
        
        _data.textureArray.AddTexture(_data.whiteTexture);
    }

    public override void BeginScene(Matrix4x4 cameraMat)
    {
        _data.textureShader.Use();
        _data.textureShader.UploadConstantMat44("MvpMat", cameraMat);

        _data.vertexBuffer = _data.quadVertexBuffer.Map<Vertex>();
        _data.vertexCount = 0;
    }

    public override void EndScene()
    {
        _data.quadVertexBuffer.CloseMap();
        
        _data.quadMesh.Bind();
        Flush();
    }

    public void Flush()
    {
        _data.textureArray.Bind();
        
        RCommand.DrawIndexed(_data.vertexCount);
        _data.quadMesh.Unbind();
    }
    
    public void DrawQuad(Vector2 position,  Vector2 size, Color color)
        => DrawQuad(Vector3.Create(position, 0f), size, color);
    public void DrawQuad(Vector2 position,  Vector2 size, ITexture2D texture, float tilingFactor = 1f, Color color = default)
        => DrawQuad(Vector3.Create(position, 0f), size, texture, tilingFactor, color);
    public void DrawRotatedQuad(Vector2 position,  Vector2 size, float rotation, Color color)
        => DrawRotatedQuad(Vector3.Create(position, 0f), size, rotation, color);
    public void DrawRotatedQuad(Vector2 position,  Vector2 size, float rotation, ITexture2D texture, float tilingFactor = 1f, Color color = default)
        => DrawRotatedQuad(Vector3.Create(position, 0f), size, rotation, texture, tilingFactor, color);
    
    public unsafe void DrawQuad(Vector3 position, Vector2 size, Color color)
    {
        var colorVector = new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
        float uScale = 1f / _data.textureArray.Width;
        float vScale = 1f / _data.textureArray.Height;
        Vector2 uvScale = new Vector2(uScale, vScale);
        
        Vertex[] vertices = 
        [
            new (position, colorVector, new Vector3(Vector2.UnitY * uvScale, 0f)),
            new (new Vector3(position.X + size.X, position.Y, 0), colorVector, new Vector3(Vector2.One  * uvScale, 0f)),
            new (new Vector3(position.X + size.X, position.Y + size.Y, 0), colorVector, new Vector3(Vector2.UnitX  * uvScale, 0f)),
            new (new Vector3(position.X, position.Y + size.Y, 0), colorVector, new Vector3(Vector2.Zero * uvScale, 0f))
        ];

        Unsafe.CopyBlock((void*)_data.vertexBuffer, vertices.GetPointerUnsafe(), (uint)(Unsafe.SizeOf<Vertex>() * 4));

        _data.vertexBuffer += Unsafe.SizeOf<Vertex>() * 4;
        _data.vertexCount += 6;
    }
    
    public unsafe void DrawQuad(Vector3 position, Vector2 size, ITexture2D texture, float tilingFactor, Color color)
    {
        var colorVector = color == default ? Vector3.One : new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
        float uScale = texture.Width / (float)_data.textureArray.Width;
        float vScale = texture.Height / (float)_data.textureArray.Height;
        Vector2 uvScale = new Vector2(uScale, vScale);
        
        Vertex[] vertices = 
        [
            new (position, colorVector, new Vector3(Vector2.UnitY * uvScale, 1f)),
            new (new Vector3(position.X + size.X, position.Y, 0), colorVector, new Vector3(Vector2.One * uvScale, 1f)),
            new (new Vector3(position.X + size.X, position.Y + size.Y, 0), colorVector, new Vector3(Vector2.UnitX * uvScale, 1f)),
            new (new Vector3(position.X, position.Y + size.Y, 0), colorVector, new Vector3(Vector2.Zero * uvScale, 1f))
        ];
        
        Unsafe.CopyBlock((void*)_data.vertexBuffer, vertices.GetPointerUnsafe(), (uint)(Unsafe.SizeOf<Vertex>() * 4));
        
        _data.vertexBuffer += Unsafe.SizeOf<Vertex>() * 4;
        _data.vertexCount += 6;
        
        _data.textureArray.AddTexture(texture);
    }

    public void DrawRotatedQuad(Vector3 position, Vector2 size, float rotation, Color color)
    {
        Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 1f)) * Matrix4x4.CreateTranslation(position) * Matrix4x4.CreateRotationZ(rotation);
        
        _data.textureShader.UploadConstantFloat3("Render2DData", new Vector3(color.R / 255f, color.G / 255f, color.B / 255f), ShaderType.Pixel);
        _data.textureShader.UploadConstantMat44("Transform", transform);
        
        _data.whiteTexture.Bind();
        _data.quadMesh.Bind();
        RCommand.DrawIndexed(_data.quadMesh.VertexCount);
    }

    public void DrawRotatedQuad(Vector3 position, Vector2 size, float rotation, ITexture2D texture, float tilingFactor, Color color)
    {
        Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(size, 1f)) * Matrix4x4.CreateTranslation(position) * Matrix4x4.CreateRotationZ(rotation);
        var colorVector = color == default ? Vector3.One : new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
        
        _data.textureShader.UploadConstantMat44("Transform", transform);
        _data.textureShader.UploadConstantFloat("TextureInfo", tilingFactor, ShaderType.Pixel);
        _data.textureShader.UploadConstantFloat3("Render2DData", colorVector, ShaderType.Pixel);
        
        texture.Bind();
        _data.quadMesh.Bind();
        RCommand.DrawIndexed(_data.quadMesh.VertexCount);
    }
}