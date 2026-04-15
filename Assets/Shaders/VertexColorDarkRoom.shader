Shader "Aftersun/Vertex Color Dark Room"
{
    Properties
    {
        _OverrideColor ("Override Color", Color) = (0,0,0,0)
        _DarknessColor ("Darkness Color", Color) = (0,0,0,0)
        _LightRadius ("Light Radius", Float) = 5
        _RingWidth ("Dither Ring Width", Float) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "cginc/VertexWiggle.cginc"
            #include "cginc/Utils.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 col : COLOR;
            };

            struct v2f
            {
                float3 col : COLOR;
                float3 worldPos : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _OverrideColor;
            float4 _DarknessColor;
            float _LightRadius;
            float _RingWidth;
            float3 _PlayerPosition;

            // 4x4 Bayer matrix normalized to 0-1
            static const float4x4 bayerMatrix = float4x4(
                 0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
                 3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
                15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
            );

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                //o.vertex = VertexWiggle(v.vertex);
                o.col = v.col;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Screen-space dither threshold
                float2 screenPos = i.vertex.xy;
                uint2 pixel = uint2(screenPos) % 4;
                float threshold = bayerMatrix[pixel.x][pixel.y];

                float dist = length(i.worldPos - _PlayerPosition);

                // Base lit color
                fixed4 col = (1,1,1,1);
                float3 vertCol = GAMMA_CORRECTION(i.col);
                col.xyz = lerp(vertCol, _OverrideColor, _OverrideColor.a);

                // Three zones:
                //   dist <= _LightRadius                        ? full color
                //   dist <= _LightRadius + _RingWidth           ? 50% dither
                //   dist >  _LightRadius + _RingWidth           ? full darkness
                float outerEdge = _LightRadius + _RingWidth;

                if (dist <= _LightRadius)
                {
                    // inside light — keep col as is
                }
                else if (dist <= outerEdge)
                {
                    // dither ring — 50% checkerboard between col and darkness
                    col = (threshold < 0.5) ? col : _DarknessColor;
                }
                else
                {
                    col = _DarknessColor;
                }

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
