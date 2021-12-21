using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace MediaPipe.FaceMesh
{
    public class FaceSwap : MonoBehaviour
    {

        [SerializeField] PipeLineManager _pipeline = null;
        [SerializeField] FaceMesh _faceMesh = null;
        [SerializeField] FaceMeshTransformed _faceMeshTransformed = null;
        [SerializeField] RenderTexture _faceUVMappedRT = null;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            _faceMesh.UpdateMesh(_pipeline.RawFaceVertexBuffer,_pipeline.FaceCropMatrix);

            _faceMeshTransformed.UpdateMesh(_pipeline.RawFaceVertexBuffer);

            _faceMeshTransformed.Draw(_pipeline.CroppedFaceTexture);

            _faceMesh.Draw(_faceUVMappedRT);
        }

        public void SaveTexture()
        {
            System.DateTime UnixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            long now = (long)(System.DateTime.Now - UnixEpoch).TotalSeconds;

            string filePath = "Assets/" + now + ".png";
            Debug.Log(filePath);
            TextureController.SaveImage(_faceUVMappedRT, filePath);


        }
    }
}