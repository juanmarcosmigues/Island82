Shader "Aftersun/Vertex Color Water"
{
    Properties
    {
        _OverrideColor ("Override Color", Color) = (0,0,0,0)
        _LightChange ("Light Change", Float) = 3
        _Waves ("Waves", Vector) = (0,0,0,0)
        _Noise ("Noise", 2D) = "black" {}
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
            sampler2D _Noise;
            float4 _Waves;
            float _LightChange;

            float2 waterVertexDisplacement (float3 vertexWorldPos, float influence, float steps) 
            {
                float steppedTime = floor(_Time.y / (1.0 / steps)) / steps;
                float4 uv = float4((vertexWorldPos.x + steppedTime) * _Waves.w, (vertexWorldPos.z + steppedTime) * _Waves.w, 0, 0); 
                fixed4 noise = tex2Dlod(_Noise, uv);

                return float2((lerp(-1, 1, noise.r)  * _Waves.z + _Waves.x) * influence, (lerp(-1, 1, noise.g) * _Waves.z + _Waves.y) * influence);
            }

            v2f vert (appdata v)
            {
                v2f o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float2 waves = waterVertexDisplacement(worldPos, 1-v.col.a, 8);

                float3 worldDisp = float3(waves.x, 0, waves.y);
                // Transform back to object space and apply
                v.vertex.xyz += mul((float3x3)unity_WorldToObject, worldDisp);

                o.vertex = UnityObjectToClipPos(v.vertex);
                
                //o.vertex = VertexWiggle(v.vertex);
                o.col = v.col;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float steppedTime = floor(_Time.y / (1.0 / 8)) / 8;
                // sample the texture
                fixed4 col = (1,1,1,1);
                float3 vertCol = GAMMA_CORRECTION(i.col);
                float lightChange = sin(steppedTime);
                col.xyz = lerp(vertCol, _OverrideColor, lerp(0, 0.5, lightChange));
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
