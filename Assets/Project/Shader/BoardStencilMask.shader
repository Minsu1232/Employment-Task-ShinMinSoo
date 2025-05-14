Shader"Custom/BoardStencilMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _StencilRef ("Stencil Reference", Range(0, 255)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
LOD 100
        
        // 보드는 실제로 보이지 않게 설정 (필요시 주석 해제)
        // ColorMask 0
        // ZWrite Off
        
        // 스텐실 버퍼에 벽 영역 표시
        Stencil
{
    Ref[_StencilRef]
            Comp Always
            Pass Replace
            Fail Keep
            ZFail Keep

}
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"
            
struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};
            
struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};
            
sampler2D _MainTex;
float4 _MainTex_ST;
fixed4 _Color;
            
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}
            
fixed4 frag(v2f i) : SV_Target
{
                // 실제 벽과 겹치는 부분의 스텐실만 설정하고 색상은 보이지 않게
    return fixed4(0, 0, 0, 0);
}
 ENDHLSL          
        }
    }
}