using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;


//dbに保存するデータ
public class CapturedData
{
    public string id;
    public Rect rect;
    //public int partId;
}

//Textureとidを同時に渡す時に使う
public class ImageData : IDisposable
{
    public CapturedData capturedData { get; set; }
    public Texture2D texture { get; set; }

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
                MonoBehaviour.Destroy(texture);
                capturedData = null;
            }

            // Note disposing has been done.
            _disposed = true;
        }
    }
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

        _capturedFileDir = Path.Combine(Application.persistentDataPath, _folderName);

        if(!Directory.Exists(_capturedFileDir))
            Directory.CreateDirectory(_capturedFileDir);

        LoadFromJSON();
    }

    public void SaveData(byte[] bytes, Rect rect) {

        //固有のIDを生成してファイルを保存

        string timeStamp = TimeUtil.GetUnixTime(DateTime.Now).ToString();

        //ハッシュ化
        HMACMD5 csp = new();
        byte[] targetBytes = System.Text.Encoding.UTF8.GetBytes(timeStamp + rect.x + rect.y + rect.width + rect.height);

        byte[] hash =  csp.ComputeHash(targetBytes);

        System.Text.StringBuilder hashStr = new();

        foreach(byte hashByte in hash)
        {
            hashStr.Append(hashByte.ToString("x2"));
        }

        string id = hashStr.ToString();

        //ファイル保存
        string filePath = Path.Combine(_capturedFileDir, id + ".png");

        File.WriteAllBytesAsync(filePath, bytes);

        //dbに情報を保存
        CapturedData dbData = new CapturedData();

        dbData.id = id;
        dbData.rect = rect;

        _capturedData.Add(dbData);
    }

    public async void UpdateJSON()
    {
        //Jsonに書き出し
        string json = JsonConvert.SerializeObject(_capturedData);

        //jsonFilieを更新
        string path = Path.Combine(_capturedFileDir, "CapturedData.json");

        await File.WriteAllTextAsync(path, json);
    }

    public async void LoadFromJSON()
    {
        //jsonを読み込んでListを更新
        try
        {
            StreamReader reader = File.OpenText(Path.Combine(_capturedFileDir, "CapturedData.json"));
            string json = await reader.ReadToEndAsync();

            Debug.Log(json);

            _capturedData = JsonConvert.DeserializeObject<List<CapturedData>>(json);

            //_capturedData==nullにならないようにする
            if (_capturedData==null)
            {
                _capturedData = new();
            }
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
        imageData.Dispose();
        return imageData;

    }

    /* public async Task<ImageData> GetRandomData(int partId)
     {
         //partIdが一致したデータの中からランダムに1つを返す
         IEnumerable<CapturedData> query = _capturedData.Where(data => data.partId == partId);

         int count = query.Count();

         int index = UnityEngine.Random.Range(0, count);

         List<CapturedData> captureds = query.ToList();

         CapturedData captured = captureds[index];

         ImageData imageData = await GetData(captured.id);

         return imageData;

     }*/

    public async Task<ImageData> GetRandomData()
    {
        //データの中からランダムに1つを返す
        
        int count = _capturedData.Count();

        int index = UnityEngine.Random.Range(0, count);

        CapturedData captured = _capturedData[index];

        ImageData imageData = await GetData(captured.id);

        captured = null;

        return imageData;

    }

    //public void GetAllData() { }

    public void GetRawJson() { }

    //public ImageData DeleteData(string id)
   // {

   // }

    public void DeleteAllData()
    {
        foreach(CapturedData data in _capturedData)
        {
            string path = Path.Combine(_capturedFileDir, data.id + ".png");

            File.Delete(path);
        }

        _capturedData.Clear();

        UpdateJSON();
    }



}
