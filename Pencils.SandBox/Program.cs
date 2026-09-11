using System.Drawing;
using System.Numerics;
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

record struct Vertex(Vector3 Position, Vector3 Color);

public struct CameraData(Vector3 position, Vector2 viewSize, float fov)
{
    public Matrix4x4 CameraMat => _viewMat * _projMat;
    
    private readonly Matrix4x4 _viewMat = Matrix4x4.CreateLookToLeftHanded(position, Vector3.UnitZ, Vector3.UnitY);
    private readonly Matrix4x4 _projMat = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(float.DegreesToRadians(fov), viewSize.X / viewSize.Y, 1, 100);
}

public class App
{
    private readonly IWindow _window;
    private IGraphicsContext rendererContext;
    
    private Renderer _renderer;
    private IMesh _mesh;
    private IShader _shader;

    private float _rotation;
    
    private CameraData _cameraData;
    
    private const string ShaderCode =
        """
        struct Attributes
        {
            float3 position : POSITION;
            float3 color : COLOR0;
        };
        
        struct Varyings
        {
            float4 position : SV_POSITION;
            float4 color : COLOR0;
        };
        
        cbuffer MvpMat : register(b0) {
            float4x4 mvp; 
        }
        
        Varyings vert(Attributes In)
        {
            Varyings Out;
            
            Out.position = float4(In.position, 1.0f);
            Out.position = mul(Out.position, mvp);
        
            Out.color = float4(In.color, 1.0f);
            
            return Out;
        }
        
        float4 frag(Varyings In) : SV_Target
        {
            // return float4(0, 0.3f, 0.1f, 1);
            return In.color;
        }
        """;
    
    public App(string[] args, string title, int width, int height)
    {
        const string LogTemplate =
            "{SourceContext} {Scope} {Timestamp:HH:mm} [{Level}] {Message:lj} {Properties:j} {NewLine}{Exception}";
        const string LogTemplateConsole =
            "[{Timestamp:yyyy-MM-dd hh:mm:ss}] [{SourceContext}] [{Level}] : {Message}{NewLine} {Exception}";
        Log.Logger = new LoggerConfiguration().WriteTo.Console(outputTemplate: LogTemplateConsole).CreateLogger();

        WindowOptions options = WindowOptions.Default;
        options.API = GraphicsAPI.None;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);

        _window = Window.Create(options);

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
        
        _renderer = new Renderer(rendererContext);
        
        Vertex[] a = [
            new(new Vector3( 0,     0.5f, 0), new Vector3(1, 0, 0)),
            new(new Vector3( 0.5f, -0.5f, 0), new Vector3(0, 1, 0)),
            new(new Vector3(-0.5f, -0.5f, 0), new Vector3(0, 0, 1)) 
        ];

        ushort[] ii =
        [
            0, 1, 2
        ];
        
        _cameraData = new CameraData(new Vector3(0, 0, -2), new Vector2(_window.Size.X, _window.Size.Y), 45f);
        
        Log.Logger.Information("Initialized DxContext");
        
        _mesh = DxMesh.Create();
        var vertexBuffer = DxVertexBuffer.Create(rendererContext, a);
        vertexBuffer.SetVertexAttribs(VertexAttribType.Position3, VertexAttribType.Color3);
        
        _mesh.AddVertexBuffer(vertexBuffer);
        _mesh.SetIndexBuffer(DxIndexBuffer.Create(rendererContext, ii));
        
        _shader = DxShader.Create(rendererContext, ShaderCode);
        _shader.SetVertexAttrib(_mesh.VertexAttribs);
        _shader.UploadConstantMat44("MvpMat", _cameraData.CameraMat);
    }

    private void OnRenderer(double obj)
    {
        _rotation += (float)obj * 2;
        rendererContext.SwapBuffers();
        
        _renderer.SetViewport(_window.Size.X, _window.Size.Y, 1f);
        _renderer.RCommand.DefaultPrimitiveTopology();
        
        _renderer.BeginScene();
        
        _shader.Use();
        _shader.UploadConstantMat44("MvpMat", Matrix4x4.CreateRotationY(_rotation) * _cameraData.CameraMat);
        _renderer.Submit(_mesh);

        _renderer.EndScene();
    }

    private void OnResize(Vector2D<int> obj)
    {
        rendererContext.ReSizeBuffer((uint)obj.X, (uint)obj.Y);
        Log.Logger.Information("Resized [{0}, {1}]",  obj.X, obj.Y);
    }


    private void OnUpdate(double obj)
    {
    }

    public void Run() => _window.Run();
}