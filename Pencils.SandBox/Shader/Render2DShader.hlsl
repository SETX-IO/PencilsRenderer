struct Attributes
{
    float3 position : POSITION;
    float3 color :COLOR0;
    float3 texCoord : TEXCOORD0;
};
        
struct Varyings
{
    float4 position : SV_POSITION;
    float3 color :COLOR0;
    float3 texCoord : TEXCOORD0;
};
        
cbuffer MvpMat : register(b0) {
    float4x4 mvp; 
}

cbuffer Transform : register(b1)
{
    float4x4 transform;
}
        
Varyings vert(Attributes In)
{
    Varyings Out;
            
    Out.position = float4(In.position, 1.0f);
    // Out.position = mul(Out.position, transform);
    Out.position = mul(Out.position, mvp);
        
    Out.color = In.color;
    Out.texCoord = In.texCoord;
            
    return Out;
}
        
Texture2D tex[32] : register(t0);
SamplerState samLineear : register(s0);
        
cbuffer Render2DData : register(b0) {
    float3 color; 
}

cbuffer TextureInfo : register(b1) {
    float tilingFactor;
    float texIndex;
}

float4 frag(Varyings In) : SV_Target
{
    // In.texCoord.xy *= tilingFactor;
    return tex[In.texCoord.z].Sample(samLineear, In.texCoord.xy) * float4(In.color, 1.0f);
}