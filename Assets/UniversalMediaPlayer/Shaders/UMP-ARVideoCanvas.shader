// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UMP/ARVideoCanvas"
{
	Properties
	{
		_MainTex ("Video Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1, 1, 1, 1)
		_BorderColor("Border Color", Color) = (0, 0, 0, 0)
		_BorderOffsetLeft("Border Offset Left", Range(0, 0.5)) = 0.001
		_BorderOffsetRight("Border Offset Right", Range(0, 0.5)) = 0.001
		_BorderOffsetTop("Border Offset Top", Range(0, 0.5)) = 0.001
		_BorderOffsetBottom("Border Offset Bottom", Range(0, 0.5)) = 0.001
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
			fixed4 _BorderColor;
			float _BorderOffsetLeft;
			float _BorderOffsetRight;
			float _BorderOffsetTop;
			float _BorderOffsetBottom;
			float _AlphaPower;
			float _BorderUWidth;
			float _BorderVWidth;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 c = _BorderColor;

				if (i.texcoord.x > _BorderUWidth && i.texcoord.x < 1 - _BorderUWidth && 
					i.texcoord.y > _BorderVWidth && i.texcoord.y < 1 - _BorderVWidth)
				{
					float2 texCoord = float2((i.texcoord.x - _BorderUWidth) * (1 / (1 - (_BorderUWidth * 2))), (i.texcoord.y - _BorderVWidth) / (1 - (_BorderVWidth * 2)));
					
					if (texCoord.x > _BorderOffsetLeft && texCoord.x < 1 - _BorderOffsetRight &&
						texCoord.y > _BorderOffsetBottom && texCoord.y < 1 - _BorderOffsetTop)
					{
						float2 texTrans = TRANSFORM_TEX(texCoord, _MainTex);

						c = tex2D(_MainTex, texTrans) * _Color;
						c.a = c.a * _AlphaPower;
					}
				}
				return c;// pow(c, 2.2f);
			}
		ENDCG
		}
	}
}