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
}
