#ifndef DIRECTIONAL_COLOR_INCLUDED
#define DIRECTIONAL_COLOR_INCLUDED

#include "Utils.cginc"

half4 _DirColorRight, _DirColorLeft;
half4 _DirColorUp, _DirColorDown;
half4 _DirColorForward, _DirColorBack;
float _DirColorIntensity;

half3 GetDirectionalColor(float3 normalWS)
{
    float3 nPos = saturate(normalWS);
    float3 nNeg = saturate(-normalWS);
    
    nPos = STEP_VALUE(nPos, 3);
    nNeg = STEP_VALUE(nNeg, 3);

    return nPos.x * _DirColorRight.rgb
         + nNeg.x * _DirColorLeft.rgb
         + nPos.y * _DirColorUp.rgb
         + nNeg.y * _DirColorDown.rgb
         + nPos.z * _DirColorForward.rgb
         + nNeg.z * _DirColorBack.rgb;
}

#endif