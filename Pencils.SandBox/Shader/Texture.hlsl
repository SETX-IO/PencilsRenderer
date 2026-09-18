struct Attributes
{
    float3 position : POSITION;
    float2 color : TEXCOORD0;
};
        
struct Varyings
{
    float4 position : SV_POSITION;
    float2 color : TEXCOORD0;
};
        
cbuffer MvpMat : register(b0) {
    float4x4 mvp; 
}
        
Varyings vert(Attributes In)
{
    Varyings Out;
            
    Out.position = float4(In.position, 1.0f);
    Out.position = mul(Out.position, mvp);
        
    Out.color = In.color;
            
    return Out;
}
        
Texture2D tex : register(t0);
SamplerState samLineear : register(s0);
        
float4 frag(Varyings In) : SV_Target
{
    return tex.Sample(samLineear, In.color.xy);
    // return float4(In.color.xy, 0, 1);
}