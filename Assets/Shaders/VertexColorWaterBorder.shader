Shader "Aftersun/Vertex Color Water Border"
{
    Properties
    {
        _OverrideColor ("Override Color", Color) = (0,0,0,0)
        _LightChange ("Light Change", Float) = 3
        _Noise ("Noise", 2D) = "black" {}
        _Speed ("Scroll Speed", Float) = 0.5
        _Amount ("Amount", Range(0,1)) = 1

        [Header(Dark Room)]
        [Toggle(_DARKROOM_ON)] _DarkRoom ("Enable Dark Room", Float) = 0
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
            // dark room toggle keyword (driven by the [Toggle] attribute above)
            #pragma shader_feature_local _DARKROOM_ON

            #include "UnityCG.cginc"
            #include "cginc/Utils.cginc"
            #if defined(_DARKROOM_ON)
            #include "cginc/DarkRoom.cginc"
            #endif

            struct appdata
            {
                float4 vertex : POSITION;
                float4 col : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 col : COLOR;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
                float4 vertex : SV_POSITION;
            };

            float4 _OverrideColor;
            sampler2D _Noise;
            float4 _Noise_ST;
            float _LightChange;
            float _Speed;
            float _Amount;

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);           
                o.col = v.col;
                o.uv = v.uv;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 noise = tex2D(_Noise, i.worldPos.xz * _Noise_ST.xy + _Time.y * _Speed);
                float clipValue = noise.r + (1-i.uv.y);

                clip(clipValue* _Amount-0.5 );

                float steppedTime = floor(_Time.y / (1.0 / 8)) / 8;
                // sample the texture
                fixed4 col = (1,1,1,1);
                float3 vertCol = GAMMA_CORRECTION(i.col);
                float lightChange = sin(steppedTime);
                col.xyz = lerp(vertCol, _OverrideColor, lerp(0, 0.5, lightChange));

            #if defined(_DARKROOM_ON)
                col = ApplyDarkRoom(col, i.worldPos, i.vertex.xy);
            #endif

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
