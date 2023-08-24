// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

 Shader "FX/Water2" {
Properties { 

	_BaseColor ("Base color", COLOR)  = ( .54, .95, .99, 0.5) 
	_SpecColor ("Specular Material Color", Color) = (1,1,1,1) 
    _Shininess ("Shininess", Float) = 10
	_ShoreTex ("Shore & Foam texture ", 2D) = "black" {} 

	[MaterialToggle] _ShoreThresholdInstead("Use threshold instead of blending?", Float) = 0 
	_ShoreDistThreshold("Shoreline Distance Threshold", Float) = 0.5

	_InvFadeParemeter ("Auto blend parameter (Edge, Shore, Distance scale)", Vector) = (0.2 ,0.39, 0.5, 1.0)

	_BumpTiling ("Foam Tiling", Vector) = (1.0 ,1.0, -2.0, 3.0)
	_BumpDirection ("Foam movement", Vector) = (1.0 ,1.0, -1.0, 1.0) 

	_Foam ("Foam (intensity, cutoff)", Vector) = (0.1, 0.375, 0.0, 0.0) 
	[MaterialToggle] _isInnerAlphaBlendOrColor("Fade inner to color or alpha?", Float) = 0 
	
	_Alpha("Water alpha", Float) =1.0

	[MaterialToggle] _WaveWorldPos("Use world position for vertex waves?", Float) = 1

	_GerstnerIntensity("Vertex Wave Normal Intensity", Float) = 1.0
	_GAmplitude ("Vertex Wave Amplitude", Vector) = (0.3 ,0.35, 0.25, 0.25)
	_GFrequency ("Vertex Wave Frequency", Vector) = (1.3, 1.35, 1.25, 1.25)
	_GSteepness ("Vertex Wave Steepness", Vector) = (1.0, 1.0, 1.0, 1.0)
	_GSpeed ("Vertex Wave Speed", Vector) = (1.2, 1.375, 1.1, 1.5)
	_GDirectionAB ("Vertex Wave Direction 1", Vector) = (0.3 ,0.85, 0.85, 0.25)
	_GDirectionCD ("Vertex Wave Direction 2", Vector) = (0.1 ,0.9, 0.5, 0.5)
}


CGINCLUDE 


	#include "UnityCG.cginc" 
	#include "UnityLightingCommon.cginc" // for _LightColor0
	#include "Water2Includes.cginc"

	sampler2D _ShoreTex;
	sampler2D_float _CameraDepthTexture;
  
	uniform float4 _BaseColor;  
    uniform float _Shininess;
	 
	uniform float4 _InvFadeParemeter;
    
	uniform float4 _BumpTiling;
	uniform float4 _BumpDirection;
 
	uniform float4 _Foam; 
  	float _isInnerAlphaBlendOrColor; 
	#define VERTEX_WORLD_NORMAL i.normalInterpolator.xyz 

	uniform float _Alpha;

	uniform float4 _GAmplitude;
	uniform float4 _GFrequency;
	uniform float4 _GSteepness;
	uniform float4 _GSpeed;
	uniform float4 _GDirectionAB;
	uniform float4 _GDirectionCD;

	float _ShoreThresholdInstead;
	float _ShoreDistThreshold;
	float _WaveWorldPos;

	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};
 
	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float4 normalInterpolator : TEXCOORD0;
		float4 viewInterpolator : TEXCOORD1;
		float4 bumpCoords : TEXCOORD2;
		float4 screenPos : TEXCOORD3;
		float4 grabPassPos : TEXCOORD4; 
		half3 worldRefl : TEXCOORD6;
		float4 posWorld : TEXCOORD7;
        float3 normalDir : TEXCOORD8;

		UNITY_FOG_COORDS(5)
	}; 
 
 
 	struct v2g
    {
        float4 pos : SV_POSITION;
        //float2 uv : TEXCOORD0;
        float3 vertex : TEXCOORD1;
		//float4 normalInterpolator : TEXCOORD0;
		float4 viewInterpolator : TEXCOORD0;
		float4 bumpCoords : TEXCOORD2;
		float4 screenPos : TEXCOORD3;
		float4 grabPassPos : TEXCOORD4; 
		half3 worldRefl : TEXCOORD6;
		float4 posWorld : TEXCOORD7;
        float3 normalDir : TEXCOORD8;
    };
 
    struct g2f
    {
        float4 pos : SV_POSITION;
        //float2 uv : TEXCOORD0;
        //float light : TEXCOORD1;

		float4 viewInterpolator : TEXCOORD1;
		float4 bumpCoords : TEXCOORD2;
		float4 screenPos : TEXCOORD3;
		float4 posWorld : TEXCOORD7;
		float3 normalDir : TEXCOORD8;

    };


	inline half4 Foam(sampler2D shoreTex, half4 coords) 
	{
		half4 foam = (tex2D(shoreTex, coords.xy) * tex2D(shoreTex,coords.zw)) - 0.125;
		return foam;
	}

	v2g vert(appdata_full v)
	{
		v2g o;
		
		half3 worldSpaceVertex = _WaveWorldPos == 1.0 ? mul(unity_ObjectToWorld,(v.vertex)).xyz : v.vertex.xyz;

		//half3 worldSpaceVertex = mul(unity_ObjectToWorld,(v.vertex)).xyz;
		half3 vtxForAni = (worldSpaceVertex).xzz;

		half3 nrml;
		half3 offsets;
		Gerstner (
			offsets, nrml, worldSpaceVertex, vtxForAni,						// offsets, nrml will be written
			_GAmplitude,												// amplitude
			_GFrequency,												// frequency
			_GSteepness,												// steepness
			_GSpeed,													// speed
			_GDirectionAB,												// direction # 1, 2
			_GDirectionCD												// direction # 3, 4
		);


		v.vertex.xyz += offsets;
		o.pos = UnityObjectToClipPos(v.vertex); //NOTE: I moved this up to flatten the surface. for proper 3D displacement put it below '+= offsets'.
		//v.normal = nrml;
		worldSpaceVertex+=offsets;
		half2 tileableUv = mul(unity_ObjectToWorld,(v.vertex)).xz;
		
		o.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

		o.viewInterpolator.xyz = worldSpaceVertex - _WorldSpaceCameraPos;
		
		o.screenPos=ComputeScreenPos(o.pos); 
		
		//o.normalInterpolator.xyz = nrml;
		o.viewInterpolator.w = saturate(offsets.y);
		//o.normalInterpolator.w = 1; 
		
		UNITY_TRANSFER_FOG(o,o.pos);
 		half3 worldNormal = UnityObjectToWorldNormal(v.normal); 
   		float4x4 modelMatrix = unity_ObjectToWorld;
        float4x4 modelMatrixInverse = unity_WorldToObject; 
	 	o.posWorld = mul(modelMatrix, v.vertex);
        o.normalDir = normalize( mul(float4(v.normal, 0.0), modelMatrixInverse).xyz); 

        float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos)); 
        o.worldRefl = reflect(-worldViewDir, worldNormal);
				
		o.vertex = v.vertex;
		return o;
	}

	//WATER4
	/*v2f vert(appdata_full v)
	{
		v2f o;
		
		half3 worldSpaceVertex = mul(unity_ObjectToWorld,(v.vertex)).xyz;
		half3 vtxForAni = (worldSpaceVertex).xzz;

		half3 nrml;
		half3 offsets;
		Gerstner (
			offsets, nrml, v.vertex.xyz, vtxForAni,						// offsets, nrml will be written
			_GAmplitude,												// amplitude
			_GFrequency,												// frequency
			_GSteepness,												// steepness
			_GSpeed,													// speed
			_GDirectionAB,												// direction # 1, 2
			_GDirectionCD												// direction # 3, 4
		);
		
		v.vertex.xyz += offsets;
		
		// one can also use worldSpaceVertex.xz here (speed!), albeit it'll end up a little skewed
		half2 tileableUv = mul(unity_ObjectToWorld,(v.vertex)).xz;
		
		o.bumpCoords.xyzw = (tileableUv.xyxy + _Time.xxxx * _BumpDirection.xyzw) * _BumpTiling.xyzw;

		o.viewInterpolator.xyz = worldSpaceVertex - _WorldSpaceCameraPos;

		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		ComputeScreenAndGrabPassPos(o.pos, o.screenPos, o.grabPassPos);
		
		o.normalInterpolator.xyz = nrml;
		
		o.viewInterpolator.w = saturate(offsets.y);
		o.normalInterpolator.w = 1;//GetDistanceFadeout(o.screenPos.w, DISTANCE_SCALE);
		
		UNITY_TRANSFER_FOG(o,o.pos);
		return o;
	}
	*/
 
	 half4 calculateBaseColor(g2f input)  
         {
            float3 normalDirection = normalize(input.normalDir);
 
            float3 viewDirection = normalize(
               _WorldSpaceCameraPos - input.posWorld.xyz);
            float3 lightDirection;
            float attenuation;
 
            if (0.0 == _WorldSpaceLightPos0.w) // directional light?
            {
               attenuation = 1.0; // no attenuation
               lightDirection = normalize(_WorldSpaceLightPos0.xyz);
            } 
            else // point or spot light
            {
               float3 vertexToLightSource = 
                  _WorldSpaceLightPos0.xyz - input.posWorld.xyz;
               float distance = length(vertexToLightSource);
               attenuation = 1.0 / distance; // linear attenuation 
               lightDirection = normalize(vertexToLightSource);
            }
 
            float3 ambientLighting = 
               UNITY_LIGHTMODEL_AMBIENT.rgb * _BaseColor.rgb;
 
            float3 diffuseReflection = 
               attenuation * _LightColor0.rgb * _BaseColor.rgb
               * max(0.0, dot(normalDirection, lightDirection));
 
            float3 specularReflection;
            if (dot(normalDirection, lightDirection) < 0.0) 
               // light source on the wrong side?
            {
               specularReflection = float3(0.0, 0.0, 0.0); 
                  // no specular reflection
            }
            else  
            {
               specularReflection = attenuation * _LightColor0.rgb  * _SpecColor.rgb * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
            }

            return half4(ambientLighting + diffuseReflection  + specularReflection, _Alpha);
         }

	half4 frag( g2f i ) : SV_Target
	{ 
		//return half4(i.screenPos/100);

		half4 edgeBlendFactors = half4(1.0, 0.0, 0.0, 0.0);

		#ifdef WATER_EDGEBLEND_ON
			half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
			depth = LinearEyeDepth(depth);

			edgeBlendFactors = saturate(_InvFadeParemeter * (depth-i.screenPos.w));

			if(_ShoreThresholdInstead==1.0){
				edgeBlendFactors.x = (depth-i.screenPos.w) > _ShoreDistThreshold;
			}

			edgeBlendFactors.y = 1.0-edgeBlendFactors.y;
		#endif
		


        half4 baseColor = calculateBaseColor(i);
 
		half4 foam = Foam(_ShoreTex, i.bumpCoords * 2.0);
		baseColor.rgb += foam.rgb * _Foam.x * (edgeBlendFactors.y + saturate(i.viewInterpolator.w - _Foam.y));

		if( _isInnerAlphaBlendOrColor==0)
			baseColor.rgb += 1.0-edgeBlendFactors.x;
		if(  _isInnerAlphaBlendOrColor==1.0)
			baseColor.a  =  edgeBlendFactors.x;
		UNITY_APPLY_FOG(i.fogCoord, baseColor);
		return baseColor;
	}


	[maxvertexcount(3)]
    void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
    {
        g2f o;
 
        // Compute the normal
        float3 vecA = IN[1].vertex - IN[0].vertex;
        float3 vecB = IN[2].vertex - IN[0].vertex;
        float3 normal = cross(vecA, vecB);
        normal = normalize(mul(normal, (float3x3) unity_WorldToObject));
 
        // Compute diffuse light
        //float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
        //o.light = max(0., dot(normal, lightDir));
 
        // Compute barycentric uv
        //o.uv = (IN[0].uv + IN[1].uv + IN[2].uv) / 3;

 		o.viewInterpolator = IN[0].viewInterpolator;
		o.bumpCoords = IN[0].bumpCoords;

		o.normalDir = normal;

        for(int i = 0; i < 3; i++)
        {
            o.pos = IN[i].pos;
			o.posWorld = IN[i].posWorld;
			o.screenPos	= IN[i].screenPos;
            triStream.Append(o);
        }
		

    }
	
ENDCG

Subshader
{
	Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
	Lod 500
	ColorMask RGB
	
	GrabPass { "_RefractionTex" }
	
	Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Cull Off
		
			CGPROGRAM
		
			#pragma target 3.0
		
			#pragma vertex vert
			#pragma fragment frag

			#pragma geometry geom
			#pragma multi_compile_fog
		
			#pragma multi_compile WATER_EDGEBLEND_ON WATER_EDGEBLEND_OFF 
		
			ENDCG
	}
}


Fallback "Transparent/Diffuse"
}
