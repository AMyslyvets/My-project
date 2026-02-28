#ifndef B1TH0VEN_SHADOW_CASTER_PASS_GRASS_INCLUDED
#define B1TH0VEN_SHADOW_CASTER_PASS_GRASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

// Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
// For Directional lights, _LightDirection is used when applying shadow Normal Bias.
// For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
float3 _LightDirection;
float3 _LightPosition;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    uint grassInstanceID : SV_InstanceID;
};

struct Varyings
{
    #if defined(_ALPHATEST_ON)
        float2 uv       : TEXCOORD0;
    #endif
    float4 positionCS   : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float4 GetShadowPositionHClip(Attributes input)
{
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

    float2 windUV = TRANSFORM_TEX(positionWS.xz + _WindPosition, _WindMap);

    float windSpeed = SAMPLE_TEXTURE2D_LOD(_WindMap, sampler_WindMap, windUV, 0).r;

    positionWS += windSpeed * TRANSFORM_TEX(input.texcoord, _BaseMap).y * _WindDirection;
    
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}

Varyings ShadowPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    DrawData drawData = (DrawData)0;
    
    #ifdef UNITY_ANY_INSTANCING_ENABLED
    drawData = _DrawData[input.grassInstanceID];
  
    unity_ObjectToWorld = mul(unity_ObjectToWorld, drawData.objectToWorld);

    float3x3 w2oRotation;
    w2oRotation[0] = unity_ObjectToWorld[1].yzx * unity_ObjectToWorld[2].zxy - unity_ObjectToWorld[1].zxy * unity_ObjectToWorld[2].yzx;
    w2oRotation[1] = unity_ObjectToWorld[0].zxy * unity_ObjectToWorld[2].yzx - unity_ObjectToWorld[0].yzx * unity_ObjectToWorld[2].zxy;
    w2oRotation[2] = unity_ObjectToWorld[0].yzx * unity_ObjectToWorld[1].zxy - unity_ObjectToWorld[0].zxy * unity_ObjectToWorld[1].yzx;

    float det = dot(unity_ObjectToWorld[0].xyz, w2oRotation[0]);
    w2oRotation = transpose(w2oRotation);
    w2oRotation *= rcp(det);
    float3 w2oPosition = mul(w2oRotation, -unity_ObjectToWorld._14_24_34);

    unity_WorldToObject._11_21_31_41 = float4(w2oRotation._11_21_31, 0.0f);
    unity_WorldToObject._12_22_32_42 = float4(w2oRotation._12_22_32, 0.0f);
    unity_WorldToObject._13_23_33_43 = float4(w2oRotation._13_23_33, 0.0f);
    unity_WorldToObject._14_24_34_44 = float4(w2oPosition, 1.0f);
    #endif
    
    #if defined(_ALPHATEST_ON)
        output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    #endif

    output.positionCS = GetShadowPositionHClip(input);
    return output;
}

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);

    #if defined(_ALPHATEST_ON)
        Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    #endif

    #if defined(LOD_FADE_CROSSFADE)
        LODFadeCrossFade(input.positionCS);
    #endif

    return 0;
}

#endif
