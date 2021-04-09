using UnityEngine;
using Unity.Barracuda;
using UI = UnityEngine.UI;

namespace FaceMesh {

public sealed class StaticImageTest : MonoBehaviour
{
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Texture2D _image = null;
    [SerializeField] UI.RawImage _uiPreview = null;
    [SerializeField] RectTransform _markerPrefab = null;

    void Start()
    {
        using var builder = new MeshBuilder(_resources);

        builder.ProcessImage(_image);

        var rectSize = ((RectTransform)_uiPreview.transform).rect.size;

        foreach (var v in builder.VertexArray)
        {
            var m = Instantiate(_markerPrefab, _uiPreview.transform);
            ((RectTransform)m.transform).anchoredPosition
              = new Vector2(v.x, v.y) * rectSize;
        }
    }
}

} // namespace FaceMesh
