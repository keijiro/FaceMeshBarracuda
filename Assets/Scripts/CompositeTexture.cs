using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;


public class CompositeTexture :IDisposable
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

    bool _disposed = false;


    public void Dispose()
    {
        // Dispose of unmanaged resources.
        Dispose(true);
        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: Dispose managed resources here.
                MonoBehaviour.Destroy(_material);
                _renderTexture.Release();
                MonoBehaviour.Destroy(_renderTexture);
            }

            // Note disposing has been done.
            _disposed = true;
        }
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
        MonoBehaviour.Destroy(tmpTexture);
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

    //Rectの計算
    public void Composite(RenderTexture targetTexture, Texture overTxtrure,
        Rect rect)
    {
        float _BlendStartU = rect.x / (float)targetTexture.width;
        float _BlendEndU = (rect.x + rect.width) / (float)targetTexture.width;
        float _BlendStartV = rect.y / (float)targetTexture.height;
        float _BlendEndV = (rect.y + rect.height) / (float)targetTexture.height;

        

        Composite(targetTexture, overTxtrure, _BlendStartU, _BlendEndU, _BlendStartV, _BlendEndV);

    }

    //todo blend演出を発火させるメソッドを作成する
    public void StartSwaping()
    {
        //初期値設定

        //透明度
        _material.SetFloat("_Blend", 0);

        //発光
        _material.SetFloat("_Emission", 0f);

        //アニメーションシークエンス設定
        Sequence sequence = DOTween.Sequence();

        sequence.Append(_material.DOFloat(1f, "_Blend", 0.2f));

        sequence.Join(_material.DOFloat(0.75f, "_Emission", 0.25f));

        sequence.Append(_material.DOFloat(0f, "_Emission", 1.0f));

        sequence.Play();
    }

}
