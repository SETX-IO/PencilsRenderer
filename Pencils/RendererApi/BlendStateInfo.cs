namespace Pencils.RendererApi;

public enum BlendFactor
{
    Zero = 1,
    One,
    SourceAlpha = 5,
    InverseSourceAlpha,
    InverseDestinationAlpha = 8
}

public enum BlendOp
{
    Add = 1,
    Subtract,
    ReverseSubtract,
    Minimum,
    Maximum,
}

public record struct BlendStateInfo
{
    public bool enabled = true;
    
    public BlendFactor srcFactor = BlendFactor.One;
    public BlendFactor dstFactor = BlendFactor.Zero;
    public BlendOp blendOp = BlendOp.Add;
    public BlendFactor srcAlphaFactor =  BlendFactor.One;
    public BlendFactor dstAlphaFactor = BlendFactor.Zero;
    public BlendOp blendOpAlpha =  BlendOp.Add;

    public BlendStateInfo() : this(true) {}
    
    public BlendStateInfo(bool enabled)
    {
        this.enabled = enabled;
        srcFactor = BlendFactor.One;
        dstFactor = BlendFactor.Zero;
        blendOp = BlendOp.Add;
        srcAlphaFactor =  BlendFactor.One;
        dstAlphaFactor = BlendFactor.Zero;
        blendOpAlpha =  BlendOp.Add;
    }
}