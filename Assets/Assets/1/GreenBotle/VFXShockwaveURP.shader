Shader "VFX/ShockwaveURP"
{
    Properties
    {
        [MainTexture] _MainTex ("Main Texture", 2D) = "white" {}
        _Noise ("Noise", 2D) = "white" {}
        _Flow ("Flow", 2D) = "gray" {}
        _Mask ("Mask", 2D) = "white" {}

        _DistortionSpeedXYPowerZ ("Distortion Speed XY Power Z", Vector) = (0,0.2,-0.5,0.01)
        _NoiseSpeedXYPowerZ ("Noise Speed XY Power Z", Vector) = (0.02,0.5,1,0)

        _NoiseOpacityLerp ("Noise Opacity Lerp", Range(0,1)) = 0
        _UV2TSwitch ("UV2 Switch", Float) = 0
        _Emission ("Emission", Float) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);

            TEXTURE2D(_Flow);
            SAMPLER(sampler_Flow);

            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);

            float4 _MainTex_ST;
            float4 _Noise_ST;
            float4 _Flow_ST;
            float4 _Mask_ST;

            float4 _DistortionSpeedXYPowerZ;
            float4 _NoiseSpeedXYPowerZ;

            float _NoiseOpacityLerp;
            float _UV2TSwitch;
            float _Emission;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.uv2 = IN.uv2;
                OUT.color = IN.color;

                return OUT;
            }

            float2 flowDistortion(float2 uv)
            {
                float2 flowUV = uv + _Time.y * _DistortionSpeedXYPowerZ.xy;

                float2 flow = SAMPLE_TEXTURE2D(_Flow, sampler_Flow, flowUV).rg;

                flow = flow * 2 - 1;

                return flow * _DistortionSpeedXYPowerZ.z;
            }

            float noiseValue(float2 uv)
            {
                float2 noiseUV = uv + _Time.y * _NoiseSpeedXYPowerZ.xy;

                float n = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, noiseUV).r;

                return n * _NoiseSpeedXYPowerZ.z;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = lerp(IN.uv, IN.uv2, _UV2TSwitch);

                float mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r;

                float2 flow = flowDistortion(uv);

                float noise = noiseValue(uv);

                float2 distortion = flow * noise;

                uv += distortion * mask;

                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                col.rgb *= _Emission;

                col *= IN.color;

                col.a *= mask;

                return col;
            }

            ENDHLSL
        }
    }
}