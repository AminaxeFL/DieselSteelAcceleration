Shader "Diesel/FastReflection2D"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _ReflectionTex ("Reflection Texture", 2D) = "black" {}
        _MaskTex ("Light Mask", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Float) = 0.45
        _FadeStart ("Fade Start", Float) = 0.0
        _FadeEnd ("Fade End", Float) = 1.0
        _FadePower ("Fade Power", Float) = 1.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "FastReflection2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex LitVertex
            #pragma fragment LitFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma target 3.0

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
            };

            struct Varyings
            {
                COMMON_2D_LIT_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"

            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _ReflectionTex_ST;
                float4 _MaskTex_ST;
                float4 _NormalMap_ST;
                half4 _Color;
                float _Intensity;
                float _FadeStart;
                float _FadeEnd;
                float _FadePower;
            CBUFFER_END

            Varyings LitVertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonLitVertex(input);
                o.color = input.color;

                return o;
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                if (uv.y >= 0.5)
                    discard;

                float dist = (0.5 - uv.y) * 2.0;

                float2 sampleUV = float2(uv.x, dist);

                half4 tex = SAMPLE_TEXTURE2D(_ReflectionTex, sampler_ReflectionTex, saturate(sampleUV));
                if (tex.a <= 0.001)
                    discard;

                float fade = pow(saturate(1.0 - smoothstep(_FadeStart, _FadeEnd, dist)), max(_FadePower, 0.01));
                half3 rgb = tex.rgb * _Color.rgb * input.color.rgb;
                half alpha = tex.a * _Color.a * input.color.a * saturate(_Intensity) * fade;

                half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv);
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv));

                SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(rgb, alpha, maskTex, normalTS, surfaceData);
                InitializeInputData(uv, input.lightingUV, inputData);

                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
