using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;


//dbに保存するデータ
public class CapturedData
{
    public string id;
    public int partId;
}

//Textureとidを同時に渡す時に使う
public struct ImageData
{
    public CapturedData capturedData { get; set; }
    public Texture texture { get; set; }
}

public class CapturedDataManager 
{
    string _folderName;

    List<CapturedData> _capturedData;

    string _capturedFileDir;

    public CapturedDataManager(string folderName)
    {
        //初期化
        _folderName = folderName;

        _capturedData = new();

        _capturedFileDir = Path.Combine(Application.streamingAssetsPath, _folderName);

        LoadFromJSON();
    }

    public void SaveData(byte[] bytes, int partId) {

        //固有のIDを生成してファイルを保存

        string timeStamp = TimeUtil.GetUnixTime(DateTime.Now).ToString();

        string id = timeStamp + "_" + partId;

        string filePath = Path.Combine(_capturedFileDir, id + ".png");

        File.WriteAllBytesAsync(filePath, bytes);

        //dbに情報を保存
        CapturedData dbData = new CapturedData();

        dbData.id = id;
        dbData.partId = partId;

        _capturedData.Add(dbData);

    }
    public async void UpdateJSON()
    {
        //Jsonに書き出し
        string json = JsonConvert.SerializeObject(_capturedData);

        //jsonFilieを更新
        StreamWriter writer = new StreamWriter(Path.Combine(_capturedFileDir, "CapturedData.json"), false);
        await writer.WriteAsync(json);
        writer.Close();
    }

    public async void LoadFromJSON()
    {
        //jsonを読み込んでListを更新
        try
        {
            StreamReader reader = new StreamReader(Path.Combine(_capturedFileDir, "CapturedData.json"));
            string json = await reader.ReadToEndAsync();

            _capturedData = JsonConvert.DeserializeObject<List<CapturedData>>(json);
        }
        catch
        {
            Debug.Log("No JSON file.");
        }
    }

    public async Task<ImageData> GetData(string inputId)
    {
        //idが一致するデータを探す
        IEnumerable<CapturedData> query = _capturedData.Where(data => data.id == inputId);

        ImageData imageData = new();

        //一致した中の先頭を返す
        foreach (CapturedData data in query)
        {
            imageData.capturedData = data;

            //ファイル読み込み
            string path = Path.Combine(_capturedFileDir, data.id + ".png");

            byte[] bytes = await File.ReadAllBytesAsync(path);

            Texture2D texture = new Texture2D(1, 1);

            texture.LoadImage(bytes);

            imageData.texture = texture;

            return imageData;
        }

        //一致しなければ空を返す
        return imageData;

    }

    public async Task<ImageData> GetRandomData(int partId)
    {
        //partIdが一致したデータの中からランダムに1つを返す
        IEnumerable<CapturedData> query = _capturedData.Where(data => data.partId == partId);

        int count = query.Count();

        //Debug.Log(count);

        int index = UnityEngine.Random.Range(0, count);

        List<CapturedData> captureds = query.ToList();

        CapturedData captured = captureds[index];

        ImageData imageData = await GetData(captured.id);

        return imageData;

    }

    //public void GetAllData() { }

    public void GetRawJson() { }

    //public ImageData DeleteData(string id)
   // {

   // }

    public void DeleteAllData() { }



}
