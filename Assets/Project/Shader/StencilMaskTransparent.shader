Shader"Custom/StencilMaskTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StencilRef ("Stencil Reference Value", Range(0, 255)) = 1
        _VertexOffset ("Vertex Offset", Range(0, 0.1)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-1" }
Blend 
SrcAlpha
OneMinusSrcAlpha
        ColorMask 0
ZWrite Off
        
        Stencil
        {
Ref[_StencilRef]
Comp always
            Pass replace
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
    float4 modifiedVertex = v.vertex + float4(v.normal * _VertexOffset, 0);
    o.vertex = UnityObjectToClipPos(modifiedVertex);
    return o;
}
            
fixed4 frag(v2f i) : SV_Target
{
    return fixed4(1, 1, 1, 0); // 완전 투명
}
            ENDCG
        }
    }
FallBack"Unlit/Transparent"
}