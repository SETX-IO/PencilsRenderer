#include "Include/ShaderType.hlsli"
#include "Include/EngineConstant.hlsl"
    
Varyings vert(Attributes In)
{
    Varyings Out;
            
    Out.position = float4(In.position, 1.0f);
    Out.position = mul(Out.position, mvp);
        
    Out.color = In.color;
    Out.texCoord = In.texCoord;
            
    return Out;
}

cbuffer TextureInfo : register(b1) {
    float tilingFactor;
}

float4 frag(Varyings In) : SV_Target
{
    // In.texCoord.xy *= tilingFactor;
    return tex.Sample(samLineear, In.texCoord) * float4(In.color, 1.0f);
}