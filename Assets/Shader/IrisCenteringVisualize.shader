Shader "Hidden/MediaPipe/FaceMesh/IrisCenteringVisualize"
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4x4 _CropMatrix;
            StructuredBuffer<float4> _Vertices;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
   
                //瞳が中心になるよう平行移動
                //中心座標
                float4 c = _Vertices[0];

                //変換行列
                float4x4 tf =
                {1,0,0,c.x-0.5,
                0,1,0, c.y-0.5,
                0,0,1,0,
                0,0,0,1};

                //クロップ
                tf = mul(_CropMatrix,tf);

                //行列演算
                float2 uv = mul(tf, float4(v.uv, 0, 1)).xy;    

                o.uv = uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
