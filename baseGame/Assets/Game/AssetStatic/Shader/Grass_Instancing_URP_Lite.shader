Shader "Universal Render Pipeline/Z/2D/Grass_Indirect_URP"
{
    Properties
    {
        _MainTex ("Atlas", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            // ⭐ 关键：开启 instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct GrassData
            {
                float3 pos;
                float rot;
                float2 scale;
            };

            StructuredBuffer<GrassData> _GrassData;
            StructuredBuffer<float4> _UVDataBuffer;

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                // ⭐ 正确写法：直接用SV_InstanceID
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;

                uint id = v.instanceID;

                GrassData data = _GrassData[id];
                float4 uvData = _UVDataBuffer[id];

                float s = sin(radians(data.rot));
                float c = cos(radians(data.rot));

                float2 p = v.positionOS.xy;

                // 缩放
                p *= data.scale;

                // 旋转
                p = float2(
                    p.x * c - p.y * s,
                    p.x * s + p.y * c
                );

                float3 worldPos = float3(p, 0) + data.pos;

                o.positionHCS = TransformWorldToHClip(worldPos);

                o.uv = v.uv * uvData.zw + uvData.xy;

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                clip(col.a - 0.1);

                return col;
            }

            ENDHLSL
        }
    }
}