using UnityEngine;
using Unity.Mathematics;
using UI = UnityEngine.UI;

namespace MediaPipe.FaceMesh {

public sealed class MyVisualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamInput _webcam = null;
    [Space]
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Shader _shader = null;
    [Space]
    [SerializeField] UI.RawImage _mainUI = null;
    [SerializeField] UI.RawImage _faceUI = null;
    [SerializeField] UI.RawImage _leftEyeUI = null;
    [SerializeField] UI.RawImage _rightEyeUI = null;

    #endregion

    #region Private members

    FacePipeline _pipeline;
    Material _material;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _pipeline = new FacePipeline(_resources);
        _material = new Material(_shader);
    }

    void OnDestroy()
    {
        _pipeline.Dispose();
        Destroy(_material);
    }

    void LateUpdate()
    {
        // Processing on the face pipeline
        _pipeline.ProcessImage(_webcam.Texture);

        // UI update
        _mainUI.texture = _webcam.Texture;
        _faceUI.texture = _pipeline.CroppedFaceTexture;
        _leftEyeUI.texture = _pipeline.CroppedLeftEyeTexture;
        _rightEyeUI.texture = _pipeline.CroppedRightEyeTexture;
    }

    void OnRenderObject()
    {
        // Main view overlay
        var mv = float4x4.Translate(math.float3(-0.875f, -0.5f, 0));
        _material.SetBuffer("_Vertices", _pipeline.RefinedFaceVertexBuffer);
        _material.SetPass(1);
        Graphics.DrawMeshNow(_resources.faceLineTemplate, mv);

        // Face view
        // Face mesh
        var fF = MathUtil.ScaleOffset(0.5f, math.float2(0.125f, -0.5f));
        _material.SetBuffer("_Vertices", _pipeline.RefinedFaceVertexBuffer);
        _material.SetPass(0);
        Graphics.DrawMeshNow(_resources.faceMeshTemplate, fF);

        // Left eye
        var fLE = math.mul(fF, _pipeline.LeftEyeCropMatrix);
        _material.SetMatrix("_XForm", fLE);
        _material.SetBuffer("_Vertices", _pipeline.RawLeftEyeVertexBuffer);
        _material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

        // Right eye
        var fRE = math.mul(fF, _pipeline.RightEyeCropMatrix);
        _material.SetMatrix("_XForm", fRE);
        _material.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);
        _material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

        // Debug views
        // Face mesh
        var dF = MathUtil.ScaleOffset(0.5f, math.float2(0.125f, 0));
        _material.SetBuffer("_Vertices", _pipeline.RawFaceVertexBuffer);
        _material.SetPass(1);
        Graphics.DrawMeshNow(_resources.faceLineTemplate, dF);

        // Left eye
        var dLE = MathUtil.ScaleOffset(0.25f, math.float2(0.625f, 0.25f));
        _material.SetMatrix("_XForm", dLE);
        _material.SetBuffer("_Vertices", _pipeline.RawLeftEyeVertexBuffer);
        _material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

        // Right eye
        var dRE = MathUtil.ScaleOffset(0.25f, math.float2(0.625f, 0f));
        _material.SetMatrix("_XForm", dRE);
        _material.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);
        _material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);
    }

    #endregion
}

} // namespace MediaPipe.FaceMesh
