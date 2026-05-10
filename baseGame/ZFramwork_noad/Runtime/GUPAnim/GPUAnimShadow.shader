Shader "Custom/GPUAnim_Shadow_URP"
{
    Properties
    {
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.01
        _ShadowEnabled ("Shadow Enabled", Float) = 1
        _ShadowColor ("Shadow Color", Color) = (0,0,0,0.35)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalRenderPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 fullPixel : TEXCOORD0;
                float2 shadowCenter : TEXCOORD1;
                float2 shadowSize : TEXCOORD2;
            };

            float _AlphaCutoff;
            float _ShadowEnabled;
            half4 _ShadowColor;

            StructuredBuffer<float4x4> _Matrices;
            StructuredBuffer<float4> _FrameBaseCenterBuffer;
            StructuredBuffer<float4> _FrameShadowBuffer;
            StructuredBuffer<float4> _DepthBiasBuffer;
            StructuredBuffer<int> _RenderIndices;

            Varyings vert(Attributes v, uint instanceID : SV_InstanceID)
            {
                Varyings o;
                uint roleId = (uint)_RenderIndices[instanceID];
                float4 baseCenter = _FrameBaseCenterBuffer[roleId];
                float4 shadowData = _FrameShadowBuffer[roleId];
                float2 baseSize = baseCenter.xy;
                float2 centerOffset = baseCenter.zw;
                float2 shadowOffset = shadowData.xy;
                float2 shadowSize = max(shadowData.zw, float2(1.0, 1.0));
                float2 anchorPixel = float2(baseSize.x * 0.5 + centerOffset.x, centerOffset.y);
                float2 shadowCenter = anchorPixel + shadowOffset;
                float2 fullPixel = lerp(shadowCenter - shadowSize, shadowCenter + shadowSize, v.uv);
                float2 anchoredPosition = (fullPixel - anchorPixel) / max(baseSize.y, 1.0);
                float3 worldPos = mul(_Matrices[roleId], float4(anchoredPosition.x, anchoredPosition.y, 0, 1)).xyz;

                o.positionHCS = TransformWorldToHClip(worldPos);
                float depthBias = _DepthBiasBuffer[roleId].x;
                #if UNITY_REVERSED_Z
                    o.positionHCS.z += depthBias * o.positionHCS.w;
                #else
                    o.positionHCS.z -= depthBias * o.positionHCS.w;
                #endif
                o.fullPixel = fullPixel;
                o.shadowCenter = shadowCenter;
                o.shadowSize = shadowSize;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 shadowDelta = (i.fullPixel - i.shadowCenter) / i.shadowSize;
                half shadowAlpha = smoothstep(1.0, 0.25, dot(shadowDelta, shadowDelta)) * _ShadowColor.a * _ShadowEnabled;

                clip(shadowAlpha - _AlphaCutoff);
                return half4(_ShadowColor.rgb, shadowAlpha);
            }
            ENDHLSL
        }
    }
}
