
// Toony Colors Pro+Mobile 2
// (c) 2014,2015 Jean Moreno


Shader "Hidden/Toony Colors Pro 2/Variants/Desktop Reflection Rim"
{
	Properties
	{
		//TOONY COLORS
		_Color ("Color", Color) = (1.0,1.0,1.0,1.0)
		_HColor ("Highlight Color", Color) = (0.6,0.6,0.6,1.0)
		_SColor ("Shadow Color", Color) = (0.4,0.4,0.4,1.0)
		
		//DIFFUSE
		_MainTex ("Main Texture (RGB) Spec/Refl Mask (A) ", 2D) = "white" {}
		
		//TOONY COLORS RAMP
		_Ramp ("#RAMPT# Toon Ramp (RGB)", 2D) = "gray" {}
		_RampThreshold ("#RAMPF# Ramp Threshold", Range(0,1)) = 0.5
		_RampSmooth ("#RAMPF# Ramp Smoothing", Range(0.01,1)) = 0.1
		
		//BUMP
		_BumpMap ("#NORM# Normal map (RGB)", 2D) = "bump" {}
		
		//REFLECTION
		_Cube ("#REFL# Reflection Cubemap", Cube) = "_Skybox" {}
		_ReflectColor ("#REFL# Reflection Color (RGB) Strength (Alpha)", Color) = (1,1,1,0.5)
		_ReflSmoothness ("#REFL_U5# Reflection Smoothness", Range(0.0,1.0)) = 1
		//RIM LIGHT
		_RimColor ("#RIM# Rim Color", Color) = (0.8,0.8,0.8,0.6)
		_RimMin ("#RIM# Rim Min", Range(0,1)) = 0.5
		_RimMax ("#RIM# Rim Max", Range(0,1)) = 1.0
		
		//RIM DIRECTION
		_RimDir ("#RIMDIR# Rim Direction", Vector) = (0.0,0.0,1.0,0.0)
		
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		
		#include "../Include/TCP2_Include.cginc"
		
		#pragma surface surf ToonyColors vertex:vert
		#pragma target 3.0
		#pragma glsl
		
		#pragma shader_feature TCP2_RAMPTEXT
		#pragma shader_feature TCP2_BUMP
		#pragma shader_feature TCP2_RIMDIR
		#pragma shader_feature TCP2_REFLECTION TCP2_REFLECTION_MASKED
		#pragma shader_feature TCP2_U5_REFLPROBE
		#pragma shader_feature TCP2_LIGHTMAP
		
		//================================================================
		// VARIABLES
		
		fixed4 _Color;
		sampler2D _MainTex;
		
	#if TCP2_BUMP
		sampler2D _BumpMap;
	#endif
	#if TCP2_REFLECTION || TCP2_REFLECTION_MASKED
		#if !TCP2_U5_REFLPROBE
		samplerCUBE _Cube;
		#else
		fixed _ReflSmoothness;
		#endif
		fixed4 _ReflectColor;
	#endif
		fixed4 _RimColor;
		fixed _RimMin;
		fixed _RimMax;
		float4 _RimDir;
		
		struct Input
		{
			half2 uv_MainTex : TEXCOORD0;
	#if TCP2_BUMP
			half2 uv_BumpMap : TEXCOORD1;
	#endif
	#if TCP2_REFLECTION || TCP2_REFLECTION_MASKED
			float3 worldRefl;
		#if TCP2_BUMP
			INTERNAL_DATA
		#endif
		#if TCP2_U5_REFLPROBE
			float3 worldPos;
			float3 worldNormal;
		#endif
	#endif
	#if !TCP2_RIMDIR
			float3 viewDir;
	#endif
	#if TCP2_RIMDIR
			float3 bViewDir;
	#endif
		};
		
		//================================================================
		// VERTEX FUNCTION
		
	#if TCP2_RIMDIR
		inline float3 TCP2_ObjSpaceViewDir( in float4 v )
		{
			float3 camPos = _WorldSpaceCameraPos;
			camPos += mul(_RimDir, UNITY_MATRIX_V).xyz;
			float3 objSpaceCameraPos = mul(_World2Object, float4(camPos, 1)).xyz;
			return objSpaceCameraPos - v.xyz;
		}
	#endif
		
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			
		#if TCP2_BUMP && TCP2_RIMDIR
			TANGENT_SPACE_ROTATION;
			o.bViewDir = mul(rotation, TCP2_ObjSpaceViewDir(v.vertex));
		#endif
		}
		
		//================================================================
		// SURFACE FUNCTION
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * _Color.rgb;
			o.Alpha = c.a * _Color.a;
			
	#if TCP2_BUMP
			//Normal map
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	#endif
	#if TCP2_REFLECTION || TCP2_REFLECTION_MASKED
			
		#if TCP2_U5_REFLPROBE
			half3 eyeVec = IN.worldPos.xyz - _WorldSpaceCameraPos.xyz;
			#if TCP2_BUMP
			half3 worldNormal = reflect(eyeVec, WorldNormalVector(IN, o.Normal));
			#else
			half3 worldNormal = reflect(eyeVec, IN.worldNormal);
			#endif
			
			float oneMinusRoughness = _ReflSmoothness;
			fixed3 reflColor = fixed3(0,0,0);
			#if UNITY_SPECCUBE_BOX_PROJECTION
			half3 worldNormal0 = BoxProjectedCubemapDirection (worldNormal, IN.worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
			#else
			half3 worldNormal0 = worldNormal;
			#endif
			half3 env0 = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, worldNormal0, 1-oneMinusRoughness);
			
			#if UNITY_SPECCUBE_BLENDING
			const float kBlendFactor = 0.99999;
			float blendLerp = unity_SpecCube0_BoxMin.w;
			UNITY_BRANCH
			if (blendLerp < kBlendFactor)
			{
				#if UNITY_SPECCUBE_BOX_PROJECTION
				half3 worldNormal1 = BoxProjectedCubemapDirection (worldNormal, IN.worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
				#else
				half3 worldNormal1 = worldNormal;
				#endif
				
				half3 env1 = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(unity_SpecCube1), unity_SpecCube1_HDR, worldNormal1, 1-oneMinusRoughness);
				reflColor = lerp(env1, env0, blendLerp);
			}
			else
			{
				reflColor = env0;
			}
			#else
			reflColor = env0;
			#endif
			reflColor *= 0.5;
		#else
			#if TCP2_BUMP
			half3 worldRefl = WorldReflectionVector(IN, o.Normal);
			#else
			half3 worldRefl = IN.worldRefl;
			#endif
			fixed4 reflColor = texCUBE(_Cube, worldRefl);
		#endif
		#if TCP2_REFLECTION_MASKED
			reflColor.rgb *= c.a;
		#endif
			reflColor.rgb *= _ReflectColor.rgb * _ReflectColor.a;
			o.Emission += reflColor.rgb;
	#endif
			//Rim Light
		#if TCP2_RIMDIR && TCP2_BUMP
			float3 viewDir = normalize(IN.bViewDir);
		#elif TCP2_RIMDIR
			_RimDir.x += UNITY_MATRIX_MV[0][3] * (1/UNITY_MATRIX_MV[2][3]) * (1-UNITY_MATRIX_P[3][3]);
			_RimDir.y += UNITY_MATRIX_MV[1][3] * (1/UNITY_MATRIX_MV[2][3]) * (1-UNITY_MATRIX_P[3][3]);
			float3 viewDir = normalize(UNITY_MATRIX_V[0].xyz * _RimDir.x + UNITY_MATRIX_V[1].xyz * _RimDir.y + UNITY_MATRIX_V[2].xyz * _RimDir.z);
		#else
			float3 viewDir = normalize(IN.viewDir);
		#endif
			half rim = 1.0f - saturate( dot(viewDir, o.Normal) );
			rim = smoothstep(_RimMin, _RimMax, rim);
			o.Emission += (_RimColor.rgb * rim) * _RimColor.a;
		}
		
		ENDCG
		
	}
	
	Fallback "Diffuse"
	CustomEditor "TCP2_MaterialInspector"
}