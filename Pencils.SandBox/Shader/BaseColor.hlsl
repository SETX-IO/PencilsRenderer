struct Attributes
{
    float3 position : POSITION;
};
        
struct Varyings
{
    float4 position : SV_POSITION;
};
        
cbuffer MvpMat : register(b0) {
    float4x4 mvp; 
}
        
Varyings vert(Attributes In)
{
    Varyings Out;
            
    Out.position = float4(In.position, 1.0f);
    Out.position = mul(Out.position, mvp);
            
    return Out;
}

cbuffer Render2DData : register(b0) {
    float3 color; 
}

float4 frag(Varyings In) : SV_Target
{
    // return tex.Sample(samLineear, In.color.xy);
    return float4(color, 1);
}