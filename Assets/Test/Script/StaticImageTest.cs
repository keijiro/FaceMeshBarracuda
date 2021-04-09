using UnityEngine;
using Unity.Barracuda;
using UI = UnityEngine.UI;

namespace MediaPipe {

public sealed class StaticImageTest : MonoBehaviour
{
    [SerializeField] FaceMesh.ResourceSet _resources = null;
    [SerializeField] Texture2D _image = null;
    [SerializeField] UI.RawImage _uiPreview = null;
    [SerializeField] Shader _shader = null;
    [SerializeField] Mesh _template = null;

    FaceMesh.MeshBuilder _builder;
    Material _material;

    void Start()
    {
        _uiPreview.texture = _image;

        _builder = new FaceMesh.MeshBuilder(_resources);
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
