Shader"Custom/WallStencilWriter"
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
        
        // 스텐실 속성
        _StencilRef("Stencil Ref", Range(0, 255)) = 1
    }
    
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry-1"
        }
LOD 300
        
        // 스텐실 설정
        Stencil
{
    Ref[_StencilRef]
            Comp Always
            Pass Replace

}
        
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
            
half4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
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
    surfaceData.alpha = albedo.a;
    surfaceData.clearCoatMask = 0;
    surfaceData.clearCoatSmoothness = 0;
                
    return UniversalFragmentPBR(inputData, surfaceData);
}
ENDHLSL
        }
        
        // Shadow caster pass
        Pass
        {
Name"ShadowCaster"
            Tags
{"LightMode" = "ShadowCaster"
}

ZWrite On

ZTest LEqual

ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        
        // Depth pass
        Pass
        {
Name"DepthOnly"
            Tags
{"LightMode" = "DepthOnly"
}

ZWrite On

ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
    
FallBack"Universal Render Pipeline/Lit"
}