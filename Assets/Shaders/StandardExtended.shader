//http://docs.unity3d.com/Manual/SL-SurfaceShaders.html
//http://docs.unity3d.com/Manual/SL-SurfaceShaderExamples.html
//http://docs.unity3d.com/Manual/SL-SurfaceShaderLightingExamples.html
//http://docs.unity3d.com/Manual/SL-VertexProgramInputs.html


Shader "Custom/StandardExtended" {
	Properties {
		_MainTex ("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)

		_OverColor("Overwrite Color", Color) = (0,0,0,0)
		_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}
		_EmissionColor("Color", Color) = (0, 0, 0)
		_EmissionMap("Emission", 2D) = "white" {}

		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		_GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
			
		[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_MetallicGlossMap("Metallic", 2D) = "white" {}
		[Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0


		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}
	}
	
	Category{
	
	SubShader {
		Tags{ "RenderType" = "Opaque" }
		CGPROGRAM
		//ici, on veut utiliser le modèle d'éclairage SimpleSpecular associé à la "fonction LightingSimpleSpecular", 
		//la fonction "surf" en fonction de surface et la fonction "vertexFunction" en vertex shader
		#pragma surface surf Standard finalcolor:final
		#pragma target 3.0

		//#include "UnityStandardInput.cginc"

		#pragma shader_feature _EMISSION
		#pragma shader_feature _NORMALMAP
		#pragma shader_feature _METALLICGLOSSMAP
		#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

		half4 _Color;
		half4 _OverColor;
		sampler2D _MainTex;
		half3 _EmissionColor;
		sampler2D _EmissionMap;
		half _BumpScale;
		sampler2D _BumpMap;
	

		sampler2D   _SpecGlossMap;
		sampler2D   _MetallicGlossMap;
		half        _Metallic;
		half        _Glossiness;
		half        _GlossMapScale;

		sampler2D   _OcclusionMap;
		half        _OcclusionStrength;


		// COPIED FROM UNITY STANDARD SHADER

		half4 Albedo(float4 texcoords)
		{
			half4 albedo = _Color * tex2D(_MainTex, texcoords.xy);
			return albedo;
		}

		half3 Emission(float2 uv)
		{
		#ifndef _EMISSION
			return 0;
		#else
			return tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
		#endif
		}


		half3 NormalInTangentSpace(float4 texcoords)
		{
			half3 normalTangent = UnpackScaleNormal(tex2D(_BumpMap, texcoords.xy), _BumpScale);
			return normalTangent;
		}


		half2 MetallicGloss(float2 uv)
		{
			half2 mg;

		#ifdef _METALLICGLOSSMAP
			#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			mg.r = tex2D(_MetallicGlossMap, uv).r;
			mg.g = tex2D(_MainTex, uv).a;
#			else
			mg = tex2D(_MetallicGlossMap, uv).ra;
			#endif
			mg.g *= _GlossMapScale;
		#else
			mg.r = _Metallic;
			#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			mg.g = tex2D(_MainTex, uv).a * _GlossMapScale;
			#else
			mg.g = _Glossiness;
			#endif
		#endif
			return mg;
		}

		half Alpha(float2 uv)
		{
#		if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
			return _Color.a;
#		else
			return tex2D(_MainTex, uv).a * _Color.a;
#		endif
		}
		
		 //Structure d'entrée/sortie du vertex shader, alimentée par la carte graphique.
		struct Prog2Vertex {
	        float4 vertex : POSITION;
	        float4 tangent : TANGENT;
	        float3 normal : NORMAL;
	        float4 texcoord : TEXCOORD0;
	        float4 texcoord1 : TEXCOORD1;
	        fixed4 color : COLOR;
	    };
	    
	    //Structure d'entrée/sortie du surface shader.
		struct Input {
			float3 viewDir; //will contain view direction, for computing Parallax effects, rim lighting etc.
			float2 uv_MainTex:TEXCOORD0; //Premier niveau d'UV. Doit etre de la forme uv_<nom d'une property existante>. Incompréhensible.
			float2 uv2_MainTex:TEXCOORD1;//Second niveau d'UV (généralement utilisé pour la lightmap). Doit etre de la forme uv2_<nom d'une property>
			float3 worldPos; //Position dans le monde
			float3 worldRefl; //Vecteur de reflection dans le monde
			float3 worldNormal; //Normale dans le monde. INTERNAL_DATA permet de modifier ce paramètre dans la fonction surf.
			float4 screenPos;
			float depth;
			INTERNAL_DATA		//Obligatoire pour utiliser une normal map. Ne pas oublier d'écrire une valeur par défaut sinon!
		};
			
			
		////Fonction de vertex shader. Optionnelle car le surface shader embarque sa propre fonction interne
		////On peut cependant modifier les données de la structure Prog2Vertex 
		////(mais elles ont déja une valeur par défaut)
		//void vertexFunction (inout Prog2Vertex v) 
		//{
		//	
		//}
		
		//Fonction principale du surface shader
		//Il faut remplir les paramètres albedo, normal, specular, gloss, emission
		void surf (Input i, inout SurfaceOutputStandard o)
		{
			o.Albedo = Albedo(float4(i.uv_MainTex, 0, 0)).rgb;
			o.Alpha = Alpha(i.uv_MainTex);
			o.Normal = NormalInTangentSpace(float4( i.uv_MainTex, 0, 0));
			o.Emission = Emission(i.uv_MainTex); // _EmissionColor * tex2D(_EmissionMap, i.uv_MainTex);

			half2 metalAndSmooth = MetallicGloss(i.uv_MainTex);

			o.Metallic = metalAndSmooth.r;      // 0=non-metal, 1=metal
			o.Smoothness = metalAndSmooth.g;    // 0=rough, 1=smooth

			o.Occlusion = (1 - _OcclusionStrength) + _OcclusionStrength * tex2D(_OcclusionMap, i.uv_MainTex);
		}

		void final(Input i, SurfaceOutputStandard o, inout fixed4 color)
		{
			color = _OverColor.a * fixed4(_OverColor.rgb, 1) + (1 - _OverColor.a) * color;
		}
		
		ENDCG
	}
	Fallback "Standard"

	CustomEditor "StandardExtendedShaderGUI"
	}
}
