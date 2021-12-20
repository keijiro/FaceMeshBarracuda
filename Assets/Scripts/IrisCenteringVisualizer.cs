using UnityEngine;
using Unity.Mathematics;
using UI = UnityEngine.UI;

namespace MediaPipe.FaceMesh
{

    public sealed class IrisCenteringVisualizer : MonoBehaviour
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
        RenderTexture _rightEyeRT;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _pipeline = new FacePipeline(_resources);
            _material = new Material(_shader);
            _rightEyeRT = new RenderTexture(1024, 1024, 0);
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

            //クロップ変換行列をshaderに設定
            _material.SetMatrix("_CropMatrix", _pipeline.RightEyeCropMatrix);

            //landmark情報をshaderに設定
            _material.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);

            //webcam画像をshaderを通してrenderTextureに書き込み
            Graphics.Blit(_webcam.Texture, _rightEyeRT, _material);

            // UI update
            _rightEyeUI.texture = _rightEyeRT;

        }


        #endregion
    }

} // namespace MediaPipe.FaceMesh
