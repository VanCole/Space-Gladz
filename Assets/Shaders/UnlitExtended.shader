//http://docs.unity3d.com/Manual/SL-SurfaceShaders.html
//http://docs.unity3d.com/Manual/SL-SurfaceShaderExamples.html
//http://docs.unity3d.com/Manual/SL-SurfaceShaderLightingExamples.html
//http://docs.unity3d.com/Manual/SL-VertexProgramInputs.html


Shader "Custom/Unlit Extended" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_EmissionColor("Emission", Color) = (0,0,0,0)
		_EmissionMap("Emissive map", 2D) = "white" {}
	}

	Category{

		SubShader{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Pass{
				CGPROGRAM
				#include "UnityCG.cginc"
				#pragma vertex vertex //"vertex" est le nom de la fonction appelée pour le vertex shader
				#pragma fragment fragment //"fragment" est le nom de la fonction appelée pour le fragment shader
				#pragma target 3.0 //version de la norme de shader visée. <3.0 est préhistorique, > 3.0 nécessite DX11

				sampler2D _MainTex;
				float _Alpha;
				float4 _Color;
				half4 _EmissionColor;
				sampler2D _EmissionMap;
				//Le contenu de cette structure sera rempli par la carte graphique
				//On peut enlever les attributs dont on a pas besoin mais je les aient tous mis pour le cours.
				//Et aussi parce que manipuler ces trucs tient un peu de la magie noire en fait.
				struct Prog2Vertex
				{
					float4 vertex : POSITION; 	//Les "registres" précisés après chaque variable servent
					float4 tangent : TANGENT; 	//A savoir ce qu'on est censé attendre de la carte graphique.
					float3 normal : NORMAL;		//(ce n'est pas basé sur le nom des variables).
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
					fixed4 color : COLOR;
				};

				//Structure servant a transporter des données du vertex shader au pixel shader.
				//C'est au vertex shader de remplir a la main les informations de cette structure.
				struct Vertex2Pixel
				{
					float4 pos : SV_POSITION;
					float4 uv : TEXCOORD0;
					float3 norm: TEXCOORD1; //On utilise un registre dispo pour la normale.
					float3 normscreen: TEXCOORD2; //On utilise un registre dispo pour la normale en screenspace. Ne faites pas gaffe au nom.
					float4 position: TEXCOORD3; //On utilise un registre dispo pour la position dans le monde. pas gaffe au nom.
					float4 screenPos: TEXCOORD4;
					fixed4 color: COLOR;
				};


				//Le vertex shader, qui prend en paramètre la structure Prog2Vertex. 
				//Dans le principe seule les deux premières lignes de ce shader sont primordiales.
				Vertex2Pixel vertex(Prog2Vertex i)
				{
					Vertex2Pixel o;

					o.pos = UnityObjectToClipPos(i.vertex); //Projection du modèle 3D, cette ligne est obligatoire


															 //Quelque données supplémentaires
					o.norm = mul((float3x3)unity_ObjectToWorld, float4(i.normal, 1)).xyz;
					o.normscreen = normalize(mul(UNITY_MATRIX_IT_MV, float4(i.normal, 1)).xyz); //Normale dans l'espace écran
					o.uv = i.texcoord; //UV de la texture
					o.position = mul(unity_ObjectToWorld, i.vertex); //Position du vertice dans le monde
					o.screenPos = ComputeScreenPos(o.pos); //Position du vertice à l'écran, avec la profondeur.
					o.color = i.color;


					return o;
				}

				//Le pixel shader, qui prend en paramètre la structure Vertex2Pixel.
				//on est censé retourner une couleur (+ alpha)
				float4 fragment(Vertex2Pixel i) : COLOR
				{
					return i.color * _Color * tex2D(_MainTex, i.uv.xy);
				}
			ENDCG
			}
		}
		//Fallback "VertexLit"
	}
}
