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

struct ImGuiAttributes
{
    float2 pos : POSITION;
    float2 uv  : TEXCOORD0;
    float4 col : COLOR0;
};

struct ImGuiVaryings
{
    float4 pos : SV_POSITION;
    float4 col : COLOR0;
    float2 uv  : TEXCOORD0;
};