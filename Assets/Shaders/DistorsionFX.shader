Shader "Hidden/DistorsionFX"
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
			float _Amplitude;

			float4 frag (v2f i) : SV_Target
			{
				float time = (_Time.z * 20) % 240;
				float y = 240 * (1 - i.uv.y);
				float2 distord = float2(_Amplitude * sin(0.5*(y - 240)), 0);

				float4 color = tex2D(_MainTex, i.uv);
				if (y > time && y < time + 2)
				{
					color = tex2D(_MainTex, i.uv + distord);
				}

				return color;
			}
			ENDCG
		}
	}
}
