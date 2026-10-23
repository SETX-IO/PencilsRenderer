using System.Numerics;
using Pencils.Platform.DirectX11;
using Pencils.RendererApi;

namespace Pencils.SandBox.Sample;

public class BlackHole(Renderer renderer) : ISandBox
{
    private ITexture _skyBoxTexture;
    private Renderer _renderer = renderer;
    private IFrameBuffer _frameBuffer;
    private IShader _shader;
    private IShader _postProessShader;
    private Camera _cameraData = null!;
    private IMesh _mesh;

    private float _rotation;

    public void Init(int width, int height)
    {
        _skyBoxTexture = DxTextureCube.Create("image/TestSkyBox");

        Vertex[] cube =
        [
            // +Z
            new(new Vector3(-1f, 1f, -1f), Vector3.One, new Vector3(Vector2.UnitY, 4)),
            new(new Vector3(1f, 1f, -1f), Vector3.One, new Vector3(Vector2.One, 4)),
            new(new Vector3(1f, -1f, -1f), Vector3.One, new Vector3(Vector2.UnitX, 4)),
            new(new Vector3(-1f, -1f, -1f), Vector3.One, new Vector3(Vector2.Zero, 4)),

            // +Y
            new(new Vector3(-1f, 1f, 1f), Vector3.One, new Vector3(Vector2.UnitY, 2)),
            new(new Vector3(1f, 1f, 1f), Vector3.One, new Vector3(Vector2.One, 2)),
            new(new Vector3(1f, 1f, -1f), Vector3.One, new Vector3(Vector2.UnitX, 2)),
            new(new Vector3(-1f, 1f, -1f), Vector3.One, new Vector3(Vector2.Zero, 2)),

            // +X
            new(new Vector3(1f, 1f, -1f), Vector3.One, new Vector3(Vector2.UnitY, 0)),
            new(new Vector3(1f, 1f, 1f), Vector3.One, new Vector3(Vector2.One, 0)),
            new(new Vector3(1f, -1f, 1f), Vector3.One, new Vector3(Vector2.UnitX, 0)),
            new(new Vector3(1f, -1f, -1f), Vector3.One, new Vector3(Vector2.Zero, 0)),

            // -Z
            new(new Vector3(1f, 1f, 1f), Vector3.One, new Vector3(Vector2.UnitY, 5)),
            new(new Vector3(-1f, 1f, 1f), Vector3.One, new Vector3(Vector2.One, 5)),
            new(new Vector3(-1f, -1f, 1f), Vector3.One, new Vector3(Vector2.UnitX, 5)),
            new(new Vector3(1f, -1f, 1f), Vector3.One, new Vector3(Vector2.Zero, 5)),

            // -Y
            new(new Vector3(1f, -1f, 1f), Vector3.One, new Vector3(Vector2.UnitY, 3)),
            new(new Vector3(-1f, -1f, 1f), Vector3.One, new Vector3(Vector2.One, 3)),
            new(new Vector3(-1f, -1f, -1f), Vector3.One, new Vector3(Vector2.UnitX, 3)),
            new(new Vector3(1f, -1f, -1f), Vector3.One, new Vector3(Vector2.Zero, 3)),

            // -X
            new(new Vector3(-1f, 1f, -1f), Vector3.One, new Vector3(Vector2.UnitY, 1)),
            new(new Vector3(-1f, -1f, -1f), Vector3.One, new Vector3(Vector2.One, 1)),
            new(new Vector3(-1f, -1f, 1f), Vector3.One, new Vector3(Vector2.UnitX, 1)),
            new(new Vector3(-1f, 1f, 1f), Vector3.One, new Vector3(Vector2.Zero, 1)),
        ];

        ushort[] ii =
        [
            0, 1, 2, 2, 3, 0,
            4, 5, 6, 6, 7, 4,
            8, 9, 10, 10, 11, 8,
            12, 13, 14, 14, 15, 12,
            16, 17, 18, 18, 19, 16,
            20, 21, 22, 22, 23, 20
        ];

        _mesh = DxMesh.Create();
        _mesh.AddVertexBuffer(DxVertexBuffer.Create(cube));
        _mesh.SetIndexBuffer(DxIndexBuffer.Create(ii));

        _postProessShader = DxShader.Create("Shader/PostProcessTest.hlsl");
        _shader = DxShader.Create("Shader/SkyBox.hlsl");
        _frameBuffer = new DxFrameBuffer(800, 600);

        _cameraData = new Camera(width, height);
    }

    public void Renderer(float time)
    {
        DepthStencilInfo depthStencilInfo = new()
        {
            depthFunc = ComparisonFunc.LessEqual
        };

        _renderer.SetViewport(800, 600, 1f);
        _renderer.SetScissor(0, 0, 800, 600);

        _renderer.RCommand.DefaultPrimitiveTopology();
        _renderer.RCommand.SetDepthStStencilState(depthStencilInfo);
        _renderer.RCommand.SetBlendState();
        _renderer.RCommand.SetFillAndCull(cullMode: Cull.Off);

        _postProessShader.Use();
        _frameBuffer.Bind();
        _renderer.RCommand.DrawVertex(3);
        _frameBuffer.Unbind();
        
        _renderer.BeginScene(_cameraData.SceneData, _frameBuffer);
        
        _skyBoxTexture.Bind();
        _renderer.Submit(_shader, _mesh, Matrix4x4.CreateRotationY(_rotation));
        
        _renderer.EndScene();
    }

    public void Update(float time)
    {
        _rotation += time * 0.4f;

        // _cameraData.Position = new Vector3(MathF.Cos(_rotation), 0, MathF.Sin(_rotation));
    }
}