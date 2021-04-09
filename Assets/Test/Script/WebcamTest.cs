using UnityEngine;
using UI = UnityEngine.UI;
using System.Linq;

namespace MediaPipe {

public sealed class WebcamTest : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] BlazeFace.ResourceSet _blazeFace = null;
    [SerializeField] FaceMesh.ResourceSet _faceMesh = null;
    [Space]
    [SerializeField] WebcamInput _webcam = null;
    [SerializeField] Shader _shader = null;
    [SerializeField] Mesh _template = null;
    [Space]
    [SerializeField] UI.RawImage _previewUI = null;

    #endregion

    #region Internal objects

    BlazeFace.FaceDetector _detector;
    FaceMesh.MeshBuilder _builder;
    RenderTexture _cropRT;
    Material _material;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _detector = new BlazeFace.FaceDetector(_blazeFace);
        _builder = new FaceMesh.MeshBuilder(_faceMesh);
        _cropRT = new RenderTexture(192, 192, 0);
        _material = new Material(_shader);
    }

    void OnDestroy()
    {
        _detector.Dispose();
        _builder.Dispose();
        Destroy(_cropRT);
        Destroy(_material);
    }

    void Update()
    {
        _detector.ProcessImage(_webcam.Texture, 0.5f);

        var detection = _detector.Detections.FirstOrDefault();
        if (detection.score == 0) return;

        var cropScale = new Vector2(detection.extent.x,
                                    detection.extent.y) * 1.5f;
        var cropOffset = detection.center - cropScale / 2;

        Graphics.Blit(_webcam.Texture, _cropRT, cropScale, cropOffset);

        _previewUI.texture = _cropRT;

        _builder.ProcessImage(_cropRT);
        _material.SetBuffer("_Vertices", _builder.VertexBuffer);

        Graphics.DrawMesh(_template, transform.localToWorldMatrix, _material, 0);
    }

    #endregion
}

} // namespace MediaPipe
