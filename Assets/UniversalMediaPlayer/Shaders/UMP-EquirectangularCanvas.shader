// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UMP/Equirectangular"
{
	Properties
	{
		_MainTex("Video Texture", 2D) = "gray" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Rotation("Rotation", Float) = 0
	}
		
	SubShader
	{
		Pass
		{
			Tags{ "LightMode" = "Always" }
			Cull Front
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			#pragma target 3.0

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 normal : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
				return o;
			}

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _Rotation;

			inline float2 RadialCoords(float3 a_coords)
			{
				const float Deg2Rad = (UNITY_PI * 2.0) / 360.0;
				float rotationRadians = _Rotation * Deg2Rad;

				float3 a_coords_n = normalize(a_coords);
				float lon = -atan2(a_coords_n.z, a_coords_n.x) + rotationRadians;
				float lat = acos(a_coords_n.y);
				float2 sphereCoords = float2(lon, lat) * (1.0 / UNITY_PI);
				return float2(sphereCoords.x * 0.5 + 0.5, 1 - sphereCoords.y);
			}

			float4 frag(v2f IN) : COLOR
			{
				float2 equiUV = TRANSFORM_TEX(RadialCoords(IN.normal), _MainTex);
				return tex2D(_MainTex, equiUV) * _Color;
			}
			ENDCG
		}
	}
	FallBack "VertexLit"
}