Shader "Unlit/TextureComposite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SubTex ("SubTexture", 2D) = "white" {}
        _Blend("Blend",Range (0, 1)) = 1

        _BlendStartU("Blend Start U",Range (0, 1)) = 0
        _BlendEndU("Blend End U",Range (0, 1)) = 1
        _BlendStartV("Blend Start V",Range (0, 1)) = 0
        _BlendEndV("Blend End V",Range (0, 1)) = 1
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
            sampler2D _SubTex;
            float _Blend;
            float4 _MainTex_ST;

            float _BlendStartU;
            float _BlendEndU;
            float _BlendStartV;
            float _BlendEndV;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                 //合成範囲のスケールを算出
                float u_scale = _BlendEndU - _BlendStartU;
                float v_scale = _BlendEndV - _BlendStartV;

                //Sub TextureのUV座標を算出
                fixed2 pos = fixed2((i.uv.x - _BlendStartU) * 1/u_scale,
                                     (i.uv.y - _BlendStartV) * 1/v_scale);

                // sample the texture
                fixed4 main = tex2D(_MainTex, i.uv);
                fixed4 sub = tex2D(_SubTex, pos);
                fixed4 col = main;

                //指定の範囲だったら合成する
                if(i.uv.x >= _BlendStartU && i.uv.y >= _BlendStartV)
                {
                    if(i.uv.x <= _BlendEndU && i.uv.y <= _BlendEndV)
                    {
                        col = main * (1-_Blend) + sub * _Blend;
                    }
                }

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
