// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/GlitchFX" {
Properties {
	_MainTex ("", 2D) = "" {}
	_Amplitude("", float) = 0.0
	_Width("", float) = 0.0
	_Position("", float) = 0.0
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
		sampler2D _MainTex2;
		float _Amplitude;
		float _Width;
		float _Position;

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

			float shift = 0;
			// position in glitch zone 
			if (i.uv.y >= _Position - _Width / 2 && i.uv.y <= _Position + _Width / 2)
			{
				shift = _Amplitude * (1.0f + cos((i.uv.y - (_Position - _Width)) * 2.0f * 3.1415926f / _Width));
			}


			return tex2D(_MainTex, float2(i.uv.x + shift, i.uv.y));
        }
		ENDCG 
	}
}

Fallback off

}