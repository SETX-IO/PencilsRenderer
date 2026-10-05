cbuffer Parameter : register(b0)
{
    float deltaTime;
}

struct Particle
{
    float2 position;
    float3 velocity;
    float4 color;
};

StructuredBuffer<Particle> ParticleSSBOIn : register(t0);
RWStructuredBuffer<Particle> ParticleSSBOOut : register(u0);

[numthreads(256, 1, 1)]
void compute(uint3 globalID : SV_DispatchThreadID)
{
    uint index = globalID.x;
    Particle particleIn = ParticleSSBOIn[index];
    
    ParticleSSBOOut[index].position = particleIn.position + particleIn.velocity.xy * deltaTime;
    ParticleSSBOOut[index].velocity = particleIn.velocity;
    ParticleSSBOOut[index].color = particleIn.color;
}