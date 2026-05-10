Shader "GpuPaperDoll/GpuRoleShadow"
{
    Properties
    {
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.01
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
            };

            float _AlphaCutoff;
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
                o.uv = v.uv;
                o.color = _ShadowColors[agentIndex];
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 delta = (i.uv - 0.5) * 2.0;
                half shadowAlpha = smoothstep(1.0, 0.25, dot(delta, delta)) * i.color.a;
                clip(shadowAlpha - _AlphaCutoff);
                return half4(i.color.rgb, shadowAlpha);
            }
            ENDHLSL
        }
    }
}
