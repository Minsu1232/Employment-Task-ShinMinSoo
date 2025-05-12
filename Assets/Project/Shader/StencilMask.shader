// Stencil Mask Shader
Shader"Custom/StencilMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StencilRef ("Stencil Reference Value", Int) = 1
        _VertexOffset ("Vertex Offset", Float) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
ColorMask 0
        ZWrite
Off
        
        Stencil
{
    Ref[_StencilRef]
            Comp
    Always
            Pass
    Replace

}
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

sampler2D _MainTex;
float4 _MainTex_ST;
float _VertexOffset;

v2f vert(appdata v)
{
    v2f o;
                // 버텍스를 노멀 방향으로 약간 확장
    float3 expandedVertex = v.vertex.xyz + v.normal * _VertexOffset;
    o.vertex = UnityObjectToClipPos(float4(expandedVertex, 1.0));
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
                // 색상은 출력되지 않음 (ColorMask 0)
    return fixed4(1, 1, 1, 1);
}
            ENDCG
        }
    }
}