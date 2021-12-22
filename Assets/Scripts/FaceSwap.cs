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
        [SerializeField] RenderTexture _faceSwappedRT = null;
        [SerializeField] Texture _swapFaceTexture = null;
        [SerializeField] Material _material = null;
        [SerializeField] Material _material2 = null;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //mesh情報をアップデート
            _faceMesh.UpdateMesh(_pipeline.RawFaceVertexBuffer,_pipeline.FaceCropMatrix);

            _faceMeshTransformed.UpdateMesh(_pipeline.RawFaceVertexBuffer);

            //RenderTextureにFaceTextureを書き込み
            _faceMeshTransformed.Draw(_pipeline.CroppedFaceTexture);

            //renderTextureと取り込んだテクスチャを合成
            _material.SetTexture("_MainTex", _faceUVMappedRT);
            Graphics.Blit(_faceUVMappedRT,_faceSwappedRT, _material);
            Graphics.Blit(_faceSwappedRT, _faceSwappedRT, _material2);

            _faceMesh.Draw(_faceSwappedRT);
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