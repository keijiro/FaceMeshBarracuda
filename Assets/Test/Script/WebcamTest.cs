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
    [SerializeField] Shader _cropShader = null;
    [Space]
    [SerializeField] UI.RawImage _mainUI = null;
    [SerializeField] UI.RawImage _cropUI = null;
    [SerializeField] UI.RawImage _previewUI = null;

    #endregion

    #region Private members

    BlazeFace.FaceDetector _detector;
    FaceMesh.MeshBuilder _builder;

    Material _faceMaterial;
    Material _wireMaterial;
    Material _cropMaterial;

    RenderTexture _cropRT;

    Matrix4x4 MakeBlitMatrix(Vector2 offset, float rotation, Vector2 scale)
    {
        return
          Matrix4x4.Translate(offset) *
          Matrix4x4.Scale(new Vector3(scale.x, scale.y, 1)) *
          Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0)) *
          Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotation)) *
          Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0));
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _detector = new BlazeFace.FaceDetector(_blazeFace);
        _builder = new FaceMesh.MeshBuilder(_faceMesh);

        _faceMaterial = new Material(_faceShader);
        _wireMaterial = new Material(_wireShader);
        _cropMaterial = new Material(_cropShader);

        _cropRT = new RenderTexture(192, 192, 0);
    }

    void OnDestroy()
    {
        _detector.Dispose();
        _builder.Dispose();

        Destroy(_faceMaterial);
        Destroy(_wireMaterial);
        Destroy(_cropMaterial);

        Destroy(_cropRT);
    }

    void LateUpdate()
    {
        // Face detection
        _detector.ProcessImage(_webcam.Texture, 0.5f);

        // Use the first detection. Break if no detection.
        var detection = _detector.Detections.FirstOrDefault();
        if (detection.score == 0) return;

        // Face region analysis
        var scale = detection.extent * 1.6f;
        var offset = detection.center - scale * 0.5f;
        var angle = Vector2.Angle(Vector2.up, detection.nose - detection.mouth);
        if (detection.nose.x > detection.mouth.x) angle *= -1;

        // Face region cropping
        _cropMaterial.SetMatrix("_Xform", MakeBlitMatrix(offset, angle, scale));
        Graphics.Blit(_webcam.Texture, _cropRT, _cropMaterial, 0);

        // Face landmark detection
        _builder.ProcessImage(_cropRT);

        // Visualization (face)
        var mf = MakeBlitMatrix(offset - new Vector2(0.75f, 0.5f), angle, scale);
        _faceMaterial.mainTexture = _faceTexture;
        _faceMaterial.SetBuffer("_Vertices", _builder.VertexBuffer);
        Graphics.DrawMesh(_faceTemplate, mf, _faceMaterial, 0);

        // Visualization (wire)
        var mw = MakeBlitMatrix(new Vector2(0.25f, -0.5f), 0, Vector2.one * 0.5f);
        _wireMaterial.SetBuffer("_Vertices", _builder.VertexBuffer);
        Graphics.DrawMesh(_wireTemplate, mw, _wireMaterial, 0);

        // UI update
        _mainUI.texture = _webcam.Texture;
        _cropUI.texture = _cropRT;
        _previewUI.texture = _webcam.Texture;
    }

    #endregion
}

} // namespace MediaPipe
