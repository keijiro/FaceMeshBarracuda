using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UI = UnityEngine.UI;

namespace MediaPipe.FaceMesh
{

    public sealed class MyVisualizer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] MyWebcamInput _webcam = null;
        [Space]
        [SerializeField] ResourceSet _resources = null;
        [Space]
        [SerializeField] UI.RawImage _rightEyeUI = null;
        [SerializeField] UI.RawImage _rightEyeDebug = null;
        [Space]
        [SerializeField] Shader _maskShader = null;
        [SerializeField] Shader _cropShader = null;
        [Space]
        //[SerializeField] DrawLandmarksToMesh _drawLandmarksToMesh = null;
        [SerializeField] EyeContourMesh _eyeContourMesh = null;
        [SerializeField] FaceMesh _faceMesh = null;

        #endregion

        #region Private members

        FacePipeline _pipeline;
        Material _crop, _mask;
        RenderTexture _croppedEyeRT, _maskedEyeRT;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _pipeline = new FacePipeline(_resources);

            //Material初期化
            _mask = new Material(_maskShader);
            _crop = new Material(_cropShader);
            

            //RenderTexture確保
            _croppedEyeRT = new RenderTexture(256, 256, 0);
            _maskedEyeRT = new RenderTexture(1024, 1024, 0);

        }

        void OnDestroy()
        {
            _pipeline.Dispose();
            Destroy(_mask);
            Destroy(_crop);
        }

        void LateUpdate()
        {
            // Processing on the face pipeline
            _pipeline.ProcessImage(_webcam.Texture);

            // UI update
            //_rightEyeUI.texture = _pipeline.CroppedRightEyeTexture;

            //シェーダーに変換行列を適用
            _crop.SetMatrix("_Xform", _pipeline.RightEyeCropMatrix);

            //maskシェーダーに頂点を設定
            _mask.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);

            //webcamTextureを、_cropシェーダーを通して、RenderTextureに描画する
            Graphics.Blit(_webcam.Texture, _croppedEyeRT, _crop);

            //cropされたTextureを、_maskシェーダーを通して、RenderTextureに描画する
            Graphics.Blit(_croppedEyeRT, _maskedEyeRT, _mask);

            //RawImageにRenderTextrueを反映
            _rightEyeUI.texture = _maskedEyeRT;


            //_drawLandmarksToMesh.DrawEye(_pipeline.RawRightEyeVertexBuffer, _pipeline.CroppedRightEyeTexture);

            //_drawLandmarksToMesh.DrawEye(_pipeline.RawLeftEyeVertexBuffer, _pipeline.CroppedLeftEyeTexture);

            //_drawLandmarksToMesh.DrawFace(_pipeline.RawFaceVertexBuffer, _pipeline.CroppedFaceTexture);

            _eyeContourMesh.UpdateMesh(_pipeline.RawRightEyeVertexBuffer);

            //_eyeContourMesh.Draw(_pipeline.CroppedRightEyeTexture);

            _faceMesh.UpdateMesh(_pipeline.RawFaceVertexBuffer);

            _faceMesh.Draw(_pipeline.CroppedFaceTexture);

            #endregion
        }
    }

} // namespace MediaPipe.FaceMesh
