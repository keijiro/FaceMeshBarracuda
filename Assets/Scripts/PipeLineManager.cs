using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UI = UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections;


namespace MediaPipe.FaceMesh
{

    public sealed class PipeLineManager : MonoBehaviour
    {
        #region Editable attributes

        [Space]
        [SerializeField] ResourceSet _resources = null;
        [Space]
        [SerializeField] RenderTexture _inputTexture = null;

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


        public void ProcessImage()
            => _pipeline.ProcessImage(_inputTexture);

        public void SetInputTextre(RenderTexture texture)
            => _inputTexture = texture;

        #endregion


        #region Private members

        FacePipeline _pipeline;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _pipeline = new FacePipeline(_resources);

            StartCoroutine(ProcessImageCoroutine());
        }
 

        void OnDestroy()
        {
            _pipeline.Dispose();
        }

        void Update()
        {
            // Processing on the face pipeline
            //_pipeline.ProcessImage(_inputTexture);

        }


        IEnumerator ProcessImageCoroutine()
        {
            while (true)
            {
                ProcessImage();

                yield return null;
            }

        }

        #endregion
    }

} // namespace MediaPipe.FaceMesh
