Shader "Aftersun/Vertex Color Folliage"
{
    Properties
    {
        _OverrideColor ("Override Color", Color) = (0,0,0,0)
        _Wind ("Wind", Vector) = (0,0,0,0)
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "cginc/VertexWiggle.cginc"
            #include "cginc/Utils.cginc"

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

            float4 _OverrideColor;
            float4 _Wind;

            float2 windVertexDisplacement (appdata v, float steps) 
            {
                float phase = v.vertex.x * 0.5 + v.vertex.z * 0.3; 

                float steppedTime = floor(_Time.y / (1.0 / steps)) / steps;

                float sway1 = sin(steppedTime * _Wind.z + phase);               
                float sway2 = sin(steppedTime * _Wind.z * 2.3 + phase * 1.7) * 0.5; 
                float sway3 = sin(steppedTime * _Wind.z * 0.4 + phase * 0.6) * 0.3;

                float sway = sway1 + sway2 + sway3;
                float2 wind = _Wind.xy * sway * _Wind.w * v.col.a;

                return wind;
            }

            v2f vert (appdata v)
            {
                v2f o;

                v.vertex.xy += windVertexDisplacement(v, 8);
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                //o.vertex = VertexWiggle(v.vertex);
                o.col = v.col;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = (1,1,1,1);
                float3 vertCol = GAMMA_CORRECTION(i.col);
                col.xyz = lerp(vertCol, _OverrideColor, _OverrideColor.a);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
