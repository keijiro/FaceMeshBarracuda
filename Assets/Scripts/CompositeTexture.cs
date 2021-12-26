using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeTexture 
{
    Shader _shader;

    Material _material;

    RenderTexture _renderTexture;

    public CompositeTexture()
    {
        _shader = Shader.Find("Hidden/MediaPipe/FaceMesh/TextureComposite");

        _material = new Material(_shader);

        _renderTexture = new RenderTexture(1024, 1024, 0);
    }

    public void Composite(RenderTexture targetTexture, Texture overTxtrure,
        float _BlendStartU, float _BlendEndU, float _BlendStartV, float _BlendEndV)
    {
        //載せるテクスチャがなければ何もしない
        if (overTxtrure == null)
            return;

        _material.SetTexture("_SubTex", overTxtrure);

        _material.SetFloat("_BlendStartU", _BlendStartU);

        _material.SetFloat("_BlendEndU", _BlendEndU);

        _material.SetFloat("_BlendStartV", _BlendStartV);

        _material.SetFloat("_BlendEndV", _BlendEndV);

        //textureをコピーして、Blitを使ってターゲットに書き込む
        RenderTexture tmpTexture = new RenderTexture(targetTexture.width, targetTexture.height, 0);

        Graphics.CopyTexture(targetTexture, tmpTexture);

        Graphics.Blit(tmpTexture, targetTexture, _material);

        //メモリ確保
        tmpTexture.Release();
    }

    //分割数とその中での番号を引数にする
    public void Composite(RenderTexture targetTexture, Texture overTxtrure,
        int row, int column, int index)
    {
        int x = index % column;
        int y = (int)index / column;

        float _BlendStartU = (float)x / (float)column;
        float _BlendEndU = _BlendStartU + 1.0f/column;
        float _BlendStartV = (float)y / (float)row;
        float _BlendEndV = _BlendStartV + 1.0f/row;

        Composite(targetTexture, overTxtrure, _BlendStartU, _BlendEndU, _BlendStartV, _BlendEndV);
    }


    public void SetBlend(float blend)
    {
        _material.SetFloat("_Blend", blend);
    }

    //todo blend演出を発火させるメソッドを作成する
}
