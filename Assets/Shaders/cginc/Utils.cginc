#ifndef UTILS_INCLUDED
#define UTILS_INCLUDED

#define STEP_VALUE(value, steps) (floor((value) * (steps - 1) + 0.5) / (steps - 1))

#define GAMMA_CORRECTION(color) (pow(color, 2.2))

float4 lerp3(float4 a, float4 b, float4 c, float t)
{
    float t2 = saturate(t) * 2.0;
    return (t2 < 1.0) ? lerp(a, b, t2) : lerp(b, c, t2 - 1.0);
}

half3 BLEND_OVERLAY(half3 base, half3 blend, half opacity)
{
    half3 result = lerp(
        2.0 * base * blend,
        1.0 - 2.0 * (1.0 - base) * (1.0 - blend),
        step(0.5, base)
    );
    return lerp(base, result, opacity);
}
#endif