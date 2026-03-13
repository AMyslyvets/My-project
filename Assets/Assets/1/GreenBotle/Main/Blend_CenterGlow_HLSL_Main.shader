Shader "Blend_CenterGlow_HLSL_Main"

{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _Noise ("Noise", 2D) = "white" {}
        _Flow ("Flow", 2D) = "white" {}
        _Mask ("Mask", 2D) = "white" {}

        _SpeedMainTexUVNoiseZW ("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
        _DistortionSpeedXYPowerZ ("Distortion Speed XY Power Z", Vector) = (0,0,0,0)

        _Emission ("Emission", Float) = 2
        _Color ("Color", Color) = (0.5,0.5,0.5,1)
        _Opacity ("Opacity", Range(0,3)) = 1

        [Toggle] _Usecenterglow ("Use center glow?", Float) = 0
        [Toggle] _Usedepth ("Use depth?", Float) = 0
        _Depthpower ("Depth power", Float) = 1

        [Enum(Off,0,Front,1,Back,2)] _CullMode ("Culling", Float) = 0
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

            Tags
            {
                "LightMode"="UniversalForward"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB
            Cull [_CullMode]
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM

            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);

            TEXTURE2D(_Flow);
            SAMPLER(sampler_Flow);

            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Noise_ST;
                float4 _Flow_ST;
                float4 _Mask_ST;

                float4 _SpeedMainTexUVNoiseZW;
                float4 _DistortionSpeedXYPowerZ;

                float4 _Color;
                float _Emission;
                float _Opacity;
                float _Usecenterglow;
                float _Usedepth;
                float _Depthpower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float4 texcoord   : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS      : SV_POSITION;
                float4 color           : COLOR;
                float4 texcoord        : TEXCOORD0;
                float4 screenPos       : TEXCOORD1;
                float  particleEyeDepth: TEXCOORD2;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);

                o.positionCS = posInputs.positionCS;
                o.screenPos = ComputeScreenPos(posInputs.positionCS);
                o.particleEyeDepth = -posInputs.positionVS.z;

                o.color = v.color;
                o.texcoord = v.texcoord;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 mainSpeed  = float2(_SpeedMainTexUVNoiseZW.x, _SpeedMainTexUVNoiseZW.y);
                float2 noiseSpeed = float2(_SpeedMainTexUVNoiseZW.z, _SpeedMainTexUVNoiseZW.w);

                float2 flowSpeed = float2(_DistortionSpeedXYPowerZ.x, _DistortionSpeedXYPowerZ.y);
                float flowPower = _DistortionSpeedXYPowerZ.z;

                float2 uvMain = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                float2 uvNoise = i.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
                float2 uvFlow = i.texcoord.xy * _Flow_ST.xy + _Flow_ST.zw;
                float2 uvMask = i.texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;

                float2 mainPan = uvMain + _Time.y * mainSpeed;
                float2 noisePan = uvNoise + _Time.y * noiseSpeed;
                float2 flowPan = uvFlow + _Time.y * flowSpeed;

                float4 maskTex = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uvMask);
                float4 flowTex = SAMPLE_TEXTURE2D(_Flow, sampler_Flow, flowPan);

                float2 distortion = (flowTex * maskTex).rg * flowPower;

                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainPan - distortion);
                float4 noiseTex = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, noisePan);

                float3 baseColor = (mainTex.rgb * noiseTex.rgb) * _Color.rgb * i.color.rgb;

                float centerMask = 1.0 - i.texcoord.z;
                float glowMask = saturate(maskTex.r * (maskTex.r - centerMask));

                float3 finalRGB = lerp(baseColor, baseColor * glowMask, _Usecenterglow) * _Emission;

                float finalA = mainTex.a * noiseTex.a * _Color.a * i.color.a * _Opacity;

                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);

                float depthFade = saturate((sceneEyeDepth - i.particleEyeDepth) / max(_Depthpower, 0.0001));
                finalA *= lerp(1.0, depthFade, _Usedepth);

                return half4(finalRGB, finalA);
            }

            ENDHLSL
        }
    }
}