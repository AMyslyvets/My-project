Shader "ShockWave_HLSL_URP_2_Packed_Final"
{
    Properties
    {
        [MainTexture] _MainTexture ("Main Texture", 2D) = "white" {}
        _PackedMap ("Packed Map (RG=Flow, B=Noise, A=Mask)", 2D) = "black" {}

        _FlowTilingOffset ("Flow Tiling Offset", Vector) = (1,1,0,0)
        _NoiseTilingOffset ("Noise Tiling Offset", Vector) = (1,1,0,0)
        _MaskTilingOffset ("Mask Tiling Offset", Vector) = (1,1,0,0)

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
                float4 _FlowTilingOffset;
                float4 _NoiseTilingOffset;
                float4 _MaskTilingOffset;
                float4 _DistortionSpeedXYPowerZ;
                float4 _NoiseSpeedXYPowerZ;
                float _Emission;
                float _NoiseOpacityLerp;
                float _UV2Tswitch;
            CBUFFER_END

            TEXTURE2D(_MainTexture);
            SAMPLER(sampler_MainTexture);

            TEXTURE2D(_PackedMap);
            SAMPLER(sampler_PackedMap);

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

                float2 custom1 = i.uv0.zw;
                float W = i.uv1.z;
                float T = i.uv1.w;

                float uv2SwitchA = (_UV2Tswitch > 0.5) ? T : 1.0;
                float uv2SwitchB = (_UV2Tswitch > 0.5) ? 1.0 : T;

                float2 noiseBaseUV = i.uv0.xy * _NoiseTilingOffset.xy + _NoiseTilingOffset.zw;
                float2 noisePan = _NoiseSpeedXYPowerZ.xy * _Time.y;
                float2 noiseUV = noiseBaseUV + noisePan;

                float packedNoiseB = SAMPLE_TEXTURE2D(_PackedMap, sampler_PackedMap, noiseUV).b;
                float noiseValue = (packedNoiseB <= 0.001) ? 1.0 : packedNoiseB; // if B is empty/black, use white fallback so the effect does not disappear
                float3 noiseRGB = noiseValue.xxx;

                float noiseLerp = lerp(noiseValue, 1.0, _NoiseOpacityLerp);
                float noiseRemap = RemapNoisePower(noiseLerp, _NoiseSpeedXYPowerZ.z);

                float2 flowBaseUV = i.uv1.xy * _FlowTilingOffset.xy + _FlowTilingOffset.zw;
                float2 flowPan = _DistortionSpeedXYPowerZ.xy * _Time.y;
                float2 flowUV = flowBaseUV + flowPan;

                float2 flowRG = SAMPLE_TEXTURE2D(_PackedMap, sampler_PackedMap, flowUV).rg;

                float2 mainBaseUV = i.uv0.xy * _MainTexture_ST.xy + _MainTexture_ST.zw;
                float2 mainUVSum = mainBaseUV + custom1;

                float2 distortedUV = mainUVSum + (flowRG * _DistortionSpeedXYPowerZ.z * uv2SwitchA);

                float4 mainTex = SAMPLE_TEXTURE2D(_MainTexture, sampler_MainTexture, distortedUV);

                float xMask = (ceil(mainUVSum.x) == 1.0) ? 0.0 : 1.0;
                float yMask = (ceil(mainUVSum.y) == 1.0) ? 0.0 : 1.0;
                float edgeMask = 1.0 - max(xMask, yMask); // masks out pixels when main UV goes outside the 0..1 area, preventing edge artifacts

                float2 maskUV = i.uv0.xy * _MaskTilingOffset.xy + _MaskTilingOffset.zw;
                float maskA = SAMPLE_TEXTURE2D(_PackedMap, sampler_PackedMap, maskUV).a;

                float4 powMain = pow(mainTex, uv2SwitchB.xxxx);
                float4 mainSat = saturate(powMain);

                float3 finalRGB =
                    _Emission *
                    (noiseRemap * mainSat.rgb) *
                    vertexColor.rgb *
                    noiseRGB;

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