using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class CapturedData
{
    public string _id;
    public int _partId;
}

public class CapturedDataManager 
{
    IEnumerable<CapturedData> _capturedData;

    string _capturedFileDir = Path.Combine(Application.streamingAssetsPath, "Captured");

    public CapturedDataManager()
    {
        _capturedData = new CapturedData[] { };    
    }

    public void SaveData(byte[] data, int partId) {

        //固有のIDを生成してファイルを保存

        string timeStamp = TimeUtil.GetUnixTime(DateTime.Now).ToString();

        string id = timeStamp + "_" + partId;

        string filePath = Path.Combine(_capturedFileDir, id + ".png");

        File.WriteAllBytesAsync(filePath, data);

        //dbに情報を保存
        CapturedData dbData = new CapturedData();

        dbData._id = id;
        dbData._partId = partId;

        _capturedData = _capturedData.Append(dbData);

        //テキストに書き出し
        Debug.Log(_capturedData.ToString());

    }

    public void GetData(string id) { }

    public void GetRandomData(int partId) { }

    //public void GetAllData() { }

    public void GetRawDB() { }

}
