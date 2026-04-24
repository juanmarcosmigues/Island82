Shader "Aftersun/Vertex Color Dither Linear"
{
    Properties
    {
        _Opacity ("Opacity", Range(0, 1)) = 1
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

            float _Opacity;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 col : COLOR;
            };

            struct v2f
            {
                float4 col : COLOR;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

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
                //o.vertex = VertexWiggle(v.vertex);
                o.col = v.col;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Screen-space dither
                float2 screenPos = i.vertex.xy;
                uint2 pixel = uint2(screenPos) % 4;
                float threshold = bayerMatrix[pixel.x][pixel.y];

                clip((_Opacity * i.col.a) - threshold);

                fixed4 col = fixed4(1,1,1,1);
                float3 vertCol = i.col;
                col.xyz = vertCol;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}