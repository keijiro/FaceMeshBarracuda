using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

public class JsonSettings : MonoBehaviour
{
    string _settingPath;

    public Settings _settings { get; private set; }

    //ここを書き換える
    public class Settings
    {
        public string deviceID { get; set; }

        public bool isMirrored { get; set; }
    }

    private void Awake()
    {
        _settingPath = Application.persistentDataPath;
        _settings = new Settings();
        Debug.Log(_settings.ToString());
        LoadFromJSON();

    }


    void UpdateJSON()
    {
        //Jsonに書き出し
        string json = JsonConvert.SerializeObject(_settings);

        Debug.Log(json);

        //jsonFilieを更新
        string path = Path.Combine(_settingPath, "settings.json");

        try
        {
            File.WriteAllText(path, json);
        }
        catch (System.Exception exception)
        {
            Debug.LogError(exception.Message);
        }

    }

    void LoadFromJSON()
    {
        //jsonを読み込み
        try
        {
            string path = Path.Combine(_settingPath, "settings.json");

            if (!File.Exists(path))
            {
                UpdateJSON();
            }

            StreamReader reader = File.OpenText(path);
            string json = reader.ReadToEnd();

            Debug.Log(json);

            _settings = JsonConvert.DeserializeObject<Settings>(json);

        }
        catch(System.Exception exception)
        {
            Debug.LogError(exception.Message);
        }
    }

    //任意のkeyの値を変更する
    public void UpdateSettings<T>(string key, T value)
    {
        var keyProperty = typeof(Settings).GetProperty(key);
        keyProperty.SetValue(_settings, value);
        UpdateJSON();
    }
}
