struct Attributes
{
    float3 position : POSITION;
    float3 color : COLOR0;
};
        
struct Varyings
{
    float4 position : SV_POSITION;
    float4 color : COLOR0;
};
        
cbuffer MvpMat : register(b0) {
    float4x4 mvp; 
}

cbuffer Transform
{
    float4x4 transform;
}
        
Varyings vert(Attributes In)
{
    Varyings Out;
            
    Out.position = float4(In.position, 1.0f);
    // Out.position = mul(Out.position, transform);
    Out.position = mul(Out.position, mvp);
    
    Out.color = float4(In.color, 1.f);
            
    return Out;
}

cbuffer Render2DData : register(b0) {
    float3 color; 
}

float4 frag(Varyings In) : SV_Target
{
    // return tex.Sample(samLineear, In.color.xy);
    return In.color;
}