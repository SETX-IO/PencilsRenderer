#include "Include/ShaderType.hlsli"
#include "Include/EngineConstant.hlsl"

Varyings vert(uint vertexID : SV_VertexID)
{
    Varyings Out;
    
    float2 grid = float2((vertexID << 1) & 2, vertexID & 2);
    float2 xy = grid * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f);
    
    Out.position = float4(xy, 1.f, 1.f);
    Out.color = float3(1.f, 1.f, 1.f);
    Out.texCoord = float3(grid * float2(1.0f, 1.0f), 0.f);
    
    return Out;
}

float4 frag(Varyings In) : SV_Target
{
    float4 texColor = tex.Sample(samLineear, In.texCoord) * float4(In.color, 1.0f);
    float gray = texColor.r * 0.299f + texColor.g * 0.587f + texColor.b * 0.114f;
    float4 color = float4(gray, gray, gray, texColor.a);
    
    return color;
}