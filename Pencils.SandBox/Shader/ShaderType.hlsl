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