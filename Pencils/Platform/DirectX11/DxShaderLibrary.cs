using System.Collections.Generic;
using System.IO;
using Pencils.RendererApi;

namespace Pencils.Platform.DirectX11;

public class DxShaderLibrary : IShaderLibrary
{
    private readonly IResourcesFactory _factory;
    private readonly Dictionary<string, IShader> _shaders;
    
    private DxShaderLibrary(IResourcesFactory factory)
    {
        _factory = factory;
        _shaders = new Dictionary<string, IShader>();
    }
    
    public IShader this[string name]
    {
        get => _shaders.TryGetValue(name, out IShader shader) ? shader : null;
        set => _shaders.TryAdd(name, value);
    }

    public IShader Load(string shaderPath)
    {
        IShader shader;
        string name = Path.GetFileNameWithoutExtension(shaderPath);
        
        if (_shaders.TryGetValue(name, out shader))
            return shader;
            
        shader = DxShader.Create(_factory, shaderPath);
        _shaders.TryAdd(shader.Name, shader);
        
        return shader;
    }

    public IShader Load(string shaderName, string shaderCode)
    {
        throw new System.NotImplementedException();
    }

    public static IShaderLibrary Create(IGraphicsContext context)
    {
        return new DxShaderLibrary(context.ResourcesFactory);
    }
}