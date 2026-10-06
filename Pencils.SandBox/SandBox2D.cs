using System.Drawing;
using System.Numerics;
using Hexa.NET.ImGui;
using Pencils.Extension;
using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Serilog;

namespace Pencils.SandBox;

public class SandBox2D(IGraphicsContext graphicsContext) : ISandBox
{
    private Renderer2D _renderer = null!;
    private Camera _cameraData = null!;
    private ITexture2D _checkerBoardTexture = null!;
    private ImGuiRenderer _imGui = null!;

    private int _width;
    private int _height;

    public void Init(int width, int height)
    {
        _width = width;
        _height = height;
        
        _renderer = new Renderer2D(graphicsContext);
        _renderer.Init();
        
        _cameraData = new Camera(width, height, CameraType.Orthographic)
        {
            Zoom = 2f,
        };
        
        _checkerBoardTexture = DxTexture2D.Create("image/container.jpg");

        _imGui = new ImGuiRenderer(_renderer);

        _imGui.ImGuiGenGuiEvent += () =>
        {
            ImGui.ShowDemoWindow();
        };
    }

    public void Renderer(float time)
    {
        _renderer.RCommand.DefaultPrimitiveTopology();
        _renderer.SetViewport(_width, _height, 1f);
        _renderer.SetScissor(0, 0, _width, _height);
        
        _renderer.RCommand.SetFillAndCull();
        _renderer.RCommand.SetBlendState();
        _renderer.RCommand.SetDepthStStencilState();
        
        _renderer.BeginScene(_cameraData.CameraMatrix);
        
        _renderer.DrawRotatedQuad(Vector2.UnitX * 0.3f, Vector2.One * 0.5f, 45f, Color.Brown);
        _renderer.DrawQuad(Vector2.UnitY * 0.55f, Vector2.One, Color.DodgerBlue);
        _renderer.DrawQuad(Vector2.Create(0, -0.5f), Vector2.One, _checkerBoardTexture);

        _renderer.EndScene();
        
        _imGui.Render(time);
    }

    public void Update(float time)
    {
        
    }
}