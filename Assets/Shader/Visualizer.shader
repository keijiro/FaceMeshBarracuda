Shader "Hidden/MediaPipe/FaceMesh/Visualizer"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    StructuredBuffer<float4> _Vertices;

    //
    // Textured surface rendering
    //

    void VertexTextured(uint vid : SV_VertexID,
                        float2 uv : TEXCOORD0,
                        out float4 outVertex : SV_Position,
                        out float2 outUV : TEXCOORD0)
    {
        outVertex = UnityObjectToClipPos(_Vertices[vid]);
        outUV = uv;
    }

    float4 FragmentTextured(float4 vertex : SV_Position,
                            float2 uv : TEXCOORD0) : SV_Target
    {
        return tex2D(_MainTex, uv);
    }

    //
    // Wireframe mesh rendering
    //

    float4 VertexWire(uint vid : SV_VertexID) : SV_Position
    {
        return UnityObjectToClipPos(_Vertices[vid]);
    }

    float4 FragmentWire(float4 vertex : SV_Position) : SV_Target
    {
        return float4(1, 1, 1, 0.8);
    }

    ENDCG

    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Cull Off
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex VertexTextured
            #pragma fragment FragmentTextured
            ENDCG
        }
        Pass
        {
            Blend SrcAlpha One
            CGPROGRAM
            #pragma vertex VertexWire
            #pragma fragment FragmentWire
            ENDCG
        }
    }
}
