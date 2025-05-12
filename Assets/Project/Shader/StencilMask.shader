Shader"Custom/StencilMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StencilRef ("Stencil Reference Value", Range(0, 255)) = 1
        _VertexOffset ("Vertex Offset", Range(0, 0.1)) = 0.01
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
    always
            Pass
    replace

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
};
            
struct v2f
{
    float4 vertex : SV_POSITION;
};
            
float _VertexOffset;
            
v2f vert(appdata v)
{
    v2f o;
                // 버텍스를 노멀 방향으로 약간 확장 (마스크 영역 확대)
    float4 modifiedVertex = v.vertex + float4(v.normal * _VertexOffset, 0);
    o.vertex = UnityObjectToClipPos(modifiedVertex);
    return o;
}
            
fixed4 frag(v2f i) : SV_Target
{
    return fixed4(1, 1, 1, 1);
}
            ENDCG
        }
    }
FallBack"Unlit/Texture"
}