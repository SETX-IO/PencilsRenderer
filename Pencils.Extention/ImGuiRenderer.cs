using Hexa.NET.ImGui;

namespace Pencils.Extension;

public class ImGuiRenderer
{
    public Renderer _renderer;
    
    public ImGuiRenderer(Renderer renderer)
    {
        _renderer = renderer;
        
        ImGui.CreateContext();
        ImGuiIOPtr io = ImGui.GetIO();

        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.RendererHasVtxOffset;
        
        
    }

    public void Render()
    {
        ImDrawDataPtr drawData = ImGui.GetDrawData();

        if (drawData.DisplaySize.X == 0 || drawData.DisplaySize.Y == 0) return;

        for (int i = 0; i < drawData.Textures.Size; i++)
        {
            
        }
    }

    public void Update()
    {
        
    }
}