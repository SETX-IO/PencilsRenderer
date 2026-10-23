#include "Include/EngineConstant.hlsl"

struct Attributes
{
    float3 position : POSITION;
};
        
struct Varyings
{
    float4 position : SV_POSITION;
    float3 texCoord : TEXCOORD0;
};

Varyings vert(Attributes In)
{
    Varyings Out;
    
    matrix mvp = mul(model, view);
    mvp = mul(mvp, proj);
    
    Out.position = float4(In.position, 1.f);
    Out.position = mul(Out.position, mvp);
    
    Out.position.z = Out.position.w;
    
    Out.texCoord = In.position;
            
    return Out;
}

float4 frag(Varyings In) : SV_Target
{
    float4 color = texCube.Sample(samLineear, In.texCoord);
    return color;
}