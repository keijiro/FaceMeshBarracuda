using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UI = UnityEngine.UI;

namespace MediaPipe.FaceMesh
{

    public sealed class FaceTextureGenerator : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] MyWebcamInput _webcam = null;
        [Space]
        [SerializeField] ResourceSet _resources = null;
        [Space]
        [SerializeField] FaceMeshTransformed _faceMeshTransformed = null;
        [Space]
        [SerializeField] RenderTexture _faceTextrueUVMapped = null;

        #endregion

        #region Private members

        FacePipeline _pipeline;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _pipeline = new FacePipeline(_resources);

        }

        void OnDestroy()
        {
            _pipeline.Dispose();
        }

        void LateUpdate()
        {
            // Processing on the face pipeline
            _pipeline.ProcessImage(_webcam.Texture);

            try
            {
                _faceMeshTransformed.UpdateMesh(_pipeline.RawFaceVertexBuffer);

                _faceMeshTransformed.Draw(_pipeline.CroppedFaceTexture);
            }

            catch { }

            #endregion
        }

        public void SaveImage()
        {
         //todo saveImage   
        }
    }

} // namespace MediaPipe.FaceMesh
