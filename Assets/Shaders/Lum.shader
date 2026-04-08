Shader "Aftersun/Lum"
{
    Properties
    {
        _Cycle ("Cycle", Float) = 1.5
        _OverrideColor ("Override Color", Color) = (0,0,0,0)
        _Color0 ("Color 0", Color) = (0,0,0,0)
        _Color1 ("Color 1", Color) = (0,0,0,0)
        _Color2 ("Color 2", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

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
                float3 col : COLOR;
            };

            struct v2f
            {
                float3 col : COLOR;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float _Cycle;
            float4 _OverrideColor;
            float4 _Color0;
            float4 _Color1;
            float4 _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float steppedTime = floor(_Time.y / (1.0 / 8)) / 8;

                float cycle = (sin(_Cycle  * steppedTime) + 1) * 0.5;
                float3 vertCol = v.col.r <= 0.1 ? 
                lerp3(_Color0, _Color1, _Color2, cycle) :
                lerp3(_Color0, _Color1, _Color2, 1-cycle) ;

                o.col = vertCol;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = (1,1,1,1);
                float3 vertCol = (i.col);
                col.xyz = lerp(vertCol, _OverrideColor, _OverrideColor.a);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
