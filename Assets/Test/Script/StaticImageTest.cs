using UnityEngine;
using Unity.Barracuda;
using UI = UnityEngine.UI;
using System.Linq;

namespace MediaPipe {

public sealed class StaticImageTest : MonoBehaviour
{
    [SerializeField] BlazeFace.ResourceSet _blazeFace = null;
    [SerializeField] FaceMesh.ResourceSet _faceMesh = null;
    [Space]
    [SerializeField] Texture2D _image = null;
    [SerializeField] Shader _shader = null;
    [SerializeField] Mesh _template = null;
    [Space]
    [SerializeField] UI.RawImage _uiPreview = null;

    FaceMesh.MeshBuilder _builder;
    RenderTexture _cropRT;
    Material _material;

    void Start()
    {
        _uiPreview.texture = _image;

        using var detector = new BlazeFace.FaceDetector(_blazeFace);
        detector.ProcessImage(_image, 0.5f);

        var detection = detector.Detections.First();
        var cropScale = new Vector2(detection.extent.x,
                                    detection.extent.y) * 1.5f;
        var cropOffset = detection.center - cropScale / 2;

        _cropRT = new RenderTexture(192, 192, 0);

        Graphics.Blit(_image, _cropRT, cropScale, cropOffset);


        _builder = new FaceMesh.MeshBuilder(_faceMesh);
        _builder.ProcessImage(_cropRT);

        _material = new Material(_shader);
        _material.SetVector("_Scale", cropScale);
        _material.SetVector("_Offset", cropOffset);
        _material.SetBuffer("_Vertices", _builder.VertexBuffer);
    }

    void OnDestroy()
    {
        _builder.Dispose();
        Destroy(_cropRT);
        Destroy(_material);
    }

    void Update()
      => Graphics.DrawMesh(_template, transform.localToWorldMatrix, _material, 0);
}

} // namespace MediaPipe
