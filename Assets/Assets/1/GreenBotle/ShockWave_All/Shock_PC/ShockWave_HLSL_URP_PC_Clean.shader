Shader "ShockWave_HLSL_URP_PC_Clean"
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

            TEXTURE2D(_MainTexture);
            SAMPLER(sampler_MainTexture);

            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);

            TEXTURE2D(_Flow);
            SAMPLER(sampler_Flow);

            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR;
                float4 uv0        : TEXCOORD0;
                float4 uv1        : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float4 uv0        : TEXCOORD0;
                float4 uv1        : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float RemapNoisePower(float noiseValue, float powerZ)
            {
                return (1.0 - powerZ) + noiseValue * (powerZ - (1.0 - powerZ));
            }

            float2 PanUV(float2 uv, float2 speed, float timeValue)
            {
                return uv + speed * timeValue;
            }

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

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 vertexColor = i.color;

                float2 customUVOffset = i.uv0.zw;
                float customW = i.uv1.z;
                float customT = i.uv1.w;

                float timeValue = _Time.y;

                float distortionMul;
                float powerValue;
                GetUV2SwitchValues(customT, distortionMul, powerValue);

                float2 noiseUVBase = i.uv0.xy * _Noise_ST.xy + _Noise_ST.zw;
                float2 noiseUV = PanUV(noiseUVBase, _NoiseSpeedXYPowerZ.xy, timeValue);

                float4 noiseSample = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, noiseUV);

                float noiseOpacity = lerp(noiseSample.r, 1.0, _NoiseOpacityLerp);
                float noiseRemap = RemapNoisePower(noiseOpacity, _NoiseSpeedXYPowerZ.z);

                float2 flowUVBase = i.uv1.xy * _Flow_ST.xy + _Flow_ST.zw;
                float2 flowUV = PanUV(flowUVBase, _DistortionSpeedXYPowerZ.xy, timeValue);

                float2 flowRG = SAMPLE_TEXTURE2D(_Flow, sampler_Flow, flowUV).rg;

                float2 mainUVBase = i.uv0.xy * _MainTexture_ST.xy + _MainTexture_ST.zw;
                float2 mainUV = mainUVBase + customUVOffset;

                float2 distortedMainUV = mainUV + (flowRG * _DistortionSpeedXYPowerZ.z * distortionMul);

                float4 mainSample = SAMPLE_TEXTURE2D(_MainTexture, sampler_MainTexture, distortedMainUV);

                float xMask = (ceil(mainUV.x) == 1.0) ? 0.0 : 1.0;
                float yMask = (ceil(mainUV.y) == 1.0) ? 0.0 : 1.0;
                float edgeMask = 1.0 - max(xMask, yMask);

                float2 maskUV = i.uv0.xy * _Mask_ST.xy + _Mask_ST.zw;
                float maskAlpha = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, maskUV).a;

                float4 mainPow = pow(mainSample, powerValue.xxxx);
                float4 mainShaped = saturate(mainPow);

                float3 finalRGB =
                    _Emission *
                    (noiseRemap * mainShaped.rgb) *
                    vertexColor.rgb *
                    noiseSample.rgb;

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