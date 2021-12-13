// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UMP/QuadrantCanvas(Gamma)"
{
	Properties
	{
		_MainTex ("Video Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1, 1, 1, 1)
		_XCoord("X Coord", Float) = 0.5
		_YCoord("Y Coord", Float) = 0.5
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"RenderType"="Transparent"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex	: POSITION;
				float2 texcoord	: TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex	: SV_POSITION;
				half2 texcoord	: TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _XCoord;
			float _YCoord;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 tex = float2(i.texcoord.x * -_XCoord, i.texcoord.y * _YCoord + _YCoord);
				return tex2D(_MainTex, tex) * _Color;
			}
		ENDCG
		}
	}
}