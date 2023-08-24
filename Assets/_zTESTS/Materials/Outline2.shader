Shader "Outlined/Silhouette2" {
	Properties {
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (1, 1.5)) = 1.2
	}
 
CGINCLUDE
#include "UnityCG.cginc"
 
struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};
 
struct v2f {
	float4 pos : POSITION;
	float4 color : COLOR;
};
 
uniform float _Outline;
uniform float4 _OutlineColor;
 
v2f vert(appdata v) {
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
 
	//float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
	//float2 offset = TransformViewToProjection(norm.xy);
 
	//o.pos.xy += offset * o.pos.z * _Outline;
	o.pos.xy *= _Outline;
	o.color = _OutlineColor;
	return o;
}

half4 frag(v2f i) :COLOR{
	return i.color;
}
ENDCG
 
	SubShader {
		Tags { "Queue" = "Transparent" }
 
		/*Pass {
			Name "BASE"
			Cull Back
			Blend Zero One
			ZWrite On

			// uncomment this to hide inner details:
			Offset -8, -8
 
			SetTexture [_OutlineColor] {
				ConstantColor (0,0,0,0)
				Combine constant
			}
		}*/
 
		// note that a vertex shader is specified here but its using the one above
		Pass {
			Name "OUTLINE"
			//Tags { "LightMode" = "Always" }
			Cull Front
			ZWrite Off
			//ZTest Always
 
			// you can choose what kind of blending mode you want for the outline
			Blend SrcAlpha OneMinusSrcAlpha // Normal

 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		} 
		}
 
	Fallback "Diffuse"
}