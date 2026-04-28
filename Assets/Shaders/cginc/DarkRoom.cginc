#ifndef AFTERSUN_DARKROOM_INCLUDED
#define AFTERSUN_DARKROOM_INCLUDED

// =============================================================================
// Dark room shared values.
//
// _PlayerPosition and _DarkRoomFactor stay as runtime globals because they DO
// change during gameplay. Set them from C# as before:
//   Shader.SetGlobalVector("_PlayerPosition", playerPos);
//   Shader.SetGlobalFloat ("_DarkRoomFactor", 1.0f);
// =============================================================================

// #021A14  ->  (2, 26, 20) / 255
// NOTE: these are raw normalized sRGB values. If your project is in Linear
// color space and the result looks wrong, convert to linear here instead.
static const float4 _DarknessColor = float4(0.000607, 0.010330, 0.006995, 1.0);
static const float _LightRadius = 2.0;
static const float _RingWidth = 0.7;

// Runtime globals.
float3 _PlayerPosition;
float _DarkRoomFactor;

// 4x4 Bayer matrix normalized to 0-1, used for the dither ring band.
static const float4x4 _DarkRoom_BayerMatrix = float4x4(
     0.0 / 16.0, 8.0 / 16.0, 2.0 / 16.0, 10.0 / 16.0,
    12.0 / 16.0, 4.0 / 16.0, 14.0 / 16.0, 6.0 / 16.0,
     3.0 / 16.0, 11.0 / 16.0, 1.0 / 16.0, 9.0 / 16.0,
    15.0 / 16.0, 7.0 / 16.0, 13.0 / 16.0, 5.0 / 16.0
);

// -----------------------------------------------------------------------------
// ApplyDarkRoom
//   Three-zone radial light around _PlayerPosition:
//     dist <= lightRadius              -> baseCol unchanged
//     dist <= lightRadius + ringWidth  -> 50% Bayer dither vs _DarknessColor
//     else                             -> full _DarknessColor
//
//   baseCol   : surface color computed by the calling shader so far.
//   worldPos  : surface point in world space (passed through from vert).
//   screenPos : i.vertex.xy from SV_POSITION (pixel coords for the dither).
// -----------------------------------------------------------------------------
fixed4 ApplyDarkRoom(fixed4 baseCol, float3 worldPos, float2 screenPos)
{
    uint2 pixel = uint2(screenPos) % 4;
    float threshold = _DarkRoom_BayerMatrix[pixel.x][pixel.y];

    float dist = length(worldPos - _PlayerPosition);
    float lightRadius = _LightRadius * _DarkRoomFactor;
    float ringWidth = _RingWidth * _DarkRoomFactor;
    float outerEdge = lightRadius + ringWidth;

    if (dist <= lightRadius)
    {
        return baseCol;
    }
    else if (dist <= outerEdge)
    {
        return (threshold < 0.5) ? baseCol : _DarknessColor;
    }
    return _DarknessColor;
}

#endif // AFTERSUN_DARKROOM_INCLUDED
