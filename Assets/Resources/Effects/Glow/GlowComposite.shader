Shader "Hidden/GlowComposite"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Cull Back
		ZWrite Off
		ZTest Always

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
				float4 vertex : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv0 = v.uv;
				o.uv1 = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _GlowPrePassTex;
			sampler2D _GlowBlurPassTex;
			sampler2D _TempTex0;
			float _Intensity;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, i.uv0);
				fixed4 blurred = tex2D(_GlowBlurPassTex, i.uv1);
				fixed4 prepass = tex2D(_GlowPrePassTex, i.uv1);
				fixed4 glow = max(0, blurred - prepass);
				return color +  glow * _Intensity;
			}
			ENDCG
		}
	}
}
