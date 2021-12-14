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
        [Space]
        [SerializeField] UI.RawImage _rightEyeUI = null;
        [SerializeField] UI.RawImage _rightEyeDebug = null;
        [Space]
        [SerializeField] Shader _maskShader = null;
        [SerializeField] Shader _cropShader = null;
        [SerializeField] Shader _shader = null;
        #endregion

        #region Private members

        FacePipeline _pipeline;
        Material _crop, _mask, _material;
        RenderTexture _croppedEyeRT, _maskedEyeRT;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _pipeline = new FacePipeline(_resources);

            //Material初期化
            _mask = new Material(_maskShader);
            _crop = new Material(_cropShader);
            _material = new Material(_shader);

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

            //_mask.SetMatrix("_Xform", _pipeline.RightEyeCropMatrix);

            //maskシェーダーに頂点を設定
            _mask.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);

            //webcamTextureを、_cropシェーダーを通して、RenderTextureに描画する
            Graphics.Blit(_webcam.Texture, _croppedEyeRT, _crop);

            Graphics.Blit(_webcam.Texture, _maskedEyeRT, _mask);

            //RawImageにRenderTextrueを反映
            _rightEyeUI.texture = _croppedEyeRT;

            _rightEyeDebug.texture = _maskedEyeRT;

            // Right eye
           /* var dRE = MathUtil.ScaleOffset(1f, math.float2(0f, 0f));
            _material.SetMatrix("_XForm", dRE);
            _material.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);
            _material.SetPass(3);
            Graphics.Blit(_webcam.Texture, _maskedEyeRT, _material);
           */
        }

        void OnRenderObject()
        {
            // Main view overlay
            var mv = float4x4.Translate(math.float3(-0.875f, -0.5f, 0));
            _material.SetBuffer("_Vertices", _pipeline.RefinedFaceVertexBuffer);
            _material.SetPass(1);
            Graphics.DrawMeshNow(_resources.faceLineTemplate, mv);

            // Face view
            // Face mesh
            var fF = MathUtil.ScaleOffset(0.5f, math.float2(0.125f, -0.5f));
            _material.SetBuffer("_Vertices", _pipeline.RefinedFaceVertexBuffer);
            _material.SetPass(0);
            Graphics.DrawMeshNow(_resources.faceMeshTemplate, fF);

            // Left eye
            var fLE = math.mul(fF, _pipeline.LeftEyeCropMatrix);
            _material.SetMatrix("_XForm", fLE);
            _material.SetBuffer("_Vertices", _pipeline.RawLeftEyeVertexBuffer);
            _material.SetPass(3);
            Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

            // Right eye
            var fRE = math.mul(fF, _pipeline.RightEyeCropMatrix);
            _material.SetMatrix("_XForm", fRE);
            _material.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);
            _material.SetPass(3);
            Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

            // Debug views
            // Face mesh
            var dF = MathUtil.ScaleOffset(0.5f, math.float2(0.125f, 0));
            _material.SetBuffer("_Vertices", _pipeline.RawFaceVertexBuffer);
            _material.SetPass(1);
            Graphics.DrawMeshNow(_resources.faceLineTemplate, dF);

            // Left eye
            var dLE = MathUtil.ScaleOffset(0.25f, math.float2(0.625f, 0.25f));
            _material.SetMatrix("_XForm", dLE);
            _material.SetBuffer("_Vertices", _pipeline.RawLeftEyeVertexBuffer);
            _material.SetPass(3);
            Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);

            // Right eye
            var dRE = MathUtil.ScaleOffset(1f, math.float2(0f, 0f));
            _material.SetMatrix("_XForm", dRE);
            _material.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);
            _material.SetPass(3);
            Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);
        }

        #endregion
    }

} // namespace MediaPipe.FaceMesh
