Shader "FaceMesh/Line"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    Buffer<float4> _Vertices;

    float4 Vertex(uint vid : SV_VertexID) : SV_Position
    {
        return UnityObjectToClipPos(float4(_Vertices[vid].xy - 0.5, 0, 1));
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
