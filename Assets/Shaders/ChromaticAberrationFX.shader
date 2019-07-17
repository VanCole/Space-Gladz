Shader "Hidden/ChromatiqueAberrationFX"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _Amplitude = 0;

			float4 frag (v2f i) : SV_Target
			{
				float2 direction = _Amplitude * normalize(float2(1, -1));


				float4 color = tex2D(_MainTex, i.uv);
				color.r = tex2D(_MainTex, i.uv + direction).r; // red
				color.b = tex2D(_MainTex, i.uv - direction).b; // blue
				color.g = tex2D(_MainTex, i.uv).g; // blue

				return color;
			}
			ENDCG
		}
	}
}
