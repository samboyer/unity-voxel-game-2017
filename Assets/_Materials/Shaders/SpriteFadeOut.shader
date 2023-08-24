// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Sprites/SpriteFade"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OverlayTex ("Overlay Texture", 2D) = "white" {}

		_Color("Tint Color", Color) = (1,1,1,1)

		_FadeDistance("Fade Distance (near distance, range)", Vector) = (1,2,0,0)

		[HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
	}
	SubShader
	{
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
		Cull Off
        Lighting Off
        ZWrite On
        Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
			
			#include "UnityCG.cginc"

			
// Material Color.
fixed4 _Color;
fixed4 _RendererColor;
sampler2D _MainTex;
sampler2D _OverlayTex;
sampler2D _CameraDepthTexture;

float4 _FadeDistance;

struct appdata_t
{
    float4 vertex   : POSITION;
    float4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    //float4 screenPos : TEXCOORD1;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex   : SV_POSITION;
    fixed4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
	float4 screenPos : TEXCOORD1;
	float depth : TEXCOORD2;
    UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert(appdata_t v)
{
    v2f OUT;

    UNITY_SETUP_INSTANCE_ID (v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

    OUT.vertex = UnityObjectToClipPos(v.vertex);
    OUT.texcoord = v.texcoord;
    OUT.color = v.color * _Color * _RendererColor;
	
	COMPUTE_EYEDEPTH(OUT.depth);

	OUT.screenPos=ComputeScreenPos(UnityObjectToClipPos(v.vertex)); 

	//OUT.screenPos = ComputeScreenPos(v.vertex);

    return OUT;
}

fixed4 frag(v2f IN) : SV_Target
{
    fixed4 color = tex2D (_MainTex, IN.texcoord) * IN.color;
	fixed4 overlay = tex2D (_OverlayTex, IN.texcoord);
	color = lerp(color, overlay, overlay.a);			
	//half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
	//depth = Linear01Depth(depth);

	//return fixed4(0,0,0,(IN.depth-_FadeDistance.x)/_FadeDistance.y);

	color.a = min(color.a, (IN.depth-_FadeDistance.x)/_FadeDistance.y);

	color.rgb *= color.a;			

    return color;
}
			ENDCG
		}
	}
}
