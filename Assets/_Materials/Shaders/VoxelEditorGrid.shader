Shader "Custom/VoxelEditorGrid"
{
	Properties
	{
		//_MainTex ("Texture", 2D) = "white" {}

		_Color("Color", color) = (1,1,1,1)

		_Thickness("Line Thickness", float) = 0.5
		_GlowStr("Glow Strength", float) = 0.2
		_GlowDist("Glow Distance", float) = 0.2
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "Render"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass
		{
			CGPROGRAM

			//#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos: TEXCOORD0;
			};

			float4 _Color;
			float _Thickness;
			float _GlowDist;
			float _GlowStr;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex - float4(0, 0, 0, 1));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _Color;

				col.a=0;

				float distX = abs(i.worldPos.x % 1);
				float distZ = abs(i.worldPos.z % 1);

				if(distX>.5) distX = 1-distX;
				if(distZ>.5) distZ = 1-distZ;

				float dist = min(distX, distZ);

				if(dist<_Thickness) col.a=1;

				else col.a = lerp(_GlowStr,0, (dist-_Thickness)/_GlowDist);

				return col;
			}
			ENDCG
		}
	}
}
