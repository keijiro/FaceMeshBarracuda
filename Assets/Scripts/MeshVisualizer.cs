using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UI = UnityEngine.UI;

namespace MediaPipe.FaceMesh
{

    public sealed class MeshVisualizer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] PipeLineManager _pipeline = null;
        [SerializeField] FaceMesh _faceMesh = null;
        [SerializeField] FaceMeshTransformed _faceMeshTransformed = null;
        [SerializeField] EyeContourMesh _rightEyeContourMesh = null;
        [SerializeField] EyeContourMesh _leftEyeContourMesh = null;

        #endregion

        #region Private members


        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
           
        }

        void OnDestroy()
        {
           
        }

        void LateUpdate()
        {
            //Update and draw mesh
            try
            {
                _rightEyeContourMesh.UpdateMesh(_pipeline.RawRightEyeVertexBuffer, _pipeline.RightEyeCropMatrix);

                _rightEyeContourMesh.Draw(_pipeline.CroppedRightEyeTexture);
            }

            catch { }

            try
            {
                _leftEyeContourMesh.UpdateMesh(_pipeline.RawLeftEyeVertexBuffer, _pipeline.LeftEyeCropMatrix);

                _leftEyeContourMesh.Draw(_pipeline.CroppedLeftEyeTexture);
            }

            catch { }

            try
            {
                _faceMesh.UpdateMesh(_pipeline.RawFaceVertexBuffer, _pipeline.FaceCropMatrix);

                _faceMesh.Draw(_pipeline.CroppedFaceTexture);
            }
            catch { }

            try
            {
                _faceMeshTransformed.UpdateMesh(_pipeline.RawFaceVertexBuffer);

                _faceMeshTransformed.Draw(_pipeline.CroppedFaceTexture);
            }

            catch { }

            #endregion
        }
    }

} // namespace MediaPipe.FaceMesh
