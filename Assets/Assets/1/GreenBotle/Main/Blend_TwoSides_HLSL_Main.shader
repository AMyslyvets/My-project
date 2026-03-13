Shader "Blend_TwoSides_HLSL_Main"

{
    Properties
    {
        _Cutoff ("Mask Clip Value", Float) = 0.5
        _MainTex ("Main Tex", 2D) = "white" {}
        _Mask ("Mask", 2D) = "white" {}
        _Noise ("Noise", 2D) = "white" {}
        _SpeedMainTexUVNoiseZW ("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)

        _Emission ("Emission", Float) = 2
        [Toggle] _UseFresnel ("Use Fresnel?", Float) = 1
        [Toggle] _Usesmoothcorners ("Use smooth corners?", Float) = 0
        _Fresnel ("Fresnel", Float) = 1
        _FresnelEmission ("Fresnel Emission", Float) = 1
        [Toggle] _SeparateFresnel ("SeparateFresnel", Float) = 0
        _SeparateEmission ("Separate Emission", Float) = 2

        _FresnelColor ("Fresnel Color", Color) = (0.3568628,0.08627451,0.08627451,1)
        _FrontFacesColor ("Front Faces Color", Color) = (0,0.2313726,1,1)
        _BackFacesColor ("Back Faces Color", Color) = (0,0.02397324,0.509434,1)
        _BackFresnelColor ("Back Fresnel Color", Color) = (0.3568628,0.08627451,0.08627451,1)

        [Toggle] _UseBackFresnel ("Use Back Fresnel?", Float) = 1
        _BackFresnel ("Back Fresnel", Float) = -2
        _BackFresnelEmission ("Back Fresnel Emission", Float) = 1

        [Toggle] _UseCustomData ("Use Custom Data?", Float) = 0
        [Toggle] _Sideopacity ("Side opacity", Float) = 0

        [HideInInspector] _texcoord ("", 2D) = "white" {}
        [HideInInspector] __dirty ("", Int) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "PreviewType"="Plane"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM

            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);

            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise);

            CBUFFER_START(UnityPerMaterial)
                float _Cutoff;
                float4 _MainTex_ST;
                float4 _Mask_ST;
                float4 _Noise_ST;
                float4 _SpeedMainTexUVNoiseZW;

                float _Emission;
                float _UseFresnel;
                float _Usesmoothcorners;
                float _Fresnel;
                float _FresnelEmission;
                float _SeparateFresnel;
                float _SeparateEmission;

                float4 _FresnelColor;
                float4 _FrontFacesColor;
                float4 _BackFacesColor;
                float4 _BackFresnelColor;

                float _UseBackFresnel;
                float _BackFresnel;
                float _BackFresnelEmission;

                float _UseCustomData;
                float _Sideopacity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR;
                float4 texcoord   : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float4 texcoord   : TEXCOORD2;
                float4 color      : COLOR;
                float  fogFactor  : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 TransformUV4(float4 uv, float4 st)
            {
                return float4(uv.xy * st.xy + st.zw, uv.z, uv.w);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS);

                o.positionCS = posInputs.positionCS;
                o.positionWS = posInputs.positionWS;
                o.normalWS = normalize(normalInputs.normalWS);
                o.texcoord = v.texcoord;
                o.color = v.color;
                o.fogFactor = ComputeFogFactor(posInputs.positionCS.z);

                return o;
            }

            half4 frag(Varyings i, half facing : VFACE) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float3 normalWS = normalize(i.normalWS);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(i.positionWS));

                float4 uvsNoise = TransformUV4(i.texcoord, _Noise_ST);
                float2 noiseSpeed = float2(_SpeedMainTexUVNoiseZW.z, _SpeedMainTexUVNoiseZW.w);

                float2 uvMask = i.texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
                float4 maskTex = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uvMask);
                float4 noiseTex = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, (uvsNoise.xy + (_Time.y * noiseSpeed) + uvsNoise.w));

                float customDataMul = (_UseCustomData > 0.5) ? uvsNoise.z : 1.0;
                float4 temp70 = maskTex * noiseTex * customDataMul;

                float4 noise156 = saturate(-1.0 + temp70 * 3.0);

                float fresnelFront = pow(1.0 - dot(normalWS, viewDirWS), _Fresnel);
                float fresnelBack  = pow(1.0 - dot(normalWS, viewDirWS), _BackFresnel);

                float4 frontColor =
                    (_UseFresnel > 0.5)
                    ? (
                        (_FrontFacesColor * (1.0 - fresnelFront) * ((_Usesmoothcorners > 0.5) ? noise156 : 1.0.xxxx))
                        + (_FresnelEmission * _FresnelColor * ((_Usesmoothcorners > 0.5) ? saturate(fresnelFront.xxxx + (1.0.xxxx - noise156)) : fresnelFront.xxxx))
                      )
                    : _FrontFacesColor;

                float4 backColor =
                    (_Usesmoothcorners > 0.5)
                    ? (
                        (_UseBackFresnel > 0.5)
                        ? (
                            (_BackFacesColor * (1.0 - fresnelBack) * noise156)
                            + (_BackFresnelEmission * _BackFresnelColor * saturate(fresnelBack.xxxx + (1.0.xxxx - noise156)))
                          )
                        : _BackFacesColor
                      )
                    : _BackFacesColor;

                float isFrontFace = (facing >= 0.0h) ? 1.0 : 0.0;
                float4 faceColor = lerp(backColor, frontColor, isFrontFace);

                float4 uvsMain = TransformUV4(i.texcoord, _MainTex_ST);
                float2 mainSpeed = float2(_SpeedMainTexUVNoiseZW.x, _SpeedMainTexUVNoiseZW.y);
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (uvsMain.xy + (mainSpeed * _Time.y)));

                float4 emissionColor =
                    (_SeparateFresnel > 0.5)
                    ? ((faceColor + (_FresnelColor * mainTex * _SeparateEmission)) * _Emission * i.color)
                    : (faceColor * _Emission * i.color * mainTex);

                float alphaValue =
                    (_Sideopacity > 0.5)
                    ? (i.color.a * saturate((temp70.r - 1.0) + ((_UseCustomData > 0.5) ? uvsNoise.z : 1.0)))
                    : i.color.a;

                clip(temp70.r - _Cutoff);

                half4 finalColor = half4(emissionColor.rgb, alphaValue);
                finalColor.rgb = MixFog(finalColor.rgb, i.fogFactor);

                return finalColor;
            }

            ENDHLSL
        }
    }
}