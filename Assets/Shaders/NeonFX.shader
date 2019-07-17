// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/NeonFX" {
Properties {
	_MainTex ("", 2D) = "" {}
	_NeonTex ("", 2D) = "" {}
	_BlurredNeonTex("", 2D) = "" {}
	_GlowCoef ("", float) = 1.0
	_WhiteCoef("", float) = 1.0
	_WhitePow("", float) = 3.0
}
SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off Fog { Mode off } //Parametrage du shader pour éviter de lire, écrire dans le zbuffer, désactiver le culling et le brouillard sur le polygone

		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma vertex vert
		#pragma fragment frag

		sampler2D _MainTex;
		sampler2D _NeonTex;
		sampler2D _BlurredNeonTex;
		float _GlowCoef;
		float _WhiteCoef;
		float _WhitePow;

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



		float GetDepthValue(float2 uv)
		{
			/*float rawDepth = DecodeFloatRG(tex2D(_CameraDepthTexture, uv));
			float linDepth = Linear01Depth(rawDepth);

			return linDepth * _ProjectionParams.z * 0.01;*/
		}


        float4 frag(Vertex2Pixel i) : COLOR 
		{
			float4 color = tex2D(_MainTex,i.uv.xy);
			float4 blurColor = tex2D(_BlurredNeonTex, i.uv.xy);
			float4 productColor = tex2D(_NeonTex, i.uv.xy) * pow(blurColor, _WhitePow);
			float maxValue = max(productColor.r, max(productColor.g, productColor.b));
			float4 maxColor = float4(maxValue, maxValue, maxValue, productColor.a);
			float4 finalColor = color + _WhiteCoef * maxColor + _GlowCoef * blurColor;

			return finalColor;
        }


		ENDCG 
	}
}

Fallback off

}