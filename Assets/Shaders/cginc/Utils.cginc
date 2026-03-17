#ifndef UTILS_INCLUDED
#define UTILS_INCLUDED

#define STEP_VALUE(value, steps) (floor((value) * (steps - 1) + 0.5) / (steps - 1))

#define GAMMA_CORRECTION(color) (pow(color, 2.2))

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