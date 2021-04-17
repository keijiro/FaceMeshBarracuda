using UnityEngine;
using UI = UnityEngine.UI;
using System.Linq;

namespace MediaPipe {

public sealed class Visualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamInput _webcam = null;
    [SerializeField] BlazeFace.ResourceSet _blazeFace = null;
    [SerializeField] FaceMesh.ResourceSet _faceMesh = null;
    [Space]
    [SerializeField] Mesh _faceTemplate = null;
    [SerializeField] Mesh _wireTemplate = null;
    [SerializeField] Texture _faceTexture = null;
    [Space]
    [SerializeField] Shader _faceShader = null;
    [SerializeField] Shader _cropShader = null;
    [Space]
    [SerializeField] UI.RawImage _mainUI = null;
    [SerializeField] UI.RawImage _cropUI = null;
    [SerializeField] UI.RawImage _previewUI = null;

    #endregion

    #region Private members

    BlazeFace.FaceDetector _boxDetector;
    FaceMesh.FaceLandmarkDetector _landmarkDetector;

    Material _faceMaterial;
    Material _cropMaterial;

    Matrix4x4 _cropMatrix;
    RenderTexture _cropRT;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _boxDetector = new BlazeFace.FaceDetector(_blazeFace);
        _landmarkDetector = new FaceMesh.FaceLandmarkDetector(_faceMesh);

        _faceMaterial = new Material(_faceShader);
        _cropMaterial = new Material(_cropShader);

        _cropRT = new RenderTexture(192, 192, 0);
    }

    void OnDestroy()
    {
        _boxDetector.Dispose();
        _landmarkDetector.Dispose();

        Destroy(_faceMaterial);
        Destroy(_cropMaterial);

        Destroy(_cropRT);
    }

    void LateUpdate()
    {
        // Face detection
        _boxDetector.ProcessImage(_webcam.Texture, 0.5f);

        // Use the first detection. Break if no detection.
        var detection = _boxDetector.Detections.FirstOrDefault();
        if (detection.score == 0) return;

        // Face region analysis
        var scale = detection.extent * 1.6f;
        var offset = detection.center - scale * 0.5f;
        var angle = Vector2.Angle(Vector2.up, detection.nose - detection.mouth);
        if (detection.nose.x > detection.mouth.x) angle *= -1;

        // Crop matrix calculation
        _cropMatrix = Matrix4x4.Translate(offset) *
                      Matrix4x4.Scale(new Vector3(scale.x, scale.y, 1)) *
                      Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0)) *
                      Matrix4x4.Rotate(Quaternion.Euler(0, 0, angle)) *
                      Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0));

        // Face region cropping
        _cropMaterial.SetMatrix("_Xform", _cropMatrix);
        Graphics.Blit(_webcam.Texture, _cropRT, _cropMaterial, 0);

        // Face landmark detection
        _landmarkDetector.ProcessImage(_cropRT);

        // UI update
        _mainUI.texture = _webcam.Texture;
        _cropUI.texture = _cropRT;
        _previewUI.texture = _webcam.Texture;
    }

    void OnRenderObject()
    {
        // Textured surface rendering
        var mf = Matrix4x4.Translate(new Vector3(-0.75f, -0.5f, 0)) *
                 _cropMatrix;

        _faceMaterial.mainTexture = _faceTexture;
        _faceMaterial.SetBuffer("_Vertices", _landmarkDetector.VertexBuffer);
        _faceMaterial.SetPass(0);

        Graphics.DrawMeshNow(_faceTemplate, mf);

        // Wireframe mesh rendering
        var mw = Matrix4x4.Translate(new Vector2(0.25f, -0.5f)) *
                 Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 1));

        _faceMaterial.SetBuffer("_Vertices", _landmarkDetector.VertexBuffer);
        _faceMaterial.SetPass(1);

        Graphics.DrawMeshNow(_wireTemplate, mw);

        // Keypoint marking
        _faceMaterial.SetBuffer("_Vertices", _landmarkDetector.VertexBuffer);
        _faceMaterial.SetMatrix("_XForm", mw);
        _faceMaterial.SetPass(2);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 400, 1);
    }

    #endregion
}

} // namespace MediaPipe
