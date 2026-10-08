Shader "Custom/WaveformTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold ("Black Threshold", Range(0, 1)) = 0.05
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Threshold;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Luminosité de l'image
                float brightness = dot(col.rgb, float3(0.299, 0.587, 0.114));

                // Noir = transparent
                // Blanc = opaque
                col.a = smoothstep(_Threshold, _Threshold + 0.05, brightness);

                return col;
            }

            ENDCG
        }
    }
}