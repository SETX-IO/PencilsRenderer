using System;
using System.Numerics;
using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Serilog;

namespace Pencils.SandBox;

public class SandBox3D : ISandBox
{
    private readonly IGraphicsContext graphicsContext;
    
    private Renderer _renderer = null!;
    private IMesh _mesh = null!;
    private IShaderLibrary _shaderLibrary = null!;
    private ITexture2D _texture = null!;
    
    private float _rotation;
    
    private Camera _cameraData = null!;
    
    public SandBox3D(IGraphicsContext context)
    {
        graphicsContext = context;
    }

    public void Init(int width, int height)
    {
        _renderer = new Renderer(graphicsContext);
        
        Vertex[] a = [
            new(new Vector3(-0.5f,  0.5f, 0), Vector3.One, new Vector3(Vector2.UnitY, 0)),
            new(new Vector3( 0.5f,  0.5f, 0), Vector3.One, new Vector3(Vector2.One, 0)),
            new(new Vector3( 0.5f, -0.5f, 0), Vector3.One, new Vector3(Vector2.UnitX, 0)),
            new(new Vector3(-0.5f, -0.5f, 0), Vector3.One, new Vector3(Vector2.Zero, 0)),
        ];

        ushort[] ii =
        [
            0, 1, 2,
            2, 3, 0
        ];
        
        _cameraData = new Camera(width, height)
        {
            Position = new Vector3(0, 0, -2)
        };
        
        _mesh = DxMesh.Create();
        var vertexBuffer = DxVertexBuffer.Create(a);
        vertexBuffer.SetVertexAttribs(VertexAttribType.Position3, VertexAttribType.Color3F);
        
        _mesh.AddVertexBuffer(vertexBuffer);
        _mesh.SetIndexBuffer(DxIndexBuffer.Create(ii));
        
        _shaderLibrary = DxShaderLibrary.Create();
        _shaderLibrary.Load("Shader/Texture.hlsl");

        _texture = DxTexture2D.Create("image/container.jpg");
    }

    public void Renderer(float deltaTime)
    {
        var textureShader = _shaderLibrary["Texture"];
        
        _renderer.RCommand.DefaultPrimitiveTopology();
        
        _renderer.SetViewport(800, 600, 1f);
        _renderer.SetScissor(0, 0, 800, 600);
        _renderer.RCommand.SetDepthStStencilState();
        _renderer.RCommand.SetBlendState();
        _renderer.RCommand.SetFillAndCull();
        
        _renderer.BeginScene(_cameraData.CameraMatrix);
        
        _texture.Bind();
        _renderer.Submit(textureShader, _mesh, Matrix4x4.CreateRotationY(MathF.Min(MathF.Cos(_rotation), 15f)));
        _texture.Unbind();
        
        _renderer.EndScene();
    }
    
    public void Update(float deltaTime)
    {
        _rotation += deltaTime * 2;
    }
}