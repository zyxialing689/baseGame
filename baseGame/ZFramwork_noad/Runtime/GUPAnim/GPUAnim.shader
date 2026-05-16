Shader "Custom/GPUAnim_Instanced_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalRenderPipeline" }

        Pass
        {
            Tags { "LightMode" = "Universal2D" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
            #pragma multi_compile _ DEBUG_DISPLAY

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            #if USE_SHAPE_LIGHT_TYPE_0
            SHAPE_LIGHT(0)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_1
            SHAPE_LIGHT(1)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_2
            SHAPE_LIGHT(2)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_3
            SHAPE_LIGHT(3)
            #endif

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 fullPixel : TEXCOORD0;
                float4 uvRect : TEXCOORD1;
                float4 offsetFrame : TEXCOORD2;
                float2 lightingUV : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _AlphaCutoff;

            StructuredBuffer<float4x4> _Matrices;
            StructuredBuffer<float4> _FrameUVBuffer;
            StructuredBuffer<float4> _FrameOffsetBuffer;
            StructuredBuffer<float4> _FrameBaseCenterBuffer;
            StructuredBuffer<float4> _DepthBiasBuffer;
            StructuredBuffer<int> _RenderIndices;

            Varyings vert(Attributes v, uint instanceID : SV_InstanceID)
            {
                Varyings o;
                uint roleId = (uint)_RenderIndices[instanceID];
                float4 baseCenter = _FrameBaseCenterBuffer[roleId];
                float2 baseSize = baseCenter.xy;
                float2 centerOffset = baseCenter.zw;
                float2 anchorPixel = float2(baseSize.x * 0.5 + centerOffset.x, centerOffset.y);
                float2 fullPixel = v.uv * baseSize;
                float2 anchoredPosition = (fullPixel - anchorPixel) / max(baseSize.y, 1.0);
                float3 worldPos = mul(_Matrices[roleId], float4(anchoredPosition.x, anchoredPosition.y, 0, 1)).xyz;
                worldPos.y += _DepthBiasBuffer[roleId].y;

                o.positionHCS = TransformWorldToHClip(worldPos);
                float depthBias = _DepthBiasBuffer[roleId].x;
                #if UNITY_REVERSED_Z
                    o.positionHCS.z += depthBias * o.positionHCS.w;
                #else
                    o.positionHCS.z -= depthBias * o.positionHCS.w;
                #endif
                o.lightingUV = ComputeScreenPos(o.positionHCS).xy / o.positionHCS.w;
                o.fullPixel = fullPixel;
                o.uvRect = _FrameUVBuffer[roleId];
                o.offsetFrame = _FrameOffsetBuffer[roleId];
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 framePixel = i.fullPixel - i.offsetFrame.xy;

                if (framePixel.x < 0 || framePixel.y < 0 || framePixel.x > i.offsetFrame.z || framePixel.y > i.offsetFrame.w)
                {
                    discard;
                }

                float2 frameUV = framePixel / max(i.offsetFrame.zw, float2(1.0, 1.0));
                float2 atlasUV = i.uvRect.xy + frameUV * i.uvRect.zw;
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, atlasUV);
                clip(c.a - _AlphaCutoff);

                SurfaceData2D surfaceData;
                InputData2D inputData;
                half4 mask = half4(1, 1, 1, 1);
                InitializeSurfaceData(c.rgb, c.a, mask, surfaceData);
                InitializeInputData(atlasUV, i.lightingUV, inputData);
                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }
    }
}
