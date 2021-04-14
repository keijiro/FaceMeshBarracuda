using UnityEngine;
using UI = UnityEngine.UI;

namespace MediaPipe {

public sealed class StaticImageTest : MonoBehaviour
{
    [SerializeField] FaceMesh.ResourceSet _faceMesh = null;
    [SerializeField] Texture2D _image = null;
    [SerializeField] Shader _shader = null;
    [SerializeField] Mesh _template = null;
    [SerializeField] UI.RawImage _uiPreview = null;

    FaceMesh.MeshBuilder _builder;
    Material _material;

    void Start()
    {
        _uiPreview.texture = _image;

        _builder = new FaceMesh.MeshBuilder(_faceMesh);
        _builder.ProcessImage(_image);

        _material = new Material(_shader);
        _material.SetBuffer("_Vertices", _builder.VertexBuffer);
    }

    void OnDestroy()
    {
        _builder.Dispose();
        Destroy(_material);
    }

    void Update()
      => Graphics.DrawMesh(_template, transform.localToWorldMatrix, _material, 0);
}

} // namespace MediaPipe
