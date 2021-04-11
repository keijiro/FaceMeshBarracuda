Shader "Hidden/MediaPipe/FaceMesh/Surface"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    float2 _Scale, _Offset;
    Buffer<float4> _Vertices;
    sampler2D _MainTex;

    void Vertex(uint vid : SV_VertexID,
                float2 uv : TEXCOORD0,
                out float4 outVertex : SV_Position,
                out float2 outUV : TEXCOORD0)
    {
        float2 p = _Vertices[vid].xy * _Scale + _Offset;
        outVertex = UnityObjectToClipPos(float4(p - 0.5, 0, 1));
        outUV = uv;
    }

    float4 Fragment(float4 vertex : SV_Position,
                    float2 uv : TEXCOORD0) : SV_Target
    {
        return tex2D(_MainTex, 1 - uv);
    }

    ENDCG

    SubShader
    {
        Tags { "Queue" = "Overlay" }
        ZTest Always Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
