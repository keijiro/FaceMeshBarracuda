using UnityEngine;
using Unity.Mathematics;
using UI = UnityEngine.UI;

namespace MediaPipe.FaceMesh
{

    public sealed class MyVisualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamInput _webcam = null;
    [Space]
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Shader _shader = null;
    [Space]
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
        _rightEyeUI.texture = _pipeline.CroppedRightEyeTexture;
    }

    #endregion
}

} // namespace MediaPipe.FaceMesh
