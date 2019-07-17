Shader "Hidden/NoiseFX"
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
			#include "Includes/noiseSimplex.cginc"

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
			float _Amplitude;

			float4 frag (v2f i) : SV_Target
			{
				float2 aspectCorrection = float2(_ScreenParams.x / _ScreenParams.y, 1);

				float noise = snoise(float3(aspectCorrection * 240 * i.uv, _Time.z * 100));
				float4 noiseColor = float4(noise, noise, noise, 1);
				float4 color = tex2D(_MainTex, i.uv);

				color.a *= 1 - (_Amplitude) * noise;
				return color;
				//return (_Amplitude) * noiseColor  + (1 - _Amplitude) * color;
			}
			ENDCG
		}
	}
}
