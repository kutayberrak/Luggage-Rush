Shader "Hidden/StencilWrite"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            ZWrite On
            ColorMask 0

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
                WriteMask 255
            }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float3 positionOS : POSITION; };
            struct Varyings   { float4 positionHCS: SV_POSITION; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Renk yazmıyoruz (ColorMask 0), yine de bir değer döndürmeliyiz
                return half4(0,0,0,0);
            }
            ENDHLSL
        }
    }
}
