Shader "Hidden/MediaPipe/FaceMesh/Crop"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float _Size;
    float4x4 _Xform;

    float4 Fragment(float4 vertex : SV_Position,
                    float2 uv : TEXCOORD0) : SV_Target
    {
        //拡大行列の宣言
        float4x4 resizeMatrix = { 1, 0, 0, 0,
                                  0, 1, 0, 0,
                                  0, 0, 1, 0,
                                  0, 0, 0, 1 };  
        //拡大率の適用
        _Xform = mul( _Xform, resizeMatrix);

        //クロップ
        uv = mul(_Xform, float4(uv, 0, 1)).xy;
        return tex2D(_MainTex, uv);
    }

    ENDCG

    SubShader
    {
        Cull Off ZTest Always ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Fragment
            ENDCG
        }
    }
}
