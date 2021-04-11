Shader "Hidden/MediaPipe/FaceMesh/Wire"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    float2 _Scale, _Offset;
    Buffer<float4> _Vertices;

    float4 Vertex(uint vid : SV_VertexID) : SV_Position
    {
        float2 p = _Vertices[vid].xy;
        p = p * _Scale + _Offset;
        return UnityObjectToClipPos(float4(p - 0.5, 0, 1));
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
