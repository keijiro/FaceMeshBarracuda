using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering;

public class TextureController
{
    public CapturedDataManager _capturedDataManager { get; private set; } 

    public  TextureController()
    {
        _capturedDataManager = new CapturedDataManager("Captured");
    }
    /*public void SaveImage(RenderTexture renderTexture, string filePath)
    {
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        RenderTexture.active = renderTexture;

        tex.ReadPixels(new Rect (0, 0, renderTexture.width, renderTexture.height),0,0);

        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();


        //System.IO.File.WriteAllBytesAsync(filePath, bytes);

    }*/

    public float _texturePercentage = 2f / 3f;

    //texture2Dをまとめて保存
    public void SaveImages(ImageData[] inputs)
    {
        
        string timeStamp = TimeUtil.GetUnixTime(System.DateTime.Now).ToString();

        List<byte[]> bytesList = new List<byte[]>();

        //バイトデータ読み込み
        foreach (ImageData imageData in inputs)
        {
            bytesList.Add(imageData.texture.GetRawTextureData());
        }

        int index = 0;

        foreach (byte[] rawData in bytesList)
        {
            //テクスチャのバイト化と保存

            byte[] bytes = ImageConversion.EncodeArrayToPNG
            (rawData,
            UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB,
            (uint)inputs[index].texture.width,
            (uint)inputs[index].texture.height);
            _capturedDataManager.SaveData(bytes, inputs[index].capturedData.rect);

            index++;
        }

        //Jsonに保存
        _capturedDataManager.UpdateJSON();
    }

    //todo 戻り値をImageData[]にする
    //テクスチャを分割する
    public ImageData[] Split(Texture input, int row, int column　)
    {
        //分割後のテクスチャのサイズを計算
        int width = (int)input.width / column;

        int height = (int)input.height / row;

        //変換用のRenderTextureにコピー
        RenderTexture renderTexture = new RenderTexture(input.width,input.height, 32);

        Graphics.Blit(input, renderTexture);

        RenderTexture.active = renderTexture;

        //配列作成して入れていく
        List<ImageData> resultData = new();

        //ここが重い
        for(int y = 0; y<row; y++)
        {
            for (int x = 0; x < column; x++)
            {
                //texture読み込み
                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                Rect rect = new Rect(width * x, height * y, width, height);
                texture.ReadPixels(rect, 0, 0);
                texture.Apply();

                ImageData data = new ImageData();
                data.capturedData = new CapturedData();
                data.capturedData.id = "";
                data.capturedData.rect = rect;
                data.texture = texture;

                resultData.Add(data);
                
                //読み込み失敗した
               /* AsyncGPUReadback.Request(renderTexture, 0, request =>
                {
                    var data = request.GetData<Color32>();
                    Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    texture.LoadRawTextureData(data);
                    texture.Apply();
                });
               */
            }
        }
        RenderTexture.active = null;

        renderTexture.Release();

        return resultData.ToArray();

    }

    public ImageData[] SplitRandom(Texture input)
    {
        //変換用のRenderTextureにコピー
        RenderTexture renderTexture = new RenderTexture(input.width, input.height, 32);
        Graphics.Blit(input, renderTexture);
        RenderTexture.active = renderTexture;

        //テクスチャ面積
        float rectSize = input.width * input.height;

        //今までの合計面積
        float resultsSize = 0;

        //配列作成して入れていく
        List<ImageData> resultData = new();

        //生成したテクスチャが全体の一定割合以上になるまで繰り返す
        while(resultsSize < rectSize　* _texturePercentage)
        {
            //ランダムな短形を生成
            int width = Random.Range(input.width / 16, input.width / 4);
            int height = Random.Range(input.height / 16, input.height / 4);
            int x = Random.Range(0, input.width);
            int y = Random.Range(0, input.height);
            //テクスチャ範囲をはみ出ないようにする
            if (x + width > input.width)
                width = input.width - x;
            if (y + height > input.height)
                height = input.height - y;

            Rect rect = new Rect(x, y, width, height);

            //短形がこれまでの短形と被っていないか判定
            bool isSepareted = true;

            foreach(ImageData result in resultData)
            {
                if (rect.Overlaps(result.capturedData.rect))
                {
                    isSepareted = false;
                    break;
                }
            }

            //被っていない短形なら処理を進める
            if (isSepareted)
            {
                //テクスチャに短形範囲をコピー
                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                //Graphics.CopyTexture(input, 0, 0, x, y, width, height, texture, 0, 0, 0, 0);
                texture.ReadPixels(rect, 0, 0);

                ImageData imageData = new();
                imageData.capturedData = new();
                imageData.texture = texture;
                imageData.capturedData.id = "";
                imageData.capturedData.rect = rect;

                resultData.Add(imageData);

                //面積の総和を更新
                resultsSize += rect.size.x * rect.size.y;
            }
        }
        RenderTexture.active = null;

        renderTexture.Release();

        return resultData.ToArray();

    }
}
