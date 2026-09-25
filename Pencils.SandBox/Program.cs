using System.Collections.Generic;
using System.Drawing;
using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Serilog;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Pencils.SandBox;

class Program
{
    static void Main(string[] args)
    {
        App app = new App(args, "Pencils SandBox", 800, 600);
        app.Run();
    }
}

public class App
{
    private readonly IWindow _window;
    private IGraphicsContext rendererContext;
    private List<ISandBox> _sandBoxs;
    
    public App(string[] args, string title, int width, int height)
    {
        const string logTemplate =
                "{SourceContext} {Scope} {Timestamp:HH:mm} [{Level}] {Message:lj} {Properties:j} {NewLine}{Exception}";
            const string logTemplateConsole =
            "[{Timestamp:yyyy-MM-dd hh:mm:ss}] [{SourceContext}] [{Level}] : {Message}{NewLine} {Exception}";
        Log.Logger = new LoggerConfiguration().WriteTo.Console(outputTemplate: logTemplateConsole).CreateLogger();

        WindowOptions options = WindowOptions.Default;
        options.API = GraphicsAPI.None;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);

        _window = Window.Create(options);

        _sandBoxs = [];
        
        _window.Load += OnInit;
        _window.Render += OnRenderer;
        _window.Update += OnUpdate;
        _window.Resize += OnResize;
    }

    private void OnInit()
    {
        rendererContext = new DxContext(_window.Native!.DXHandle!.Value);
        rendererContext.Init();
        rendererContext.SetBufferColor(Color.LightSlateGray);
        
        // _sandBoxs.Add(new SandBox3D(rendererContext));
        _sandBoxs.Add(new SandBox2D(rendererContext));
        
        foreach (var sandBox in _sandBoxs)
        {
            sandBox.Init(_window.Size.X, _window.Size.Y);
        }
    }

    private void OnRenderer(double obj)
    {
        rendererContext.SwapBuffers();
        foreach (var sandBox in _sandBoxs)
            sandBox.Renderer((float)obj);
    }

    private void OnResize(Vector2D<int> obj)
    {
        rendererContext.ReSizeBuffer((uint)obj.X, (uint)obj.Y);
        
        Log.Logger.Information("Resized [{0}, {1}]",  obj.X, obj.Y);
    }
    
    private void OnUpdate(double obj)
    {
        foreach (var sandBox in _sandBoxs)
            sandBox.Update((float)obj);
    }

    public void Run() => _window.Run();
}