using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UI = UnityEngine.UI;

namespace MediaPipe.FaceMesh
{

    public sealed class TapiocaVisualizer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] PipeLineManager _pipeline = null;
        [SerializeField] IrisMesh _rightIrisMesh = null;
        [SerializeField] UI.RawImage rawImage = null;
        [SerializeField] Shader _shader = null;

        #endregion

        #region Private members

        Material _material;
        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _material = new Material(_shader);
        }

        void OnDestroy()
        {
           
        }

        void LateUpdate()
        {
            try
            {
                _rightIrisMesh.UpdateMesh(_pipeline.RawRightEyeVertexBuffer);

                _rightIrisMesh.Draw(_pipeline.CroppedRightEyeTexture);
            }

            catch { }

            rawImage.texture = _pipeline.CroppedRightEyeTexture;
            #endregion
        }

        private void OnRenderObject()
        {
            // Right eye
            var dRE = MathUtil.ScaleOffset(0.25f, math.float2(0.25f, -0.5f));
            _material.SetMatrix("_XForm", dRE);
            _material.SetBuffer("_Vertices", _pipeline.RawRightEyeVertexBuffer);
            _material.SetPass(3);
            Graphics.DrawProceduralNow(MeshTopology.Lines, 64, 1);
        }
    }

} // namespace MediaPipe.FaceMesh
