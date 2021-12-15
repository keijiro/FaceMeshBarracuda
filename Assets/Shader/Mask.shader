Shader "Hidden/MediaPipe/FaceMesh/Mask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                uint vid : SV_VertexID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            StructuredBuffer<float4> _Vertices;
            float4x4 _Xform;

            v2f vert (appdata v)
            {
                v2f o;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                if (v.vid < 32)
                {
                    const int indices[] =
                    {
                        0,  1,  1,  2,  2,  3,  3,  4,  4,  5,  5,  6,  6, 7, 7, 8,
                        8, 15, 15, 14, 14, 13, 13, 12, 12, 11, 11, 10, 10, 9, 9, 0
                    };

                    float2 p = _Vertices[indices[v.vid] + 5].xy;

                    o.vertex = UnityObjectToClipPos( float4(p, 0, 1));
                    o.color = float4(0, 1, 1, 1);
                }

                else{ 
                    float2 c = _Vertices[0].xy;
                    float r = distance(_Vertices[1].xy, _Vertices[3].xy) / 2;

                    float phi = UNITY_PI * 2 * (v.vid / 2 + (v.vid & 1) - 16) / 15;
                    float2 p = c + float2(cos(phi), sin(phi)) * r;

                    o.vertex = UnityObjectToClipPos( float4(p, 0, 1));

                    o.color = float4(1,1,0,1);
                }

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //fixed4 col = fixed4(1,1,1,1) + i.color;
                // col = fixed4(1,1,1,1);
                return i.color;
            }
            ENDCG
        }
    }
}
