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

    //texture2Dをまとめて保存
    public void SaveImages(Texture2D[] inputs, string dirPath)
    {
        
        string timeStamp = TimeUtil.GetUnixTime(System.DateTime.Now).ToString();

        List<byte[]> bytesList = new List<byte[]>();

        //バイトデータ読み込み
        foreach (Texture2D texture in inputs)
        {
            bytesList.Add(texture.GetRawTextureData());
        }

        int index = 0;

        foreach (byte[] rawData in bytesList)
        {
            //テクスチャのバイト化と保存

            byte[] bytes = ImageConversion.EncodeArrayToPNG
            (rawData,
            UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB,
            (uint)inputs[index].width,
            (uint)inputs[index].height);
            _capturedDataManager.SaveData(bytes, index);

            index++;
        }

        //Jsonに保存
        _capturedDataManager.UpdateJSON();
    }

    //テクスチャを分割する
    public Texture2D[] Split(Texture input, int row, int column　)
    {
        //分割後のテクスチャのサイズを計算
        int width = (int)input.width / column;

        int height = (int)input.height / row;

        //変換用のRenderTextureにコピー
        RenderTexture renderTexture = new RenderTexture(input.width,input.height, 32);

        Graphics.Blit(input, renderTexture);

        RenderTexture.active = renderTexture;

        //配列作成して入れていく
        List<Texture2D> textures = new();

        //ここが重い
        for(int y = 0; y<row; y++)
        {
            for (int x = 0; x < column; x++)
            {
                //texture読み込み
                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(width * x, height * y, width, height), 0, 0);
                texture.Apply();

                textures.Add(texture);
                
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

        return textures.ToArray();

    }
}
