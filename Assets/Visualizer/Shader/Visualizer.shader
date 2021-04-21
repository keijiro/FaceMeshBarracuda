Shader "Hidden/MediaPipe/FaceMesh//Visualizer"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    //
    // Common uniforms
    //
    StructuredBuffer<float4> _Vertices;
    float4x4 _XForm;

    //
    // Vertex shader for face mesh
    //

    void VertexFace(uint vid : SV_VertexID,
                    float2 uv : TEXCOORD0,
                    out float4 outVertex : SV_Position,
                    out float2 outUV : TEXCOORD0)
    {
        outVertex = UnityObjectToClipPos(_Vertices[vid]);
        outUV = uv;
    }

    //
    // Fragment shaders for face mesh
    //

    // For main view
    float4 FragmentFaceMain(float4 vertex : SV_Position,
                            float2 uv : TEXCOORD0) : SV_Target
    {
        const float repeat = 20;
        const float width = 2;

        float2 ct = abs(0.5 - frac(uv * repeat));
        ct = 1 - saturate(ct / (fwidth(uv * repeat) * width));
        float y = max(ct.x, ct.y);

        float dist = length((uv - 0.5) * 2.2);
        float a = (1 - smoothstep(0.5, 1, dist)) * 0.7;

        return float4(y, y, y, a);
    }

    // For debug view
    float4 FragmentFaceDebug(float4 position : SV_Position) : SV_Target
    {
        return float4(1, 1, 1, 0.8);
    }

    //
    // Vertex shader for iris landmarks
    //

    void VertexEye(uint vid : SV_VertexID,
                   out float4 position : SV_Position,
                   out float4 color : COLOR)
    {
        if (vid < 32)
        {
            const int indices[] =
            {
                0,  1,  1,  2,  2,  3,  3,  4,  4,  5,  5,  6,  6, 7, 7, 8,
                8, 15, 15, 14, 14, 13, 13, 12, 12, 11, 11, 10, 10, 9, 9, 0
            };

            float2 p = _Vertices[indices[vid] + 5].xy;

            position = UnityObjectToClipPos(mul(_XForm, float4(p, 0, 1)));
            color = float4(0, 1, 1, 1);
        }
        else
        {
            float2 c = _Vertices[0].xy;
            float r = distance(_Vertices[1].xy, _Vertices[3].xy) / 2;

            float phi = UNITY_PI * 2 * (vid / 2 + (vid & 1) - 16) / 15;
            float2 p = c + float2(cos(phi), sin(phi)) * r;

            position = UnityObjectToClipPos(mul(_XForm, float4(p, 0, 1)));
            color = float4(1, 1, 0, 1);
        }
    }

    //
    // Fragment shaders for iris landmarks
    //

    // For main view
    float4 FragmentEyeMain(float4 position : SV_Position,
                           float4 color : COLOR) : SV_Target
    {
        return color;
    }

    // For debug view
    float4 FragmentEyeDebug(float4 position : SV_Position,
                            float4 color : COLOR) : SV_Target
    {
        return color;
    }

    ENDCG

    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Cull Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex VertexFace
            #pragma fragment FragmentFaceMain
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex VertexFace
            #pragma fragment FragmentFaceDebug
            ENDCG
        }
        Pass
        {
            ZTest Always
            CGPROGRAM
            #pragma vertex VertexEye
            #pragma fragment FragmentEyeMain
            ENDCG
        }
        Pass
        {
            ZTest Always
            CGPROGRAM
            #pragma vertex VertexEye
            #pragma fragment FragmentEyeDebug
            ENDCG
        }
    }
}
