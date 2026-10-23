cbuffer MvpMat : register(b0) {
    float4x4 model;
    float4x4 view;
    float4x4 proj;
}

cbuffer MvpMat : register(b0) {
    float4x4 mvp;
}

Texture2DArray tex : register(t0);
TextureCube texCube : register(t0);
TextureCubeArray texCubes : register(t0);

SamplerState samLineear : register(s0);