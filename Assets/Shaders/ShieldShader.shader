//http://docs.unity3d.com/Manual/SL-SurfaceShaders.html
//http://docs.unity3d.com/Manual/SL-SurfaceShaderExamples.html
//http://docs.unity3d.com/Manual/SL-SurfaceShaderLightingExamples.html
//http://docs.unity3d.com/Manual/SL-VertexProgramInputs.html


Shader "Custom/Shader Shield" {
	Properties{
		_MainTex("Texture", 2D) = "black" {}
		_Color("Color", Color) = (1,1,1,1)
		_RimPower("Rim Power", Range(0.5,8.0)) = 1.0
	}

	Category{

		//SubShader{
		//	Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		//	Blend SrcAlpha OneMinusSrcAlpha
		//	ZWrite Off
		//	Pass{
		//		CGPROGRAM
		//		#include "UnityCG.cginc"
		//		#pragma vertex vertex //"vertex" est le nom de la fonction appelée pour le vertex shader
		//		#pragma fragment fragment //"fragment" est le nom de la fonction appelée pour le fragment shader
		//		#pragma target 3.0 //version de la norme de shader visée. <3.0 est préhistorique, > 3.0 nécessite DX11

		//		sampler2D _MainTex;
		//		float4 _MainTex_ST;
		//		float _Alpha;
		//		float4 _Color;
		//		//Le contenu de cette structure sera rempli par la carte graphique
		//		//On peut enlever les attributs dont on a pas besoin mais je les aient tous mis pour le cours.
		//		//Et aussi parce que manipuler ces trucs tient un peu de la magie noire en fait.
		//		struct Prog2Vertex
		//		{
		//			float4 vertex : POSITION; 	//Les "registres" précisés après chaque variable servent
		//			float4 tangent : TANGENT; 	//A savoir ce qu'on est censé attendre de la carte graphique.
		//			float3 normal : NORMAL;		//(ce n'est pas basé sur le nom des variables).
		//			float4 texcoord : TEXCOORD0;
		//			float4 texcoord1 : TEXCOORD1;
		//			fixed4 color : COLOR;
		//		};

		//		//Structure servant a transporter des données du vertex shader au pixel shader.
		//		//C'est au vertex shader de remplir a la main les informations de cette structure.
		//		struct Vertex2Pixel
		//		{
		//			float4 pos : SV_POSITION;
		//			float4 uv : TEXCOORD0;
		//			float3 norm: TEXCOORD1; //On utilise un registre dispo pour la normale.
		//			float3 normscreen: TEXCOORD2; //On utilise un registre dispo pour la normale en screenspace. Ne faites pas gaffe au nom.
		//			float4 position: TEXCOORD3; //On utilise un registre dispo pour la position dans le monde. pas gaffe au nom.
		//			float4 screenPos: TEXCOORD4;
		//		};


		//		//Le vertex shader, qui prend en paramètre la structure Prog2Vertex. 
		//		//Dans le principe seule les deux premières lignes de ce shader sont primordiales.
		//		Vertex2Pixel vertex(Prog2Vertex i)
		//		{
		//			Vertex2Pixel o;

		//			o.pos = UnityObjectToClipPos(i.vertex); //Projection du modèle 3D, cette ligne est obligatoire


		//													 //Quelque données supplémentaires
		//			o.norm = mul((float3x3)unity_ObjectToWorld, float4(i.normal, 1)).xyz;
		//			o.normscreen = normalize(mul(UNITY_MATRIX_IT_MV, float4(i.normal, 1)).xyz); //Normale dans l'espace écran
		//			o.uv = i.texcoord; //UV de la texture
		//			o.position = mul(unity_ObjectToWorld, i.vertex); //Position du vertice dans le monde
		//			o.screenPos = ComputeScreenPos(o.pos); //Position du vertice à l'écran, avec la profondeur.
		//												   //On retourne le set de données vertex2pixel


		//			return o;
		//		}

		//		//Le pixel shader, qui prend en paramètre la structure Vertex2Pixel.
		//		//on est censé retourner une couleur (+ alpha)
		//		float4 fragment(Vertex2Pixel i) : COLOR
		//		{
		//			float4 color = _Color;
		//			color.a = abs(length(i.normscreen.xy)) - 0.5;
		//			color.a = clamp(color.a, 0, 1);

		//			float4 texColor = tex2D(_MainTex, i.uv * _MainTex_ST);
		//			return color + texColor * _Color;
		//		}
		//	ENDCG
		//	}
		//}
		//Fallback "VertexLit"


		SubShader{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			CGPROGRAM
			//ici, on veut utiliser le modèle d'éclairage SimpleSpecular associé à la "fonction LightingSimpleSpecular", 
			//la fonction "surf" en fonction de surface et la fonction "vertexFunction" en vertex shader
			#pragma surface surf Lambert finalcolor:final alpha:blend noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap noforwardadd noshadowmask 
			#pragma target 3.0

			//#include "UnityStandardInput.cginc"

			half4 _Color;
			sampler2D _MainTex; 
			float _RimPower;


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

			//Fonction principale du surface shader
			//Il faut remplir les paramètres albedo, normal, specular, gloss, emission
			void surf(Input i, inout SurfaceOutput o)
			{
			}

			void final(Input i, SurfaceOutput o, inout fixed4 color)
			{
				half rim = pow(1.0 - saturate(dot(normalize(i.viewDir), o.Normal)) , _RimPower);
				fixed4 rimColor = fixed4(_Color.rgb, 1);
				rimColor.a = rim;
				float4 texColor = _Color * tex2D(_MainTex, i.uv_MainTex);
				color = rimColor.a * (rimColor + float4(texColor.rgb, 1) * texColor.a) + (1 - rimColor.a) * texColor;
			}

			ENDCG
		}
		//Fallback "Unlit/Color"
		CustomEditor "ShieldShaderGUI"
	}
}
