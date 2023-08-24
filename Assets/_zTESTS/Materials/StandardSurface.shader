// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/StandardSurface" {
	Properties {
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0, 1.5)) = 1.2
		_MainTex("Main Texture", 2D) = "white" {}
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

/*v2f vert2(appdata v) {
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.color = float4(1,1,1,1);
	return o;
}*/

v2f vertScale(appdata v) {
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex*_Outline);
	//o.pos = UnityObjectToClipPos(v.vertex);
	//o.pos.xy *= _Outline;
	//float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
	//float2 offset = TransformViewToProjection(norm.xy);

	//o.pos.xy += offset * o.pos.z * _Outline;
	o.color = _OutlineColor;
	return o;
}

half4 frag(v2f i) :COLOR{
	return i.color;
}

v2f vert2(appdata v) {
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.color = float4(1,1,1,1);
	return o;
}

ENDCG

SubShader{


	Pass{
		Tags{ "Queue" = "Geometry" }

		Name "OUTLINE"
	//Tags { "LightMode" = "Always" }
	Cull Front
	ZWrite Off
	//Offset 800, 800

	Blend SrcAlpha OneMinusSrcAlpha // Normal

	CGPROGRAM
	#pragma vertex vertScale
	#pragma fragment frag
	ENDCG
}


Pass{
	Name "Surface"
	//Tags{ "RenderType" = "Fade" }
	//LOD 200

	CGPROGRAM
	// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf Standard
	// Use shader model 3.0 target, to get nicer looking lighting
	//#pragma target 5.0
	sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex;
	};

	//half _Glossiness;
	//half _Metallic;
	fixed4 _Color;

	void surf(Input IN, inout SurfaceOutputStandard o) {
		// Albedo comes from a texture tinted by color
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
		o.Albedo = c.rgb;
		// Metallic and smoothness come from slider variables
		o.Metallic = 0;
		o.Smoothness = 0;
		//o.Normal += tex2D(_NormalMap, IN.uv_MainTex);

		//float4 posRel = input.world - _SphereCenter;
		//if(posRel.x*posRel.x+posRel.z*posRel.z>_SphereRadiusOut*_SphereRadiusOut || posRel.x*posRel.x+posRel.z*posRel.z<_SphereRadiusIn*_SphereRadiusIn) return float4(0,0,0,0);

		o.Alpha = c.a;
	}
	ENDCG
}

		/*Pass{
			Tags{ "Queue" = "Transparent" }


			Name "Main"
			//Cull Back
			//Blend Zero One
			ZWrite On

			Blend SrcAlpha OneMinusSrcAlpha // Normal

			CGPROGRAM
		#pragma vertex vert2
		#pragma fragment frag
			ENDCG
		}*/

		Pass{ //outline 2
			Tags{ "Queue" = "Transparent" }

			Cull Front
			ZWrite On
			//ZTest Less
			//Offset 800, 800
			Blend Zero One
			//Blend SrcAlpha OneMinusSrcAlpha // Normal

			CGPROGRAM
		#pragma vertex vertScale
		#pragma fragment frag
			ENDCG
		}
	}
	FallBack "Diffuse"
}
