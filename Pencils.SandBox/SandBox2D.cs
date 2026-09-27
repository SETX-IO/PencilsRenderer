using System.Drawing;
using System.Numerics;
using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Serilog;

namespace Pencils.SandBox;

public class SandBox2D(IGraphicsContext graphicsContext) : ISandBox
{
    private Renderer2D _renderer = null!;
    private Camera _cameraData = null!;
    private ITexture2D _checkerBoardTexture = null!;

    public void Init(int width, int height)
    {
        _renderer = new Renderer2D(graphicsContext);
        _renderer.Init();
        
        _cameraData = new Camera(width, height, CameraType.Orthographic)
        {
            Zoom = 2f,
            Position = Vector3.Create(0, 0, 0),
        };
        _renderer.SetViewport(width, height, 1f);

        Log.Logger.Information("Initialized DxContext");
        
        _checkerBoardTexture = DxTexture2D.Create("image/container.jpg");
        
        _renderer.RCommand.DefaultPrimitiveTopology();
    }

    public void Renderer(float time)
    {
        _renderer.BeginScene(_cameraData.CameraMatrix);

        _renderer.DrawQuad(-Vector2.UnitY * 0.55f, Vector2.One, Color.DodgerBlue);
        _renderer.DrawQuad(-Vector2.UnitX * 0.7f, Vector2.One * 0.5f, Color.Brown);
        // _renderer.DrawQuad(Vector2.Create(-0.7f, -0.55f), Vector2.One * 0.5f, Color.DimGray);
        _renderer.DrawQuad(Vector2.Create(-0.7f, -0.7f), Vector2.One, _checkerBoardTexture);

        _renderer.EndScene();
    }

    public void Update(float time)
    {
        
    }
}