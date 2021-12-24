Shader "Hidden/MediaPipe/FaceMesh/IrisMesh"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                uint vid : SV_VertexID;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
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

                
                //中心と半径を使って円を描く
                float2 c = _Vertices[0].xy;
                float r = distance(_Vertices[1].xy, _Vertices[3].xy) / 2;

                float phi = UNITY_PI * 2 * ((float)v.vid / (float)15);

                float2 p = c + float2(cos(phi), sin(phi)) * r;
                
                o.vertex = float4(p, 0, 1);

                o.uv = TRANSFORM_TEX(o.vertex.xy, _MainTex);

                //座標変換
                o.vertex = mul(_Xform,o.vertex);

                //原点を合わせる
                //(-0.5,-0.5)平行移動する行列
                float4x4 t ={
                1,0,0,-0.5,
                0,1,0,-0.5,
                0,0,1,0,
                0,0,0,1
                };

                o.vertex = mul(t,o.vertex);
                o.vertex = UnityObjectToClipPos(o.vertex);

                o.color  = float4(1, 1, 0, 1);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
               fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
