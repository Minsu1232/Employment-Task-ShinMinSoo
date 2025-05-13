Shader"Custom/BlockStencilReader"
{
    Properties
    {
        // 기존 URP/Lit 속성들
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}
        
        // 투명도 관련 설정 추가
        [Toggle(_SURFACE_TYPE_TRANSPARENT)] _Surface("Surface Type (Transparent)", Float) = 0
        [HideInInspector] _Blend("Blend Mode", Float) = 0
        [HideInInspector] _SrcBlend("Src Blend", Float) = 1
        [HideInInspector] _DstBlend("Dst Blend", Float) = 0
        [HideInInspector] _ZWrite("Z Write", Float) = 1
        [HideInInspector] _AlphaClip("Alpha Clip", Float) = 0
        
        // 스텐실 및 클리핑 속성
        _StencilRef("Stencil Ref", Range(0, 255)) = 1
        _ClipPlanePos("Clip Plane Position", Vector) = (0,0,0,0)
        _ClipPlaneNormal("Clip Plane Normal", Vector) = (0,0,0,1)
    }
    
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
LOD 300
        
        // 스텐실 설정은 주석 처리 (필요 없음)
        // Stencil 
        // {
        //     Ref [_StencilRef]
        //     Comp NotEqual 
        // }
        
        // Blend 모드 설정
        Blend[_SrcBlend][_DstBlend]
        ZWrite [_ZWrite]
        
        // 기존 URP/Lit 패스들 유지
        Pass
        {
Name"ForwardLit"
            Tags
{"LightMode" = "UniversalForward"
}
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            // 투명도 관련 키워드 추가
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 texcoord : TEXCOORD0;
    float2 lightmapUV : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
};
            
struct Varyings
{
    float2 uv : TEXCOORD0;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
float3 positionWS : TEXCOORD2;
float3 normalWS : TEXCOORD3;
float3 viewDirWS : TEXCOORD4;
#if defined(_NORMALMAP)
                    float4 tangentWS            : TEXCOORD5;
#endif
float4 positionCS : SV_POSITION;
UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_OcclusionMap);
            SAMPLER(sampler_OcclusionMap);
            
            CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BaseColor;
float _Metallic;
float _Smoothness;
float _BumpScale;
float4 _ClipPlanePos;
float4 _ClipPlaneNormal;
float _Surface;
float _Blend;
float _Cutoff;
CBUFFER_END
            
            Varyings vert(
Attributes input)
            {
Varyings output = (Varyings) 0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionWS = vertexInput.
positionWS;
                output.positionCS = vertexInput.
positionCS;
                output.normalWS = normalInput.
normalWS;
                
                #if defined(_NORMALMAP)
                    float sign = input.tangentOS.w * GetOddNegativeScale();
                    output.tangentWS = float4(normalInput.tangentWS.xyz, sign);
                #endif
                
                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
                
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                
                return
output;
            }
            
            // 투명도 처리 함수
float OutputAlpha(float alpha, float surface, float blend)
{
#if defined(_SURFACE_TYPE_TRANSPARENT)
#if defined(_ALPHAPREMULTIPLY_ON)
                        return 1.0; // 프리멀티플라이드 알파
#else
                        return alpha;
#endif
#else
    return 1.0;
#endif
}
            
half4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // 클리핑 플레인 처리
    float3 planePos = _ClipPlanePos.xyz;
    float3 planeNormal = normalize(_ClipPlaneNormal.xyz);
    float dist = dot(input.positionWS - planePos, planeNormal);
                
                // 벽 너머 부분은 렌더링하지 않음
    clip(dist);
                
                // 텍스처 및 색상
    float2 uv = input.uv;
    float4 albedoAlpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    float4 albedo = albedoAlpha * _BaseColor;
                
                // 노말맵
#if defined(_NORMALMAP)
                    float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv), _BumpScale);
                    float3 normalWS = TransformTangentToWorld(normalTS,
                        float3x3(input.tangentWS.xyz, cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w, input.normalWS));
#else
    float3 normalWS = input.normalWS;
#endif
    normalWS = normalize(normalWS);
                
                // PBR 설정
    float metallic = _Metallic;
    float smoothness = _Smoothness;
    float occlusion = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).r;
                
                // 라이팅 계산
    InputData inputData = (InputData) 0;
    inputData.positionWS = input.positionWS;
    inputData.normalWS = normalWS;
    inputData.viewDirectionWS = normalize(input.viewDirWS);
    inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
#ifdef LIGHTMAP_ON
                    inputData.bakedGI = SampleLightmap(input.lightmapUV, normalWS);
#else
    inputData.bakedGI = SampleSH(normalWS);
#endif
                
                // 최종 색상 계산
    SurfaceData surfaceData = (SurfaceData) 0;
    surfaceData.albedo = albedo.rgb;
    surfaceData.metallic = metallic;
    surfaceData.specular = 0;
    surfaceData.smoothness = smoothness;
    surfaceData.occlusion = occlusion;
    surfaceData.emission = 0;
                
                // 투명도 처리
#if defined(_SURFACE_TYPE_TRANSPARENT)
                    surfaceData.alpha = albedo.a;
#else
    surfaceData.alpha = 1.0;
#endif
                
    surfaceData.clearCoatMask = 0;
    surfaceData.clearCoatSmoothness = 0;
                
    half4 color = UniversalFragmentPBR(inputData, surfaceData);
                
                // 최종 알파값 설정
    color.a = OutputAlpha(albedo.a, _Surface, _Blend);
                
    return color;
}
            ENDHLSL
        }
        
        // Shadow caster pass - 커스텀 구조체 사용
        Pass
        {
Name"ShadowCaster"
            Tags
{"LightMode" = "ShadowCaster"
}

ZWrite On

ZTest LEqual

ColorMask 0
            Cull
Back

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
struct ShadowAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct ShadowVaryings
{
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float4 positionCS : SV_POSITION;
};
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BaseColor;
float _Cutoff;
float4 _ClipPlanePos;
float4 _ClipPlaneNormal;
            CBUFFER_END
            
float3 _LightDirection;
            
float4 GetShadowPositionHClip(ShadowAttributes input)
{
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                
#if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#endif
                
    return positionCS;
}

ShadowVaryings ShadowVert(ShadowAttributes input)
{
    ShadowVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
                
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = GetShadowPositionHClip(input);
                
    return output;
}

half4 ShadowFrag(ShadowVaryings input) : SV_TARGET
{
                // 클리핑 플레인 처리
    float3 planePos = _ClipPlanePos.xyz;
    float3 planeNormal = normalize(_ClipPlaneNormal.xyz);
    float dist = dot(input.positionWS - planePos, planeNormal);
                
                // 벽 너머 부분은 렌더링하지 않음
    clip(dist);
                
    float alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a;
    clip(alpha * _BaseColor.a - _Cutoff);
                
    return 0;
}
            ENDHLSL
        }
        
        // Depth only pass - 커스텀 구조체 사용
        Pass
        {
Name"DepthOnly"
            Tags
{"LightMode" = "DepthOnly"
}

ZWrite On

ColorMask 0
            Cull
Back

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
struct DepthAttributes
{
    float4 positionOS : POSITION;
    float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct DepthVaryings
{
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BaseColor;
float _Cutoff;
float4 _ClipPlanePos;
float4 _ClipPlaneNormal;
CBUFFER_END
            
            DepthVaryings DepthVert(
DepthAttributes input)
            {
DepthVaryings output = (DepthVaryings) 0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                
                return
output;
            }

half4 DepthFrag(DepthVaryings input) : SV_TARGET
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // 클리핑 플레인 처리
    float3 planePos = _ClipPlanePos.xyz;
    float3 planeNormal = normalize(_ClipPlaneNormal.xyz);
    float dist = dot(input.positionWS - planePos, planeNormal);
                
                // 벽 너머 부분은 렌더링하지 않음
    clip(dist);
                
    float alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a;
    clip(alpha * _BaseColor.a - _Cutoff);
                
    return 0;
}
            ENDHLSL
        }
    }
    
    // URP Lit 셰이더로 폴백
FallBack"Universal Render Pipeline/Lit"
}