Shader "Custom/Black & White"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Gamma ("Gamma", Range(0.1, 3.0)) = 1.0
        _Blend ("B&W Blend", Range(0.0, 1.0)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Gamma;
            float _Blend;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Perceptual luminance (BT.709)
                float luminance = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));

                // Gamma curve
                luminance = pow(luminance, _Gamma);

                // Blend: 0 = original color, 1 = full B&W
                float3 final = lerp(col.rgb, float3(luminance, luminance, luminance), _Blend);

                return fixed4(final, col.a);
            }
            ENDCG
        }
    }

    FallBack "Unlit/Texture"
}