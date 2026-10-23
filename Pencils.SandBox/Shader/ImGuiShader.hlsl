#include "Include/EngineConstant.hlsl"
#include "Include/ShaderType.hlsli"

ImGuiVaryings vert(ImGuiAttributes input)
{
    ImGuiVaryings output;
    output.pos = mul(mvp, float4(input.pos.xy, 0.f, 1.f));
              
    output.col = input.col;
    output.uv  = input.uv;
    return output;
}

sampler sampler0 : register(s0);
Texture2D texture0 : register(t0);

float4 frag(ImGuiVaryings input) : SV_Target
{
    float4 out_col = input.col * texture0.Sample(sampler0, input.uv); 
    return out_col;
}