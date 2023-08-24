Shader "Custom/WireframeExpanding" 
{
	Properties 
	{
		_WireColor ("Line Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		_Thickness ("Thickness", Float) = 1
		_SphereCenter ("Expansion Center", Vector) = (0,0,0)
		_SphereRadiusOut ("Outer Expansion Radius", Float) = 100
		_SphereRadiusIn ("Inner Expansion Radius", Float) = 50
	}

	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Transparent" "Queue"="Transparent" }

			Blend SrcAlpha OneMinusSrcAlpha 
			ZWrite Off
			LOD 200
			
			CGPROGRAM
				#pragma target 5.0
				#include "UnityCG.cginc"
				#include "WireframeCustom.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom

				float4 _SphereCenter;
				float _SphereRadiusOut;
				float _SphereRadiusIn;

				// Vertex Shader
				UCLAGL_v2g vert(appdata_base v)
				{
					return UCLAGL_vert(v);
				}
				
				// Geometry Shader
				[maxvertexcount(3)]
				void geom(triangle UCLAGL_v2g p[3], inout TriangleStream<UCLAGL_g2f> triStream)
				{
					UCLAGL_geom(p, triStream);
				}
				
				// Fragment Shader
				float4 frag(UCLAGL_g2f input) : COLOR
				{	
					float4 posRel = input.world - _SphereCenter;
					if(posRel.x*posRel.x+posRel.z*posRel.z>_SphereRadiusOut*_SphereRadiusOut || posRel.x*posRel.x+posRel.z*posRel.z<_SphereRadiusIn*_SphereRadiusIn) return float4(0,0,0,0);
					return UCLAGL_frag(input);
				}
			
			ENDCG
		}
	} 
}
