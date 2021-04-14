Shader "Hidden/MediaPipe/FaceMesh/Surface"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    StructuredBuffer<float4> _Vertices;

    void Vertex(uint vid : SV_VertexID,
                float2 uv : TEXCOORD0,
                out float4 outVertex : SV_Position,
                out float2 outUV : TEXCOORD0)
    {
        outVertex = UnityObjectToClipPos(_Vertices[vid]);
        outUV = uv;
    }

    float4 Fragment(float4 vertex : SV_Position,
                    float2 uv : TEXCOORD0) : SV_Target
    {
        return tex2D(_MainTex, uv);
    }

    ENDCG

    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Cull Off
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
