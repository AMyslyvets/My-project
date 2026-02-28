Shader "Custom/BillboardURP"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="AlphaTest" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float2 positionOS : POSITION;  // XY billboard quad coords
                float2 uv : TEXCOORD0;         // UV coordinates
                uint id : SV_InstanceID;       // Instance ID for batching
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // Billboard system-provided global variables
            float3 unity_BillboardCameraPosition;
            float3 unity_BillboardTangent;
            float3 unity_BillboardNormal;
            float unity_BillboardCameraXZAngle;
            float4 unity_BillboardSize; 
            float4 unity_BillboardImageTexCoords; 

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Billboard positioning using tangent (right) & normal (up)
                float3 right = unity_BillboardTangent.xyz * unity_BillboardSize.x;
                float3 up = unity_BillboardNormal.xyz * unity_BillboardSize.y;
                float3 center = float3(0, unity_BillboardSize.y, 0); // Pivot adjustment

                // Compute world position
                float3 pos = center + IN.positionOS.x * right + IN.positionOS.y * up;
                OUT.positionCS = TransformWorldToHClip(pos);

                // UV adjustment from atlas
                OUT.uv.x = unity_BillboardImageTexCoords.x + IN.uv.x * unity_BillboardImageTexCoords.z;
                OUT.uv.y = unity_BillboardImageTexCoords.y + IN.uv.y * unity_BillboardImageTexCoords.w;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                clip(col.a - 0.5); // Alpha cutoff for transparency
                return col;
            }
            ENDHLSL
        }
    }
}