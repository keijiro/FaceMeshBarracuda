// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UMP/TransparentCanvas"
{
	Properties
	{
		_MainTex("Video Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1, 1, 1, 1)
		_TransparentColor("Transparent Color", Color) = (1, 1, 1, 1)
		_TransparentPower("Transparent Power", Float) = 1
		_BorderColor("Border Color", Color) = (0, 0, 0, 0)
		_AlphaPower("Alpha Power", Float) = 1
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
			fixed4 _TransparentColor;
			float _TransparentPower;
			fixed4 _BorderColor;
			float _AlphaPower;
			float _BorderUWidth;
			float _BorderVWidth;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 c = _BorderColor;

				if (abs(i.texcoord.x) > _BorderUWidth && abs(i.texcoord.x) < 1 - _BorderUWidth &&
					abs(i.texcoord.y) > _BorderVWidth && abs(i.texcoord.y) < 1 - _BorderVWidth)
				{
					float2 tex = float2((i.texcoord.x - _BorderUWidth * _MainTex_ST.x) * (1 / (1 - (_BorderUWidth * 2))), (i.texcoord.y - _BorderVWidth * _MainTex_ST.y) * (1 / (1 - (_BorderVWidth * 2))));
					c = tex2D(_MainTex, tex) * _Color;

					float delta = c.rgb - _TransparentColor.rgb;

					if (abs(delta) <= _TransparentPower)
					{
						c.a = 0;
						return c;
					}

					c.a = c.a * _AlphaPower;
				}
				return c;
			}
		ENDCG
		}
	}
}