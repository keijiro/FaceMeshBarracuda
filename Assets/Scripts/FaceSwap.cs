using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Threading.Tasks;


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
        [SerializeField] Vector2Int grid;
        //[Space]
        //[SerializeField] List<Texture> splitFaces;
        List<ImageData> _splitFacesData;


        CompositeTexture _composite;

        TextureController _textureController;

        // Start is called before the first frame update
        void Start()
        {
            _composite = new CompositeTexture();

            _textureController = new TextureController();

            _splitFacesData = new();
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
            
            foreach(ImageData splitFaceData in _splitFacesData)
            {
                _composite.Composite(_faceSwappedRT, splitFaceData.texture, splitFaceData.capturedData.rect);
                index++;
            }

            //合成結果をメッシュ上に描画
            _faceMesh.Draw(_faceSwappedRT);
        }


        public async void SelectTexture()//asyncで困ることがあるかも？
        {
            //テクスチャ選択をリセット、メモリ開放
            foreach(ImageData data in _splitFacesData)
            {
                data.Dispose();
            }
            _splitFacesData.Clear();

            //入れ替えが一定の面積を占めるまで、テクスチャを入れ替える
            float swappedSize = 0;
            float textureSize = _faceSwappedRT.width * _faceSwappedRT.height;
            const int block = 8;//面積を調べるときに何ピクセルごとにサンプルするか
            
            while(swappedSize < textureSize * _textureController._texturePercentage)
            {
                ImageData imageData = await _textureController._capturedDataManager.GetRandomData();

                _splitFacesData.Add(imageData);

                //置き換えられていないピクセル数を求める
                swappedSize = textureSize;

                for(int y=0; y<_faceSwappedRT.height; y+=block)
                {
                    for(int x=0; x<_faceSwappedRT.width; x+=block)
                    {
                        Rect identityRect = new Rect(x, y, block,block);

                        bool isSepareted = true;

                        foreach(ImageData data in _splitFacesData)
                        {
                            if (identityRect.Overlaps(data.capturedData.rect))
                            {
                                isSepareted = false;
                                break;
                            }
                        }

                        if (isSepareted)
                        {
                            swappedSize -= (block*block);
                        }
                    }
                }
            }
            Debug.Log("SwapFinish");
        }


        public void SaveTextureGrid()
        {
            ImageData[] imageData = _textureController.Split(_faceUVMappedRT, grid.y, grid.x);

            Debug.Log(imageData.Length);

            _textureController.SaveImages(imageData);

            //メモリ開放
            foreach (ImageData data in imageData)
            {
                data.Dispose();
            }
        }

        public void SaveTextureRandom()
        {
            ImageData[] imageData = _textureController.SplitRandom(_faceUVMappedRT);

            Debug.Log(imageData.Length);

            _textureController.SaveImages(imageData);

            //メモリ開放
            foreach(ImageData data in imageData)
            {
                data.Dispose();
            }
        }




        public void DeleteAllTextures()
        {
            _textureController._capturedDataManager.DeleteAllData();
        }
    }
}