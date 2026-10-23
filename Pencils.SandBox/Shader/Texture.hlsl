#include "Include/EngineConstant.hlsl"
#include "Include/ShaderType.hlsli"
        
Varyings vert(Attributes In)
{
    Varyings Out;
            
    Out.position = float4(In.position, 1.0f);
    
    matrix mvp = mul(model, view);
    mvp = mul(mvp, proj);
    
    Out.position = mul(Out.position, mvp);
        
    Out.color = In.color;
    Out.texCoord = In.texCoord;
    
    return Out;
}

float4 frag(Varyings In) : SV_Target
{
    float4 color = tex.Sample(samLineear, In.texCoord) * float4(In.color, 1.0f);
    return color;
}