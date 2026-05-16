Shader "GpuPaperDoll/GpuRoleShadow"
{
    Properties
    {
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.01
        _ShadowLightFade ("Shadow Light Fade", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalRenderPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_ShapeLightTexture0);
            SAMPLER(sampler_ShapeLightTexture0);
            TEXTURE2D(_ShapeLightTexture1);
            SAMPLER(sampler_ShapeLightTexture1);
            TEXTURE2D(_ShapeLightTexture2);
            SAMPLER(sampler_ShapeLightTexture2);
            TEXTURE2D(_ShapeLightTexture3);
            SAMPLER(sampler_ShapeLightTexture3);
            half _HDREmulationScale;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float2 lightingUV : TEXCOORD1;
            };

            float _AlphaCutoff;
            float _ShadowLightFade;
            StructuredBuffer<float4x4> _AgentMatrices;
            StructuredBuffer<float4> _AgentAnimExtraData;
            StructuredBuffer<float4> _ShadowData;
            StructuredBuffer<float4> _ShadowColors;
            StructuredBuffer<int> _ShadowRenderIndices;
            StructuredBuffer<int> _AgentRemap;

            Varyings Vert(Attributes v, uint instanceID : SV_InstanceID)
            {
                Varyings o;
                uint agentIndex = (uint)_ShadowRenderIndices[instanceID];
                uint remappedIdx = (uint)_AgentRemap[agentIndex];
                float4 shadowData = _ShadowData[agentIndex];
                float2 offset = shadowData.xy;
                float2 size = max(shadowData.zw, float2(0.0001, 0.0001));
                float2 localXY = (v.uv - 0.5) * size + offset;
                float3 worldPos = mul(_AgentMatrices[remappedIdx], float4(localXY.x, localXY.y, 0, 1)).xyz;

                o.positionCS = TransformWorldToHClip(worldPos);
                float depthBias = _AgentAnimExtraData[remappedIdx].y;
                #if UNITY_REVERSED_Z
                    o.positionCS.z += depthBias * o.positionCS.w;
                #else
                    o.positionCS.z -= depthBias * o.positionCS.w;
                #endif
                o.lightingUV = ComputeScreenPos(o.positionCS).xy / o.positionCS.w;
                o.uv = v.uv;
                o.color = _ShadowColors[agentIndex];
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 delta = (i.uv - 0.5) * 2.0;
                half shadowAlpha = smoothstep(1.0, 0.25, dot(delta, delta)) * i.color.a;
                clip(shadowAlpha - _AlphaCutoff);

                half4 light =
                    SAMPLE_TEXTURE2D(_ShapeLightTexture0, sampler_ShapeLightTexture0, i.lightingUV) +
                    SAMPLE_TEXTURE2D(_ShapeLightTexture1, sampler_ShapeLightTexture1, i.lightingUV) +
                    SAMPLE_TEXTURE2D(_ShapeLightTexture2, sampler_ShapeLightTexture2, i.lightingUV) +
                    SAMPLE_TEXTURE2D(_ShapeLightTexture3, sampler_ShapeLightTexture3, i.lightingUV);
                float lightStrength = dot((light * _HDREmulationScale).rgb, half3(0.2126, 0.7152, 0.0722));
                shadowAlpha *= lerp(1.0, 0.0, pow(saturate(lightStrength / max(_ShadowLightFade, 0.001)), 3.0));
                shadowAlpha = max(shadowAlpha, 0.001);
                return half4(i.color.rgb, shadowAlpha);
            }
            ENDHLSL
        }
    }
}
