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
    float4x4 _XForm;

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
        return float4(1, 1, 1, 0.5);
    }

    //
    // Keypoint marking
    //

    float4 VertexMark(uint vid : SV_VertexID) : SV_Position
    {
        const uint vindices[] = {
            1,           // noteTip
            205,         // rightCheek
            425,         // leftCheek
            33, 133,     // rightEyeLower0
            263, 362,    // leftEyeLower0
            168,         // midwayBetweenEyes
            78, 13, 308, // lipsUpperInner
            14,          // lipsLowerInner
            70, 55,      // rightEyebrowUpper
            300, 285     // leftEyebrowUpper
        };

        float4 p = _Vertices[vindices[vid / 4 % 16]];

        uint tid = vid & 3;
        p.x += ((tid & 1) - 0.5) * (tid < 2) * 0.05;
        p.y += ((tid & 1) - 0.5) * (tid > 1) * 0.05;

        return UnityObjectToClipPos(mul(_XForm, float4(p.xy, 0, 1)));
    }

    float4 FragmentMark(float4 vertex : SV_Position) : SV_Target
    {
        return float4(1, 0, 0, 1);
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
            ZTest Always ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex VertexWire
            #pragma fragment FragmentWire
            ENDCG
        }
        Pass
        {
            ZTest Always ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex VertexMark
            #pragma fragment FragmentMark
            ENDCG
        }
    }
}
