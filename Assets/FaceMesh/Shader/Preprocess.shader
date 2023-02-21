Shader "Hidden/MediaPipe/FaceMesh/Preprocess"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4x4 _Xform;

    void Vertex(float4 vertex : POSITION,
                float2 uv : TEXCOORD0,
                out float4 outVertex : SV_Position,
                out float2 outUV : TEXCOORD0)
    {
        outVertex = UnityObjectToClipPos(vertex);
        outUV = uv;
    }

    float4 Fragment(float4 vertex : SV_Position,
                    half2 uv : TEXCOORD0) : SV_Target
    {
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
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
