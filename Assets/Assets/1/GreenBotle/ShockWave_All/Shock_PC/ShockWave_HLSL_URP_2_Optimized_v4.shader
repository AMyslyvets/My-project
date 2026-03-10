Shader "ShockWave_HLSL_URP_2_Optimized_v4"
{
    Properties
    {
        [MainTexture] _MainTexture ("Main Texture", 2D) = "white" {}
        _Noise ("Noise", 2D) = "white" {}
        _Flow ("Flow", 2D) = "white" {}
        _Mask ("Mask", 2D) = "white" {}

        _DistortionSpeedXYPowerZ ("Distortion Speed XY Power Z", Vector) = (0, 0.2, -0.5, 0.01)
        _NoiseSpeedXYPowerZ ("Noise Speed XY Power Z", Vector) = (0.02, 0.5, 1, 0)

        _Emission ("Emission", Float) = 4
        _NoiseOpacityLerp ("Noise Opacity Lerp", Range(0,1)) = 0
        [Toggle] _UV2Tswitch ("UV2Tswitch", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB
            Cull Off
            Lighting Off
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _MainTexture_ST;
                half4 _Noise_ST;
                half4 _Flow_ST;
                half4 _Mask_ST;
                half4 _DistortionSpeedXYPowerZ;
                half4 _NoiseSpeedXYPowerZ;
                half _Emission;
                half _NoiseOpacityLerp;
                half _UV2Tswitch;
            CBUFFER_END

            TEXTURE2D(_MainTexture); SAMPLER(sampler_MainTexture);
            TEXTURE2D(_Noise);       SAMPLER(sampler_Noise);
            TEXTURE2D(_Flow);        SAMPLER(sampler_Flow);
            TEXTURE2D(_Mask);        SAMPLER(sampler_Mask);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                half4  color      : COLOR;
                half4  uv0        : TEXCOORD0;
                half4  uv1        : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4  color      : COLOR;
                half4  uv0        : TEXCOORD0;
                half4  uv1        : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            inline half RemapNoisePower(half n, half p)
            {
                return (half(1.0) - p) + n * (p - (half(1.0) - p));
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = GetVertexPositionInputs(v.positionOS.xyz).positionCS;
                o.color = v.color;
                o.uv0 = v.uv0;
                o.uv1 = v.uv1;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                half timeValue = (half)_Time.y;
                half customT = i.uv1.w;
                half distortionMul = (_UV2Tswitch > half(0.5)) ? customT : half(1.0);
                half powerValue    = (_UV2Tswitch > half(0.5)) ? half(1.0) : customT;

                half2 baseUV  = i.uv0.xy;
                half2 mainUV  = baseUV * _MainTexture_ST.xy + _MainTexture_ST.zw + i.uv0.zw;
                half2 noiseUV = baseUV * _Noise_ST.xy       + _Noise_ST.zw       + _NoiseSpeedXYPowerZ.xy      * timeValue;
                half2 flowUV  = i.uv1.xy * _Flow_ST.xy      + _Flow_ST.zw        + _DistortionSpeedXYPowerZ.xy * timeValue;
                half2 maskUV  = baseUV * _Mask_ST.xy        + _Mask_ST.zw;

                half4 noiseSample = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, noiseUV);
                half2 flowRG      = SAMPLE_TEXTURE2D(_Flow, sampler_Flow, flowUV).rg;
                half4 mainSample  = SAMPLE_TEXTURE2D(_MainTexture, sampler_MainTexture, mainUV + flowRG * _DistortionSpeedXYPowerZ.z * distortionMul);
                half  maskAlpha   = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, maskUV).a;

                half noiseRemap = RemapNoisePower(lerp(noiseSample.r, half(1.0), _NoiseOpacityLerp), _NoiseSpeedXYPowerZ.z);

                half xMask = (ceil(mainUV.x) == 1.0) ? half(0.0) : half(1.0);
                half yMask = (ceil(mainUV.y) == 1.0) ? half(0.0) : half(1.0);
                half edgeMask = half(1.0) - max(xMask, yMask); // masks out pixels when main UV goes outside the 0..1 area, preventing edge artifacts

                half3 finalRGB =
                    _Emission *
                    noiseRemap *
                    saturate(pow(mainSample, powerValue.xxxx)).rgb *
                    i.color.rgb *
                    noiseSample.rgb;

                half finalAlpha = saturate(
                    mainSample.a *
                    edgeMask *
                    i.color.a *
                    maskAlpha *
                    noiseRemap *
                    i.uv1.z
                );

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}
/*
================ FUTURE OPTIMIZATION NOTES ================

Шейдер уже прошёл базовую оптимизацию:
- часть типов переведена в half
- математика в fragment уплотнена
- убраны лишние промежуточные переменные
- функционал полностью сохранён

Дальнейшую оптимизацию можно делать в следующих направлениях:

1. POW OPTIMIZATION
   pow(mainSample, powerValue) — одна из самых дорогих операций.
   Можно попробовать заменить на более дешёвые варианты, если powerValue
   используется в ограниченном диапазоне (например 1–3).

2. EDGE MASK
   Сейчас используется:
   half edgeMask = 1.0 - max(xMask, yMask);

   Это защита от артефактов UV за пределами 0..1.
   Можно попробовать альтернативы через step(), но важно сохранить
   тот же визуальный результат.

3. TEXTURE SAMPLE REDUCTION (если понадобится mobile версия)
   Сейчас используются 4 текстуры:
   - Main
   - Noise
   - Flow
   - Mask

   В mobile-версии можно рассмотреть:
   - packing mask + noise
   - packing flow + noise
   но это изменит pipeline текстур.

4. FLOW DISTORTION
   Сейчас:
   mainUV + flowRG * power

   Можно попробовать уменьшить точность flow или
   использовать half2, если не появятся артефакты.

5. BRANCH OPTIMIZATION
   Переключатель UV2Tswitch сейчас реализован через условие.
   Можно заменить на lerp() вариант, если понадобится убрать branch.

6. OVERDRAW OPTIMIZATION (НЕ В ШЕЙДЕРЕ)
   Для прозрачных эффектов главный враг — overdraw.
   Оптимизация возможна через:
   - уменьшение размера quad
   - уменьшение lifetime частиц
   - ограничение количества shockwave одновременно

===========================================================
*/