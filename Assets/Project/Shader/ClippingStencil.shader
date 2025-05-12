Shader"Custom/ClippingStencil" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _StencilRef ("Stencil Reference Value", Range(0, 255)) = 1
        
        // Ŭ���� �÷��� �Ӽ�
        _ClipPlaneOrigin ("Clip Plane Origin", Vector) = (0, 0, 0, 0)
        _ClipPlaneNormal ("Clip Plane Normal", Vector) = (0, 0, 0, 0)
        _ClipProgress ("Clip Progress", Range(0, 1)) = 0
    }
    
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        
        Stencil {
Ref[_StencilRef]
Comp equal
            Pass keep
        }
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"
            
struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
};
            
struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float4 worldPos : TEXCOORD1;
};
            
sampler2D _MainTex;
float4 _MainTex_ST;
fixed4 _Color;
            
float4 _ClipPlaneOrigin;
float4 _ClipPlaneNormal;
float _ClipProgress;
            
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    return o;
}
            
fixed4 frag(v2f i) : SV_Target
{
                // ���� ��ġ�� Ŭ���� �÷��� ���� ���� ���� ���
    float3 toPoint = i.worldPos.xyz - _ClipPlaneOrigin.xyz;
                
                // Ŭ���� �÷��� �븻 ���������� ���� �Ÿ� ���
    float distance = dot(toPoint, _ClipPlaneNormal.xyz);
                
                // Ŭ���� ���࿡ ���� �ȼ� ����
    clip(distance + _ClipProgress);
                
    fixed4 col = tex2D(_MainTex, i.uv) * _Color;
    return col;
}
            ENDCG
        }
    }
FallBack"Unlit/Texture"
}