Shader "Outline" {
	Properties {
		_OutlineWidth("Outline Width", Range(0, 1)) = 0.1
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_EnableOutline("Enable Outline", Float) = 1.0
	}

	CGINCLUDE
#include "UnityCG.cginc"

	struct appdata {
		float4 vertex : POSITION;
	};

	struct v2f {
		float4 pos : POSITION;
		float4 color : COLOR;
	};

	uniform float _OutlineWidth;
	uniform float4 _OutlineColor;
	uniform float _EnableOutline;

	v2f vert(appdata v) {
		v2f o;
		v.vertex *= (1 + _OutlineWidth);
		o.pos = UnityObjectToClipPos(v.vertex * _EnableOutline);
		o.color = _OutlineColor;
		return o;
	}
	ENDCG

	SubShader {

		// transparent pass
		Pass {
			Tags { "Queue" = "Transparent" }
			Cull Back
			Blend Zero One
		}

		// outline pass
		Pass {
			Name "Outline"
			Tags { "LightMode" = "Always" }
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha

CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	half4 frag(v2f i) :COLOR { 
		return i.color; 
	}
ENDCG
		}
	}
}