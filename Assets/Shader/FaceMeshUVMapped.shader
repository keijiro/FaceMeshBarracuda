Shader "Hidden/MediaPipe/FaceMesh/FaceMeshUVMappded"
//UV展開済の顔テクスチャをmeshに当てるためのshader
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OverTex("Texture", 2D) = "white" {}
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
               //float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            StructuredBuffer<float4> _Vertices;
            float4x4 _Xform;

            v2f vert (appdata v)
            {
                v2f o;

                //座標変換
                o.vertex = mul(_Xform,_Vertices[v.vid]);

                //原点を合わせる
                //(-0.5,-0.5)平行移動する行列
                float4x4 t ={
                1,0,0,-0.5,
                0,1,0,-0.5,
                0,0,1,0,
                0,0,0,1
                };

                o.vertex = mul(t,o.vertex);

                //座標変換してからUnityObjectToClipPos変換
                o.vertex = UnityObjectToClipPos(o.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

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
