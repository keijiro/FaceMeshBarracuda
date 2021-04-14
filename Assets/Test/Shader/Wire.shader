Shader "Hidden/MediaPipe/FaceMesh/Wire"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    StructuredBuffer<float4> _Vertices;

    float4 Vertex(uint vid : SV_VertexID) : SV_Position
    {
        return UnityObjectToClipPos(_Vertices[vid]);
    }

    float4 Fragment(float4 vertex : SV_Position) : SV_Target
    {
        return float4(1, 1, 1, 0.8);
    }

    ENDCG

    SubShader
    {
        Tags { "Queue" = "Overlay" }
        ZTest Always Blend SrcAlpha One
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
