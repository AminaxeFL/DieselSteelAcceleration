Shader "Diesel/SpriteGraffiti"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _MaskTex ("Light Mask", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        _MistSpread ("Mist Spread", Float) = 0.018
        _MistStrength ("Mist Strength", Float) = 0.7
        _SprayNoiseScale ("Spray Noise Scale", Float) = 180.0

        _DripLength ("Drip Length", Float) = 0.1
        _DripDensity ("Drip Density", Float) = 48.0
        _DripThreshold ("Drip Threshold", Float) = 0.78

        _WallScale ("Wall Grain Scale", Float) = 140.0
        _WallRoughness ("Wall Roughness", Float) = 0.28
        _WallNormalStrength ("Wall Normal Strength", Float) = 0.35
        _PaintOpacity ("Paint Opacity", Float) = 0.92
        _Saturation ("Saturation", Float) = 1.25
        _Contrast ("Contrast", Float) = 1.12
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
            Name "SpriteGraffiti"
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

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MaskTex_ST;
                float4 _NormalMap_ST;
                half4 _Color;
                float _MistSpread;
                float _MistStrength;
                float _SprayNoiseScale;
                float _DripLength;
                float _DripDensity;
                float _DripThreshold;
                float _WallScale;
                float _WallRoughness;
                float _WallNormalStrength;
                float _PaintOpacity;
                float _Saturation;
                float _Contrast;
            CBUFFER_END

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float FbmWall(float2 p)
            {
                float n = ValueNoise(p) * 0.55;
                n += ValueNoise(p * 2.13 + 7.1) * 0.30;
                n += ValueNoise(p * 4.71 + 19.3) * 0.15;
                return n;
            }

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

                half4 center = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                static const float2 dirs[8] = {
                    float2(1.0, 0.0), float2(-1.0, 0.0), float2(0.0, 1.0), float2(0.0, -1.0),
                    float2(0.707, 0.707), float2(-0.707, 0.707), float2(0.707, -0.707), float2(-0.707, -0.707)
                };

                half4 mistAccum = half4(0.0, 0.0, 0.0, 0.0);
                float mistWeight = 0.0;
                for (int i = 0; i < 8; i++)
                {
                    float jitter = 0.5 + 0.5 * Hash21(uv * _SprayNoiseScale + dirs[i] * 13.7);
                    float2 tapUV = saturate(uv + dirs[i] * (_MistSpread * jitter));
                    half4 s = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, tapUV);
                    mistAccum += s * s.a;
                    mistWeight += s.a;
                }

                half3 mistRgb = mistWeight > 0.001 ? (mistAccum.rgb / mistWeight) : center.rgb;
                float mistAlpha = (mistWeight / 8.0) * _MistStrength;

                float dripCol = floor(uv.x * _DripDensity);
                float dripLocalX = frac(uv.x * _DripDensity) - 0.5;
                float dripRand = Hash21(float2(dripCol, 13.7));
                float dripActive = step(_DripThreshold, dripRand);
                float dripSpan = dripActive * _DripLength * (0.35 + 0.65 * Hash21(float2(dripCol, 79.1)));
                float dripWobble = (ValueNoise(float2(dripCol, uv.y * 18.0)) - 0.5) * 0.25;
                float dripShape = 1.0 - smoothstep(0.08, 0.36, abs(dripLocalX + dripWobble));
                float dripOffsetY = dripSpan * dripShape;

                half4 dripSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, saturate(uv + float2(0.0, dripOffsetY)));
                float dripMask = step(center.a, dripSample.a) * dripActive * dripShape;

                half3 paintRgb = lerp(mistRgb, center.rgb, saturate(center.a * 1.4));
                paintRgb = lerp(paintRgb, dripSample.rgb, dripMask * 0.85);

                float rawAlpha = max(center.a, max(mistAlpha, dripSample.a * dripMask * 0.9));

                float spraySpeckle = Hash21(floor(uv * _SprayNoiseScale * 2.2));
                float wallGrain = FbmWall(uv * _WallScale);
                float wallPits = smoothstep(0.25, 0.78, wallGrain);

                float edgeMist = smoothstep(0.04, 0.85, rawAlpha);
                float dropletMask = saturate(edgeMist + (spraySpeckle - 0.5) * (1.0 - center.a) * 0.65);
                float finalAlpha = dropletMask * lerp(1.0 - _WallRoughness * 0.45, 1.0, wallPits) * saturate(_PaintOpacity);

                if (finalAlpha <= 0.002)
                    discard;

                half3 rgb = paintRgb * _Color.rgb * input.color.rgb;
                float luma = dot(rgb, half3(0.299, 0.587, 0.114));
                rgb = lerp(half3(luma, luma, luma), rgb, _Saturation);
                rgb = (rgb - 0.5) * _Contrast + 0.5;

                float passVariation = 0.88 + 0.24 * ValueNoise(uv * 9.5);
                float wallShade = lerp(1.0 - _WallRoughness, 1.0 + _WallRoughness * 0.18, wallGrain);
                rgb = saturate(rgb * passVariation * wallShade);

                half alpha = finalAlpha * _Color.a * input.color.a;

                float wallDx = FbmWall((uv + float2(0.002, 0.0)) * _WallScale) - wallGrain;
                float wallDy = FbmWall((uv + float2(0.0, 0.002)) * _WallScale) - wallGrain;
                half3 wallNormal = normalize(half3(-wallDx * _WallNormalStrength * 30.0, -wallDy * _WallNormalStrength * 30.0, 1.0));

                half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv);
                half3 baseNormal = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv));
                half3 normalTS = normalize(half3(baseNormal.xy + wallNormal.xy, baseNormal.z));

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
