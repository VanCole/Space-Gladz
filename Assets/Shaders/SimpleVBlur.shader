// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/SimpleVBlur" {
Properties {
	_MainTex ("", 2D) = "" {}
	_NbSample("", int) = 20
	_Shift("", float) = 0.005
}
SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off Fog { Mode off } //Parametrage du shader pour éviter de lire, écrire dans le zbuffer, désactiver le culling et le brouillard sur le polygone

		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma vertex vert
		#pragma fragment frag

		sampler2D _MainTex;
		int _NbSample;
		float _Shift;

		struct Prog2Vertex {
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

			};  	 

		Vertex2Pixel vert (Prog2Vertex i)
		{
			Vertex2Pixel o;
		    o.pos = UnityObjectToClipPos (i.vertex); //Projection du modèle 3D, cette ligne est obligatoire
		    o.uv=i.texcoord; //UV de la texture
		      	
		    return o;
		}

        float4 frag(Vertex2Pixel i) : COLOR 
        {
			float screenRatio = _ScreenParams.y / _ScreenParams.x;

			float gaussian[5] = { 0.16, 0.15, 0.12, 0.09, 0.05 };

			float4 bluredColor = tex2D(_MainTex, i.uv.xy) * gaussian[0];
			for (int k = 1; k < 5; k++)
			{
				bluredColor += tex2D(_MainTex, float2(i.uv.x, i.uv.y + k * _Shift)) * gaussian[k];
				bluredColor += tex2D(_MainTex, float2(i.uv.x, i.uv.y - k * _Shift)) * gaussian[k];
			}
			//bluredColor /= 2 * _NbSample + 1.0f;

            return bluredColor; //On renvoit la couleur reçue
        }
		ENDCG 
	}
}

Fallback off

}