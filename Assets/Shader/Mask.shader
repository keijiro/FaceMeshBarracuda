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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            StructuredBuffer<float4> _Vertices;//マスクする多角形の頂点
            const uint _Count;//_Verticesの要素数
            float4x4 _Xform;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                float2 p = i.uv;
                //Crossing Number Algorithms
                uint cn = 0;
                uint index;
                /*for(index=0; index<_Count-1; index++){

                    // 上向きの辺。点Pがy軸方向について、始点と終点の間にある。ただし、終点は含まない。(ルール1)

                    if(((_Vertices[index].y <= p.y) && (_Vertices[index+1].y > p.y))

                     // 下向きの辺。点Pがy軸方向について、始点と終点の間にある。ただし、始点は含まない。(ルール2)

                     || ((_Vertices[index].y > p.y) && (_Vertices[index+1].y <= p.y))){
                    // ルール1,ルール2を確認することで、ルール3も確認できている。
                        // 辺は点pよりも右側にある。ただし、重ならない。(ルール4)
                        // 辺が点pと同じ高さになる位置を特定し、その時のxの値と点pのxの値を比較する。
                        float vt = (p.y - _Vertices[index].y) / (_Vertices[index+1].y - _Vertices[index].y);
                        if(p.x < (_Vertices[index].x + (vt * (_Vertices[index+1].x - _Vertices[index].x)))){
                            cn++;
                        }
                    }
                }*/
                //cnが奇数なら内側、偶数なら外側にいる

                uint isInside = cn%2;

                //col.w = isInside;
                
                return col;
            }
            ENDCG
        }
    }
}
