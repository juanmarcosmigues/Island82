#ifndef PS1_EFFECTS_INCLUDED
#define PS1_EFFECTS_INCLUDED

#define PS1_DEFAULT_RESOLUTION 50.0

float4 VertexWiggle(float4 vertex)
{
    float4 view = mul(UNITY_MATRIX_MV, vertex);
    view.xyz = round(view.xyz * PS1_DEFAULT_RESOLUTION) / PS1_DEFAULT_RESOLUTION;
    return mul(UNITY_MATRIX_P, view);
}

float4 VertexWiggle(float4 vertex, float resolution)
{
    float4 view = mul(UNITY_MATRIX_MV, vertex);
    view.xyz = round(view.xyz * resolution) / resolution;
    return mul(UNITY_MATRIX_P, view);
}

#endif