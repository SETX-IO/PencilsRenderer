struct Attributes
{
    float3 position : POSITION;
};
        
struct Varyings
{
    float4 position : SV_POSITION;
    float3 texCoord : TEXCOORD0;
};

cbuffer MvpMat : register(b0) {
    float4x4 mvp;
}

Varyings vert(Attributes In)
{
    Varyings Out;
            
    Out.position = float4(In.position, 1.f);
    Out.position = mul(Out.position, mvp);
    
    Out.texCoord = In.position;
            
    return Out;
}
        
TextureCube tex : register(t0);
SamplerState samLineear : register(s0);

float4 frag(Varyings In) : SV_Target
{
    float4 color = tex.Sample(samLineear, In.texCoord);
    return color;
}