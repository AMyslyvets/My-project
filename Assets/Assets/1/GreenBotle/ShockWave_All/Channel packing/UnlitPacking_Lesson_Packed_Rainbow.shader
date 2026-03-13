Shader "Unlit/Packing_Lesson_Packed_Rainbow"
{
    Properties
    {
        _PackedTex ("Packed Texture (R=Noise G=Ring B=Stripes A=Alpha)", 2D) = "white" {}

        _Color ("Base Color", Color) = (0.2, 0.8, 1, 1)
        _Emission ("Emission", Range(0,10)) = 2
        _NoiseStrength ("Noise Strength", Range(0,2)) = 0.5
        _StripeStrength ("Stripe Strength", Range(0,2)) = 1
        _AlphaStrength ("Alpha Strength", Range(0,2)) = 1

        _RainbowSpeed ("Rainbow Speed", Range(0,10)) = 2
        _RainbowScale ("Rainbow Scale", Range(0.1,20)) = 6
        _RainbowDirX ("Rainbow Dir X", Range(-1,1)) = 1
        _RainbowDirY ("Rainbow Dir Y", Range(-1,1)) = 0
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
            Cull Off
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _PackedTex_ST;
                float4 _Color;

                float _Emission;
                float _NoiseStrength;
                float _StripeStrength;
                float _AlphaStrength;

                float _RainbowSpeed;
                float _RainbowScale;
                float _RainbowDirX;
                float _RainbowDirY;
            CBUFFER_END

            TEXTURE2D(_PackedTex);
            SAMPLER(sampler_PackedTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = posInputs.positionCS;
                o.uv = v.uv * _PackedTex_ST.xy + _PackedTex_ST.zw;
                return o;
            }

            half3 RainbowFromValue(half t)
            {
                half3 rgb;
                rgb.r = 0.5h + 0.5h * sin(6.28318h * (t + 0.0h));
                rgb.g = 0.5h + 0.5h * sin(6.28318h * (t + 0.3333h));
                rgb.b = 0.5h + 0.5h * sin(6.28318h * (t + 0.6666h));
                return rgb;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 packed = SAMPLE_TEXTURE2D(_PackedTex, sampler_PackedTex, i.uv);

                half noise  = packed.r;
                half ring   = packed.g;
                half stripe = 1.0h - packed.b;
                half alpha  = packed.a;

                half noiseMul = lerp(1.0h, noise, _NoiseStrength);
                half3 baseRGB = _Color.rgb * ring * noiseMul * _Emission;

                float2 rainbowDir = float2(_RainbowDirX, _RainbowDirY);
                float rainbowCoord = dot(i.uv, rainbowDir) * _RainbowScale + _Time.y * _RainbowSpeed;
                half3 rainbowRGB = RainbowFromValue(frac(rainbowCoord));

                half3 stripeRGB = rainbowRGB * stripe * _StripeStrength;

                half3 finalRGB = baseRGB + stripeRGB;
                half finalA = saturate(ring * alpha * _AlphaStrength * _Color.a);

                return half4(finalRGB, finalA);
            }
            ENDHLSL
        }
    }
}