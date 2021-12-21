using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UI = UnityEngine.UI;

namespace MediaPipe.FaceMesh
{

    public sealed class FaceSwap : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] MyWebcamInput _webcam = null;
        [Space]
        [SerializeField] ResourceSet _resources = null;
        [Space]
        [SerializeField] UI.RawImage _webCamUI = null;
        [Space]
        [SerializeField] FaceMesh _faceMesh = null;
        [SerializeField] FaceMeshTransformed _faceMeshTransformed = null;
        [Space]
        [SerializeField] RenderTexture _faceTextureUVMapped = null;

        #endregion

        #region Private members

        FacePipeline _pipeline;

        RenderTexture _webCamViewRT;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _pipeline = new FacePipeline(_resources);

            _webCamViewRT = new RenderTexture(1024, 1024, 0);
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

            //Update and draw mesh

            //UVマッピングした顔テクスチャを更新
            try
            {
                _faceMeshTransformed.UpdateMesh(_pipeline.RawFaceVertexBuffer);

                _faceMeshTransformed.Draw(_pipeline.CroppedFaceTexture);
            }

            catch { }



            try
            {
                _faceMesh.UpdateMesh(_pipeline.RawFaceVertexBuffer, _pipeline.FaceCropMatrix);

                _faceMesh.Draw(_faceTextureUVMapped);
            }
            catch { }

         

            #endregion
        }
    }

} // namespace MediaPipe.FaceMesh
