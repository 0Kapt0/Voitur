Shader "PSX/Flat Cutout"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Texture", 2D) = "white" {}
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.1

        [Header(World Scale)]
        [Enum(Auto,0,XY (mur),1,XZ (sol),2,ZY (mur cote),3)] _Projection("Projection", Float) = 0
        _Tiling("Repetitions par unite monde", Float) = 1
        [Toggle] _Anchor("Suivre le pivot de l'objet", Float) = 0

        [Header(PSX)]
        _SnapRes("Snapping vertex (resolution, 0 = off)", Float) = 160
        _ColorDepth("Niveaux de couleur", Float) = 32
        _Dither("Dithering", Range(0,1)) = 1
        _Ambient("Lumiere ambiante", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 100

        CGINCLUDE
        #include "UnityCG.cginc"
        #include "UnityLightingCommon.cginc"
        #include "AutoLight.cginc"

        sampler2D _MainTex;
        float4 _MainTex_ST;
        float4 _Color;
        float _Cutoff, _Projection, _Tiling, _Anchor;
        float _SnapRes, _ColorDepth, _Dither, _Ambient;

        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float3 worldPos : TEXCOORD0;   // position pour les UV (avec option pivot)
            float3 worldNormal : TEXCOORD1;
            float3 light : TEXCOORD2;
            float3 wpRaw : TEXCOORD4;      // vraie position monde (pour les lights)
            UNITY_FOG_COORDS(3)
        };

        v2f vertCommon(appdata v)
        {
            v2f o;
            float4 clip = UnityObjectToClipPos(v.vertex);

            // Vertex snapping (le wobble PSX)
            if (_SnapRes > 0)
            {
                float2 grid = _SnapRes * 0.5;
                float2 ndc = clip.xy / clip.w;
                ndc = round(ndc * grid) / grid;
                clip.xy = ndc * clip.w;
            }
            o.pos = clip;

            float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 pivot = float3(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23);
            o.wpRaw = wp;
            o.worldPos = lerp(wp, wp - pivot, _Anchor);
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
            o.light = 0;

            UNITY_TRANSFER_FOG(o, o.pos);
            return o;
        }

        v2f vertBase(appdata v)
        {
            v2f o = vertCommon(v);
            // Eclairage par vertex : lumiere directionnelle + ambiante
            float3 n = normalize(o.worldNormal);
            float ndl = saturate(dot(n, _WorldSpaceLightPos0.xyz));
            o.light = ndl * _LightColor0.rgb + ShadeSH9(float4(n, 1)) * _Ambient + _Ambient * 0.5;
            return o;
        }

        v2f vertAdd(appdata v)
        {
            return vertCommon(v);
        }

        fixed4 SampleAlbedo(v2f i)
        {
            // UV en espace monde : la texture garde la meme taille quel que soit le scale
            float3 wp = i.worldPos;
            float2 uv;
            if (_Projection < 0.5)
            {
                float3 n = abs(i.worldNormal);
                if (n.y >= n.x && n.y >= n.z) uv = wp.xz;
                else if (n.x >= n.z)          uv = wp.zy;
                else                          uv = wp.xy;
            }
            else if (_Projection < 1.5) uv = wp.xy;
            else if (_Projection < 2.5) uv = wp.xz;
            else                        uv = wp.zy;

            uv *= _Tiling;
            uv = uv * _MainTex_ST.xy + _MainTex_ST.zw;

            return tex2D(_MainTex, uv) * _Color;
        }

        static const float bayer[16] = {
             0, 8, 2,10,
            12, 4,14, 6,
             3,11, 1, 9,
            15, 7,13, 5
        };

        fixed4 fragBase(v2f i) : SV_Target
        {
            fixed4 col = SampleAlbedo(i);
            clip(col.a - _Cutoff);

            col.rgb *= i.light;

            // Reduction de couleurs + dithering ordonne
            uint2 px = uint2(i.pos.xy) % 4;
            float d = (bayer[px.y * 4 + px.x] / 16.0 - 0.5) / _ColorDepth * _Dither;
            col.rgb = floor((col.rgb + d) * _ColorDepth + 0.5) / _ColorDepth;

            UNITY_APPLY_FOG(i.fogCoord, col);
            return col;
        }

        // Point lights et spot lights (par pixel, avec cone pour les spots)
        fixed4 fragAdd(v2f i) : SV_Target
        {
            fixed4 col = SampleAlbedo(i);
            clip(col.a - _Cutoff);

            float3 n = normalize(i.worldNormal);
            float3 L = normalize(_WorldSpaceLightPos0.xyz - i.wpRaw * _WorldSpaceLightPos0.w);

            UNITY_LIGHT_ATTENUATION(atten, i, i.wpRaw);
            float ndl = saturate(dot(n, L));

            fixed4 o = fixed4(col.rgb * _LightColor0.rgb * ndl * atten, 0);
            UNITY_APPLY_FOG_COLOR(i.fogCoord, o, fixed4(0, 0, 0, 0));
            return o;
        }
        ENDCG

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vertBase
            #pragma fragment fragBase
            #pragma multi_compile_fog
            ENDCG
        }

        Pass
        {
            Tags { "LightMode"="ForwardAdd" }
            Blend One One
            ZWrite Off
            CGPROGRAM
            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #pragma multi_compile_fwdadd
            #pragma multi_compile_fog
            ENDCG
        }
    }
    Fallback "Transparent/Cutout/VertexLit"
}
