using UnityEngine;
using Unity.Barracuda;
using UI = UnityEngine.UI;

namespace FaceMesh {

public sealed class StaticImageTest : MonoBehaviour
{
    [SerializeField] WorkerFactory.Type _workerType;
    [SerializeField] NNModel _model;
    [SerializeField] Texture2D _image;
    [SerializeField] UI.RawImage _uiPreview;
    [SerializeField] RectTransform _markerPrefab;

    void Start()
    {
        // Input image -> Tensor (1, 192, 192, 3)
        var source = new float[192 * 192 * 3];

        for (var y = 0; y < 192; y++)
        {
            for (var x = 0; x < 192; x++)
            {
                var i = (y * 192 + x) * 3;
                var p = _image.GetPixel(x, 191 - y);
                source[i + 0] = p.r * 2 - 1;
                source[i + 1] = p.g * 2 - 1;
                source[i + 2] = p.b * 2 - 1;
            }
        }

        // Inference
        var model = ModelLoader.Load(_model);
        using var worker = WorkerFactory.CreateWorker(_workerType, model);

        using (var tensor = new Tensor(1, 192, 192, 3, source))
            worker.Execute(tensor);

        // Visualization
        var output = worker.PeekOutput("conv2d_20");
        var size = ((RectTransform)_uiPreview.transform).rect.size;

        for (var i = 0; i < 468; i++)
        {
            var x = (      output[0, 0, 0, i * 3 + 0]) / 192 * size.x;
            var y = (191 - output[0, 0, 0, i * 3 + 1]) / 192 * size.y;
            var m = Instantiate(_markerPrefab, _uiPreview.transform);
            ((RectTransform)m.transform).anchoredPosition = new Vector2(x, y);
        }
    }
}

} // namespace FaceMesh
