Shader "ShockWave_HLSL_URP_3"
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

        _Thickness ("Thickness", Range(0.5, 5)) = 1
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
                float _Thickness;
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
                float4 uv0        : TEXCOORD0; // xy = UV, zw = Custom1.xy
                float4 uv1        : TEXCOORD1; // xy = UV2, zw = Custom2.xy
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

            float2 ScaleUVFromCenter(float2 uv, float scaleValue)
            {
                return ((uv - 0.5) / scaleValue) + 0.5;
            }

            Varyings vert(Attributes v)
            {
                Varyings o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);

                o.positionCS = posInputs.positionCS;
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

                // Custom data from particle streams
                float2 custom1 = i.uv0.zw; // Custom1.xy
                float W = i.uv1.z;         // Custom2.x
                float T = i.uv1.w;         // Custom2.y

                // Thickness control
                float2 thickUV = ScaleUVFromCenter(i.uv0.xy, _Thickness);

                // -------------------------
                // NOISE
                // -------------------------
                float2 noiseBaseUV = i.uv0.xy * _Noise_ST.xy + _Noise_ST.zw;
                float2 noisePan = _NoiseSpeedXYPowerZ.xy * _Time.y;
                float2 noiseUV = noiseBaseUV + noisePan;

                float4 noiseTex = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, noiseUV);

                float noiseLerp = lerp(noiseTex.r, 1.0, _NoiseOpacityLerp);
                float noiseRemap = RemapNoisePower(noiseLerp, _NoiseSpeedXYPowerZ.z);

                // -------------------------
                // FLOW
                // -------------------------
                float2 flowBaseUV = i.uv1.xy * _Flow_ST.xy + _Flow_ST.zw;
                float2 flowPan = _DistortionSpeedXYPowerZ.xy * _Time.y;
                float2 flowUV = flowBaseUV + flowPan;

                float2 flowRG = SAMPLE_TEXTURE2D(_Flow, sampler_Flow, flowUV).rg;

                // UV2Tswitch logic from ASE
                float uv2SwitchA = (_UV2Tswitch > 0.5) ? T : 1.0;
                float uv2SwitchB = (_UV2Tswitch > 0.5) ? 1.0 : T;

                // -------------------------
                // MAIN TEX
                // -------------------------
                float2 mainBaseUV = thickUV * _MainTexture_ST.xy + _MainTexture_ST.zw;
                float2 mainUVSum = mainBaseUV + custom1;

                float2 distortedUV = mainUVSum + (flowRG * _DistortionSpeedXYPowerZ.z * uv2SwitchA);

                float4 mainTex = SAMPLE_TEXTURE2D(_MainTexture, sampler_MainTexture, distortedUV);

                // -------------------------
                // EDGE MASK (same idea as ASE)
                // -------------------------
                float xMask = (ceil(mainUVSum.x) == 1.0) ? 0.0 : 1.0;
                float yMask = (ceil(mainUVSum.y) == 1.0) ? 0.0 : 1.0;
                float edgeMask = 1.0 - max(xMask, yMask);

                // -------------------------
                // MASK
                // -------------------------
                float2 maskUV = thickUV * _Mask_ST.xy + _Mask_ST.zw;
                float maskA = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, maskUV).a;

                // -------------------------
                // RGB
                // -------------------------
                float4 powMain = pow(mainTex, uv2SwitchB.xxxx);
                float4 mainSat = saturate(powMain);

                float3 finalRGB =
                    _Emission *
                    (noiseRemap * mainSat.rgb) *
                    vertexColor.rgb *
                    noiseTex.rgb;

                // -------------------------
                // ALPHA
                // -------------------------
                float finalA = saturate(
                    mainTex.a *
                    edgeMask *
                    vertexColor.a *
                    maskA *
                    noiseRemap *
                    W
                );

                return half4(finalRGB, finalA);
            }
            ENDHLSL
        }
    }
}