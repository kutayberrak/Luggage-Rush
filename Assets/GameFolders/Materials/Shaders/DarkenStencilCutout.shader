Shader "URP/DarkenStencilCutout"
{
    Properties
    {
        _Color ("Tint", Color) = (0,0,0,0.5)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue"         = "Transparent"
            "RenderType"    = "Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            // Stencil == 1 olan yerlerde ÇİZME -> delik aç
            Stencil
            {
                Ref 1
                Comp NotEqual
                ReadMask 255
            }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float3 positionOS : POSITION; };
            struct Varyings   { float4 positionHCS: SV_POSITION; };

            // SRP Batcher uyumlu malzeme değişkenleri
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
            CBUFFER_END

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                return _Color; // alfa ile karanlık miktarı
            }
            ENDHLSL
        }
    }
}
