using System.Drawing;
using System.Numerics;
using Pencils.RendererApi;
using Serilog;

namespace Pencils.SandBox;

public class SandBox2D : ISandBox
{
    private IGraphicsContext context;
    
    private Renderer2D _renderer = null!;
    private Camera _cameraData = null!;
    
    public SandBox2D(IGraphicsContext graphicsContext)
    {
        context = graphicsContext;
    }
    
    public void Init(int width, int height)
    {
        _renderer = new Renderer2D(context);
        _renderer.Init();
        
        _cameraData = new Camera(width, height)
        {
            Position = new Vector3(0, 0, -2)
        };
        _renderer.SetViewport(width, height, 1f);

        Log.Logger.Information("Initialized DxContext");
        
        _renderer.RCommand.DefaultPrimitiveTopology();
    }

    public void Renderer(float time)
    {
        _renderer.BeginScene(_cameraData.CameraMatrix);

        _renderer.DrawQuad(Vector2.Zero, Vector2.One, Color.DodgerBlue);

        _renderer.EndScene();
    }

    public void Update(float time)
    {
        
    }
}