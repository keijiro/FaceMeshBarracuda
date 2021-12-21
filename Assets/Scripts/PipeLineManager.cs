using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UI = UnityEngine.UI;

namespace MediaPipe.FaceMesh
{

    public sealed class PipeLineManager : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] MyWebcamInput _webcam = null;
        [Space]
        [SerializeField] ResourceSet _resources = null;
        [Space]
        [SerializeField] UI.RawImage _webCamUI = null;

        #endregion

        #region Public members

        //Accessors for vertex buffers

        public ComputeBuffer RawFaceVertexBuffer
            => _pipeline.RawFaceVertexBuffer;

        public ComputeBuffer RawRightEyeVertexBuffer
            => _pipeline.RawRightEyeVertexBuffer;

        public ComputeBuffer RawLeftEyeVertexBuffer
            => _pipeline.RawLeftEyeVertexBuffer;

        public ComputeBuffer RefinedFaceVertexBuffer
          => _pipeline.RefinedFaceVertexBuffer;

        //Accessors for cropped textures

        public Texture CroppedFaceTexture
          => _pipeline.CroppedFaceTexture;

        public Texture CroppedLeftEyeTexture
          => _pipeline.CroppedLeftEyeTexture;

        public Texture CroppedRightEyeTexture
          => _pipeline.CroppedRightEyeTexture;

        //Accessors for crop region matrices

        public float4x4 FaceCropMatrix
          => _pipeline.FaceCropMatrix;

        public float4x4 LeftEyeCropMatrix
          => _pipeline.LeftEyeCropMatrix;

        public float4x4 RightEyeCropMatrix
          => _pipeline.RightEyeCropMatrix;

        //Access for status
        public bool IsFaceTracking
            => _pipeline.IsFaceTracking;

        #endregion


        #region Private members

        FacePipeline _pipeline;

        RenderTexture _webCamViewRT;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _pipeline = new FacePipeline(_resources);

            _webCamViewRT = new RenderTexture(_webcam.Texture.width, _webcam.Texture.height, 0);
        }

        void OnDestroy()
        {
            _pipeline.Dispose();
        }

        void LateUpdate()
        {
            // Processing on the face pipeline
            _pipeline.ProcessImage(_webcam.Texture);

            _webCamUI.texture = _webcam.Texture;

        }

        #endregion
    }

} // namespace MediaPipe.FaceMesh
