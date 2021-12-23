using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class TextureController
{

    static public void SaveImage(RenderTexture renderTexture, string filePath)
    {
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        RenderTexture.active = renderTexture;

        tex.ReadPixels(new Rect (0, 0, renderTexture.width, renderTexture.height),0,0);

        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();

        //Destroy(tex);

        System.IO.File.WriteAllBytesAsync(filePath, bytes);

    }

    //テクスチャを分割する
    static public Texture2D[] Split(Texture input, int row, int column　)
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

        for(int y = 0; y<row; y++)
        {
            for(int x = 0; x<column; x++)
            {
                //texture読み込み
                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32,false);
                texture.ReadPixels(new Rect(width * x, height * y, width, height), 0, 0);

                textures.Add(texture);
            }
        }

        return textures.ToArray();

    }
}
