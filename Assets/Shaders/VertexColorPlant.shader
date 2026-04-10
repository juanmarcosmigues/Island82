Shader "Aftersun/Vertex Color Plant"
{
    Properties
    {
        _OverrideColor ("Override Color", Color) = (0,0,0,0)
        _Wind ("Wind", Vector) = (0,0,0,0)
        _HorizontalRadius ("Horizontal Radius", Float) = 1
        _VerticalRadius ("Height Radius", Float) = 2
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 col : COLOR;
                UNITY_FOG_COORDS(0)
                float4 vertex : SV_POSITION;
            };

            float4 _OverrideColor;
            float4 _Wind;
            float _HorizontalRadius;
            float _VerticalRadius;

            float3 _PlayerPosition;

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

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 objectOrigin = unity_ObjectToWorld._m03_m13_m23;
                float3 delta = objectOrigin - _PlayerPosition;
                float hDist = 1 - clamp(0, 1, length(delta.xz) / _HorizontalRadius);
                float vDist = 1 - clamp(0, 1, -delta.y / _VerticalRadius);
                float dist = hDist * vDist;

                float pushStrength = dist * (v.uv.y * v.uv.y);
                
                float3 push = normalize(delta) * pushStrength * 1;
                push.y *= 0.2; 
                worldPos += push;

                //Apply to local space before clip space
                v.vertex = mul(unity_WorldToObject, float4(worldPos, 1.0)); 

                o.vertex = UnityObjectToClipPos(v.vertex);
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
