Shader "ShockWave_HLSL_URP_PC_Clean_com"
{
    Properties
    {
        // Основная текстура шоквейва
        [MainTexture] _MainTexture ("Main Texture", 2D) = "white" {}

        // Шум для дополнительной модуляции RGB/Alpha
        _Noise ("Noise", 2D) = "white" {}

        // Flow map для искажения UV основной текстуры
        _Flow ("Flow", 2D) = "white" {}

        // Дополнительная маска alpha
        _Mask ("Mask", 2D) = "white" {}

        // XY = скорость паннинга flow
        // Z  = сила дисторшна
        // W  = не используется, оставлен как в оригинале
        _DistortionSpeedXYPowerZ ("Distortion Speed XY Power Z", Vector) = (0, 0.2, -0.5, 0.01)

        // XY = скорость паннинга noise
        // Z  = remap/power-подобное влияние noise
        // W  = не используется, оставлен как в оригинале
        _NoiseSpeedXYPowerZ ("Noise Speed XY Power Z", Vector) = (0.02, 0.5, 1, 0)

        // Общая яркость эффекта
        _Emission ("Emission", Float) = 4

        // Смешивание noise.r -> 1
        // 0 = используем noise.r
        // 1 = noise влияние на opacity почти отключается
        _NoiseOpacityLerp ("Noise Opacity Lerp", Range(0,1)) = 0

        // Переключатель логики из оригинального ASE:
        // Off -> distortion uses 1, pow uses T
        // On  -> distortion uses T, pow uses 1
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

            // Стандартный прозрачный blend
            Blend SrcAlpha OneMinusSrcAlpha

            // Пишем только RGB, alpha идет в выход цвета, но в буфер отдельно не нужен
            ColorMask RGB

            // Для двустороннего рендера
            Cull Off

            // Без освещения
            Lighting Off

            // Для прозрачности обычно выключаем запись в depth
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // =========================================================
            // Material properties
            // =========================================================
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTexture_ST;
                float4 _Noise_ST;
                float4 _Flow_ST;
                float4 _Mask_ST;

                float4 _DistortionSpeedXYPowerZ;
                float4 _NoiseSpeedXYPowerZ;

                float _Emission;
                float _NoiseOpacityLerp;
                float _UV2Tswitch;
            CBUFFER_END

            // =========================================================
            // Textures
            // =========================================================
            TEXTURE2D(_MainTexture);
            SAMPLER(sampler_MainTexture);

            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);

            TEXTURE2D(_Flow);
            SAMPLER(sampler_Flow);

            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);

            // =========================================================
            // Vertex input
            // =========================================================
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR;

                // uv0.xy = обычный UV
                // uv0.zw = Custom1.xy
                float4 uv0        : TEXCOORD0;

                // uv1.xy = UV2
                // uv1.z  = Custom2.x
                // uv1.w  = Custom2.y
                float4 uv1        : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // =========================================================
            // Data passed to fragment
            // =========================================================
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float4 uv0        : TEXCOORD0;
                float4 uv1        : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // =========================================================
            // Helper: remap noise like in ASE-generated shader
            //
            // Оставляем именно эту логику, чтобы поведение было тем же.
            // При powerZ = 1 получаем noise почти без изменений.
            // =========================================================
            float RemapNoisePower(float noiseValue, float powerZ)
            {
                return (1.0 - powerZ) + noiseValue * (powerZ - (1.0 - powerZ));
            }

            // =========================================================
            // Helper: pan UV over time
            // =========================================================
            float2 PanUV(float2 uv, float2 speed, float timeValue)
            {
                return uv + speed * timeValue;
            }

            // =========================================================
            // Helper: same UV2 switch logic as original ASE shader
            //
            // switch OFF:
            //   distortion multiplier = 1
            //   pow exponent          = T
            //
            // switch ON:
            //   distortion multiplier = T
            //   pow exponent          = 1
            // =========================================================
            void GetUV2SwitchValues(float customT, out float distortionMul, out float powerValue)
            {
                if (_UV2Tswitch > 0.5)
                {
                    distortionMul = customT;
                    powerValue = 1.0;
                }
                else
                {
                    distortionMul = 1.0;
                    powerValue = customT;
                }
            }

            // =========================================================
            // Vertex shader
            // =========================================================
            Varyings vert(Attributes v)
            {
                Varyings o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);

                o.positionCS = positionInputs.positionCS;
                o.color = v.color;
                o.uv0 = v.uv0;
                o.uv1 = v.uv1;

                return o;
            }

            // =========================================================
            // Fragment shader
            // =========================================================
            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // -----------------------------------------------------
                // Base inputs
                // -----------------------------------------------------
                float4 vertexColor = i.color;

                // Particle custom streams:
                // Custom1.xy -> uv offset
                float2 customUVOffset = i.uv0.zw;

                // Custom2.x -> additional alpha multiplier
                float customW = i.uv1.z;

                // Custom2.y -> used in UV2 switch logic
                float customT = i.uv1.w;

                // Current shader time
                float timeValue = _Time.y;

                // -----------------------------------------------------
                // UV2 switch values
                // -----------------------------------------------------
                float distortionMul;
                float powerValue;
                GetUV2SwitchValues(customT, distortionMul, powerValue);

                // -----------------------------------------------------
                // NOISE UV + sample
                // -----------------------------------------------------
                float2 noiseUVBase = i.uv0.xy * _Noise_ST.xy + _Noise_ST.zw;
                float2 noiseUV = PanUV(noiseUVBase, _NoiseSpeedXYPowerZ.xy, timeValue);

                float4 noiseSample = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, noiseUV);

                // Смешиваем noise.r -> 1 через NoiseOpacityLerp
                float noiseOpacity = lerp(noiseSample.r, 1.0, _NoiseOpacityLerp);

                // Оставляем ту же remap-логику, что в оригинале
                float noiseRemap = RemapNoisePower(noiseOpacity, _NoiseSpeedXYPowerZ.z);

                // -----------------------------------------------------
                // FLOW UV + sample
                // -----------------------------------------------------
                float2 flowUVBase = i.uv1.xy * _Flow_ST.xy + _Flow_ST.zw;
                float2 flowUV = PanUV(flowUVBase, _DistortionSpeedXYPowerZ.xy, timeValue);

                // Используем RG flow map
                float2 flowRG = SAMPLE_TEXTURE2D(_Flow, sampler_Flow, flowUV).rg;

                // -----------------------------------------------------
                // MAIN UV + distortion
                // -----------------------------------------------------
                float2 mainUVBase = i.uv0.xy * _MainTexture_ST.xy + _MainTexture_ST.zw;

                // Смещение UV из Custom1.xy
                float2 mainUV = mainUVBase + customUVOffset;

                // Искажение UV через flow
                float2 distortedMainUV = mainUV + (flowRG * _DistortionSpeedXYPowerZ.z * distortionMul);

                float4 mainSample = SAMPLE_TEXTURE2D(_MainTexture, sampler_MainTexture, distortedMainUV);

                // -----------------------------------------------------
                // EDGE MASK
                //
                // Оставляем именно эту логику, потому что она повторяет
                // то, что было в портированном варианте из ASE.
                // -----------------------------------------------------
                float xMask = (ceil(mainUV.x) == 1.0) ? 0.0 : 1.0;
                float yMask = (ceil(mainUV.y) == 1.0) ? 0.0 : 1.0;
                float edgeMask = 1.0 - max(xMask, yMask);

                // -----------------------------------------------------
                // MASK sample
                // -----------------------------------------------------
                float2 maskUV = i.uv0.xy * _Mask_ST.xy + _Mask_ST.zw;
                float maskAlpha = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, maskUV).a;

                // -----------------------------------------------------
                // MAIN color shaping
                //
                // pow(mainSample, powerValue) оставляем, потому что это
                // часть оригинальной логики.
                // -----------------------------------------------------
                float4 mainPow = pow(mainSample, powerValue.xxxx);
                float4 mainShaped = saturate(mainPow);

                // -----------------------------------------------------
                // Final RGB
                // -----------------------------------------------------
                float3 finalRGB =
                    _Emission *
                    (noiseRemap * mainShaped.rgb) *
                    vertexColor.rgb *
                    noiseSample.rgb;

                // -----------------------------------------------------
                // Final Alpha
                // -----------------------------------------------------
                float finalAlpha = saturate(
                    mainSample.a *
                    edgeMask *
                    vertexColor.a *
                    maskAlpha *
                    noiseRemap *
                    customW
                );

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}