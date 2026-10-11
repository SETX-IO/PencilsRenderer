namespace Pencils.RendererApi;

public enum WriteMask
{
    Off,
    All
}

public enum ComparisonFunc
{
    Never = 1,
    Less,
    LessEqual = 4,
    Always = 8
}

public record struct DepthStencilInfo
{
    public bool depthEnable = true;
    public bool stencilEnable = false;
    public ComparisonFunc depthFunc = ComparisonFunc.Less;
    public WriteMask depthWriteMask = WriteMask.All;

    public DepthStencilInfo() : this(true, false) {}
    
    public DepthStencilInfo(bool depthEnable, bool stencilEnable)
    {
        this.depthEnable = depthEnable;
        this.stencilEnable = stencilEnable;
        depthFunc = ComparisonFunc.Less;
    }
}