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

        [Space]
        [SerializeField] Vector2Int splitNum;
        [Space]
        [SerializeField] Texture[] splitFaces;
        

        CompositeTexture _composite;

        TextureController _textureController;

        // Start is called before the first frame update
        void Start()
        {
            _composite = new CompositeTexture();

            _textureController = new TextureController();
        }

        private void OnDestroy()
        {
            _faceUVMappedRT.Release();
            _faceSwappedRT.Release();
        }

        // Update is called once per frame
        void Update()
        {
            //mesh情報をアップデート
            _faceMesh.UpdateMesh(_pipeline.RefinedFaceVertexBuffer);//Refinedを使うのでCropMatrix不要
            _faceMeshTransformed.UpdateMesh(_pipeline.RawFaceVertexBuffer);//用意されたmeshに貼り付けるのでrefinedだとだめ

            //RenderTextureにFaceTextureを書き込み
            _faceMeshTransformed.Draw(_pipeline.CroppedFaceTexture);

            //renderTextureと取り込んだテクスチャを合成;
            Graphics.CopyTexture(_faceUVMappedRT, _faceSwappedRT);

            int index = 0;
            
            foreach(Texture splitFace in splitFaces)
            {
                _composite.Composite(_faceSwappedRT, splitFace, splitNum.y,splitNum.x,index);
                index++;
            }

            //合成結果をメッシュ上に描画
            _faceMesh.Draw(_faceSwappedRT);
        }

        //todo Textureをランダムに入れ替える
        public async void SelectTexture()
        {
            //テクスチャ選択をリセット
            for(int i=0; i<splitFaces.Length; i++)
            {
                splitFaces[i] = null;

                ImageData imageData = await _textureController._capturedDataManager.GetRandomData(i);

                splitFaces[i] = imageData.texture;
            }
        }

        public void SaveTexture()
        {
            //System.DateTime UnixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            //  long now = (long)(System.DateTime.Now - UnixEpoch).TotalSeconds;

            //  string filePath = "Assets/" + now + ".png";
            //  Debug.Log(filePath);
            //  TextureController.SaveImage(_faceUVMappedRT, filePath);

            Texture2D[] splitTexture = _textureController.Split(_faceUVMappedRT, splitNum.y, splitNum.x);

            _textureController.SaveImages(splitTexture, "Assets/SplitFaces");
        }

        public void DeleteAllTextures()
        {
            _textureController._capturedDataManager.DeleteAllData();
        }
    }
}