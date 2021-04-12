using UnityEngine;
using UI = UnityEngine.UI;
using System.Linq;

namespace MediaPipe {

public sealed class WebcamTest : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamInput _webcam = null;
    [SerializeField] BlazeFace.ResourceSet _blazeFace = null;
    [SerializeField] FaceMesh.ResourceSet _faceMesh = null;
    [Space]
    [SerializeField] Mesh _faceTemplate = null;
    [SerializeField] Texture _faceTexture = null;
    [SerializeField] Shader _faceShader = null;
    [Space]
    [SerializeField] Mesh _wireTemplate = null;
    [SerializeField] Shader _wireShader = null;
    [Space]
    [SerializeField] UI.RawImage _mainUI = null;
    [SerializeField] UI.RawImage _cropUI = null;
    [SerializeField] UI.RawImage _previewUI = null;

    #endregion

    #region Internal objects

    BlazeFace.FaceDetector _detector;
    FaceMesh.MeshBuilder _builder;

    Material _faceMaterial;
    Material _wireMaterial;

    RenderTexture _cropRT;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _detector = new BlazeFace.FaceDetector(_blazeFace);
        _builder = new FaceMesh.MeshBuilder(_faceMesh);

        _faceMaterial = new Material(_faceShader);
        _wireMaterial = new Material(_wireShader);

        _cropRT = new RenderTexture(192, 192, 0);
    }

    void OnDestroy()
    {
        _detector.Dispose();
        _builder.Dispose();

        Destroy(_faceMaterial);
        Destroy(_wireMaterial);

        Destroy(_cropRT);
    }

    void LateUpdate()
    {
        // Face detection
        _detector.ProcessImage(_webcam.Texture, 0.5f);

        // Use the first detection. Break if no detection.
        var detection = _detector.Detections.FirstOrDefault();
        if (detection.score == 0) return;

        // Face region cropping
        var scale = detection.extent * 1.5f;
        var offset = detection.center - scale * 0.5f;
        Graphics.Blit(_webcam.Texture, _cropRT, scale, offset);

        // Face landmark detection
        _builder.ProcessImage(_cropRT);

        // Visualization (face)
        _faceMaterial.mainTexture = _faceTexture;
        _faceMaterial.SetVector("_Scale", scale);
        _faceMaterial.SetVector("_Offset", offset - new Vector2(0.25f, 0));
        _faceMaterial.SetBuffer("_Vertices", _builder.VertexBuffer);
        Graphics.DrawMesh(_faceTemplate, Matrix4x4.identity, _faceMaterial, 0);

        // Visualization (wire)
        _wireMaterial.SetVector("_Scale", Vector2.one * 0.5f);
        _wireMaterial.SetVector("_Offset", new Vector2(0.75f, 0));
        _wireMaterial.SetBuffer("_Vertices", _builder.VertexBuffer);
        Graphics.DrawMesh(_wireTemplate, Matrix4x4.identity, _wireMaterial, 0);

        // UI update
        _mainUI.texture = _webcam.Texture;
        _cropUI.texture = _cropRT;
        _previewUI.texture = _webcam.Texture;
    }

    #endregion
}

} // namespace MediaPipe
