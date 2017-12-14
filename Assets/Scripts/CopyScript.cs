using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Persistence;

#if UNITY_UWP
using Windows.Storage;
using System.Threading.Tasks;
#endif

#if !UNITY_EDITOR
using HoloToolkit.Unity.SpatialMapping;
#endif

public class CreateManager : MonoBehaviour
{

    public Text InfoText;

    public Text CountText;

    public StageData stage;

    // ---UIButton-------------------------

    public List<GameObject> FirstButtons;

    public List<GameObject> CreateButtons;

    public List<GameObject> ModelListButtons;

    public List<GameObject> SetRemoveButton;

    public ButtonStatus AbstructButton;

    // ------------------------------------

    private GameObject _NowSelectObj;

    private ModelStatus _NowSelectItem;

    private GameObject bedPrefab;

    private List<ModelStatus> modelItems;

    private WorldAnchorStore anchorStore;

    private float time;

    // Use this for initialization
    void Awake()
    {
        WorldAnchorStore.GetAsync(WorldAnchorStoreReady);
    }

    // Update is called once per frame
    void Update()
    {
        //CountText.text = "モデル数 : " + GameObject.FindGameObjectsWithTag("Model").Length.ToString();
        time += Time.deltaTime;
        CountText.text = time.ToString();
    }

    public void OnCreateBed()
    {
        FirstButtons.ForEach(e => e.SetActive(false));
        CreateButtons.ForEach(e => e.SetActive(false));
        ModelListButtons.ForEach(e => e.SetActive(false));
        SetRemoveButton.ForEach(e => e.SetActive(true));


        GameObject[] objs = GameObject.FindGameObjectsWithTag("Model");
        List<GameObject> objList = new List<GameObject>();
        for (int i = 0; i < objs.Length; i++)
        {
            objList.Add(objs[i]);
        }
        int count = objList == null ? 0 : objList.Count(e => e.GetComponent<ModelStatus>().Category == (int)CATEGORY.BED);
        GameObject obj = Instantiate(Resources.Load("bed1"), new Vector3(0, 0, 10), Quaternion.identity) as GameObject;
        ModelStatus status = obj.GetComponent<ModelStatus>();
        status.ID = count + 1;
        status.Name = "Bed" + status.ID.ToString();
        status.Category = (int)CATEGORY.BED;
        status.Path = status.Name;
        status.name = status.Name;
        modelItems.Add(status);
        InfoText.text = "Model is Created!";
    }

    /// <summary>
    /// WorldAnchorStore.GetAsync完了時のコールバック
    /// </summary>
    /// <param name="anchorStore">取得できたWorldAnchorStoreのインスタンス</param>
    private void WorldAnchorStoreReady(WorldAnchorStore anchor)
    {
        anchorStore = anchor;

        Initialized();
    }

    private void Initialized()
    {
        stage = new StageData();
        modelItems = new List<ModelStatus>();
        TextAsset modelAsset = Resources.Load("Json/ModelJson.json") as TextAsset;
        string json = modelAsset.text;
        stage = Load();

        if (stage == null) return;

        bedPrefab = Resources.Load("bed1") as GameObject;

        foreach (ModelItem e in stage.Models)
        {
            switch (e.category)
            {
                case (int)CATEGORY.BED:
                    GameObject obj = Instantiate(bedPrefab, new Vector3(0, 0, 10), Quaternion.identity);
                    ModelStatus item = obj.GetComponent<ModelStatus>();
                    item.ID = e.id;
                    item.Name = e.name;
                    item.Path = e.path;
                    item.IsAnchor = true;
                    obj.name = item.Name;
                    anchorStore.Load(e.path, obj);
                    modelItems.Add(item);
                    break;
            }

        }
    }

    public void OnAnchor()
    {
        WorldAnchor anc = _NowSelectObj.AddComponent<WorldAnchor>();
        _NowSelectItem.IsMoving = false;
        _NowSelectItem.IsAnchor = true;
        bool successful = anchorStore.Save(_NowSelectItem.Path, anc);
        ModelItem item = new ModelItem();
        item.id = _NowSelectItem.ID;
        item.name = _NowSelectItem.name;
        item.path = _NowSelectItem.Path;
        item.category = _NowSelectItem.Category;
        stage.Models.Add(item);
        SaveJson();

        FirstButtons.ForEach(e => e.SetActive(true));
        CreateButtons.ForEach(e => e.SetActive(false));
        ModelListButtons.ForEach(e => e.SetActive(false));
        SetRemoveButton.ForEach(e => e.SetActive(false));
    }

    public void RemoveAnchor()
    {
        DestroyImmediate(_NowSelectObj.GetComponent<WorldAnchor>());
        Destroy(_NowSelectObj.gameObject);
        bool successful = anchorStore.Delete(_NowSelectItem.Path);
        stage.Models.RemoveAll(e => e.id == _NowSelectItem.ID);
        modelItems.RemoveAll(e => e.ID == _NowSelectItem.ID);

        FirstButtons.ForEach(e => e.SetActive(true));
        CreateButtons.ForEach(e => e.SetActive(false));
        ModelListButtons.ForEach(e => e.SetActive(false));
        SetRemoveButton.ForEach(e => e.SetActive(false));
    }

    public void OnModelCreateButton()
    {
        Debug.Log("モデル生成");
        FirstButtons.ForEach(e => e.SetActive(false));
        CreateButtons.ForEach(e => e.SetActive(true));
        ModelListButtons.ForEach(e => e.SetActive(false));
        SetRemoveButton.ForEach(e => e.SetActive(false));
    }

    public void OnSetModelButton()
    {
        Debug.Log("モデル配置");
        FirstButtons.ForEach(e => e.SetActive(false));
        CreateButtons.ForEach(e => e.SetActive(false));
        ModelListButtons.ForEach(e => Destroy(e.gameObject));
        SetRemoveButton.ForEach(e => e.SetActive(false));
        ModelListButtons.Clear();
        // 一覧取得してアブストラクトをうんぬん
        foreach (ModelStatus e in modelItems)
        {
            ButtonStatus status = Instantiate(AbstructButton);
            status.name = e.Name;
            status.NameText.text = e.Name;
            status.Category = e.Category;
            status.SetCallbackOnClick(OnCallbackSelectModelButton);
            ModelListButtons.Add(status.gameObject);
            //status.transform.SetParent(transform, false);
            status.gameObject.SetActive(true);
        }
    }

    public void OnCallbackSelectModelButton(int id, int category)
    {
        foreach (ModelStatus e in modelItems)
        {
            if (e.Category == category && e.ID == id)
            {
                _NowSelectItem = e;
                e.IsMoving = true;
                _NowSelectObj = e.gameObject;
            }
        }
    }


    private StageData Load()
    {
        string adress = Application.persistentDataPath + "SaveModel.json";
        StreamWriter sw;
        if (!File.Exists(adress))
        {

            FileInfo files = new FileInfo(adress);

            string result = "";

            try
            {
                using (StreamReader sr = new StreamReader(files.OpenRead(), System.Text.Encoding.UTF8))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                return null;
            }

            return JsonUtility.FromJson<StageData>(result);
        }
        else
        {
            return null;
        }
    }


    private void SaveJson()
    {
        string adress = Application.persistentDataPath + "SaveModel.json";
        StreamWriter sw;
        if (!File.Exists(adress))
        {
            sw = File.CreateText(adress);
            sw.Flush();
            sw.Dispose();
        }

        string json = JsonUtility.ToJson(stage);

        sw = new StreamWriter(new FileStream(adress, FileMode.Open), System.Text.Encoding.UTF8);
        sw.WriteLine(json);
        sw.Flush();
        sw.Dispose();
    }

    /*

    /// <summary>
    /// ファイルパスがあるかどうかを調べる。
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<bool> IsFileExistAsync(string filePath)
    {
        var folder = ApplicationData.Current.RoamingFolder;

        try
        {
            var url = "ms-appdata:///roaming/" + filePath;
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(url));

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// ファイルを読み込む
    /// </summary>
    /// <param name="fileName">ファイルパス</param>
    /// <returns>ファイルのbyte型の配列が戻り値</returns>
    public static async Task<string> ReadFileText(string filePath)
    {
        var url = "ms-appdata:///roaming/" + filePath;
        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(url));
        var text = await FileIO.ReadTextAsync(file);
        return text;

    }

    /// <summary>
    /// 書き出す
    /// </summary>
    /// <param name="folderPath">Folderパス</param>
    /// <param name="fileName">ファイルパス</param>
    /// <param name="body">stringの文字データ</param>
    public static async void WriteFileText(string folderPath, string fileName, string body)
    {
        // ローミングフォルダ
        var folder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync(folderPath, CreationCollisionOption.OpenIfExists);

        // ファイル（存在すれば上書き）
        var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

        // 書き込み
        using (var rStream = await file.OpenAsync(FileAccessMode.ReadWrite))
        using (var oStream = rStream.GetOutputStreamAt(0))
        {
            var writer = new DataWriter(oStream);
            writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            writer.WriteString(body);
            await writer.StoreAsync();
        }

    }

    */
}

[Serializable]
public class StageData
{
    public List<ModelItem> Models;
}


[Serializable]
public class ModelItem
{
    public int id;
    public string path;
    public string name;
    public int category;
}

public enum CATEGORY
{
    BED = 0,
    CHAIR = 1,
}
