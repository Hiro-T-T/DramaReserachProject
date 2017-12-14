using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Persistence;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

#if !UNITY_EDITOR
using HoloToolkit.Unity.SpatialMapping;
#endif

public class HoloCreateManager : MonoBehaviour {

    #region Unity - Inspector

    /// <summary>
    /// 初期配置ボタン親オブジェクト。
    /// </summary>
    [SerializeField]
    private GameObject FirstButtons;

    /// <summary>
    /// 生成ボタン親オブジェクト。
    /// </summary>
    [SerializeField]
    private GameObject CreateButtons;

    /// <summary>
    /// モデルリストボタン親オブジェクト。
    /// </summary>
    [SerializeField]
    private GameObject ModelListButtons;

    /// <summary>
    /// モデル移動・削除ボタン親オブジェクト。
    /// </summary>
    [SerializeField]
    private GameObject ModelMoveButtons;

    #endregion

    #region private variables

    /// <summary>
    /// 位置保存用ワールドアンカー。
    /// </summary>
    private WorldAnchorStore anchorStore;

    /// <summary>
    /// ステージデータ。
    /// </summary>
    private Stage stage;

    /// <summary>
    /// モデルステータスリスト。
    /// </summary>
    private List<ModelStatus> modelStatuses;

    /// <summary>
    /// ベッドモデルプレハブ。
    /// </summary>
    private GameObject bedPrefab;

    #endregion

    #region Auto Callback Method

    void Awake()
    {
        WorldAnchorStore.GetAsync(WorldAnchorStoreReady);
    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion

    #region Unity - Button Callback

    /// <summary>
    /// モデルを追加ボタンのコールバック。
    /// </summary>
    public void OnClickCreateButton()
    {
        Debug.Log("Tap");
        FirstButtons.SetActive(false);
        CreateButtons.SetActive(true);
        ModelListButtons.SetActive(false);
        ModelMoveButtons.SetActive(false);
    }

    /// <summary>
    /// モデルの追加>ベッドボタンを押したときのコールバック。
    /// </summary>
    public void OnCreateBed()
    {
        FirstButtons.SetActive(false);
        CreateButtons.SetActive(false);
        ModelListButtons.SetActive(false);
        ModelMoveButtons.SetActive(true);

        GameObject[] objs = GameObject.FindGameObjectsWithTag("Model");
        List<GameObject> objList = new List<GameObject>();
        for (int i = 0; i < objs.Length; i++)
        {
            objList.Add(objs[i]);
        }
        int count = objList == null ? 0 : objList.Count(e => e.GetComponent<ModelStatus>().Category == (int)Category.BED);
        GameObject obj = Instantiate(Resources.Load("bed1"), new Vector3(0, 0, 10), Quaternion.identity) as GameObject;
        ModelStatus status = obj.GetComponent<ModelStatus>();
        status.ID = count + 1;
        status.Name = "Bed" + status.ID.ToString();
        status.Category = (int)Category.BED;
        status.Path = status.Name;
        status.name = status.Name;
        modelStatuses.Add(status);
    }


    #endregion

    /// <summary>
    /// WorldAnchorStore.GetAsync完了時のコールバック。
    /// </summary>
    /// <param name="anchor"></param>
    private void WorldAnchorStoreReady(WorldAnchorStore anchor)
    {
        anchorStore = anchor;

        Initialize();
    }

    /// <summary>
    /// /初期化。
    /// </summary>
    private void Initialize()
    {
        stage = new Stage();
        modelStatuses = new List<ModelStatus>();

        stage = LoadStage();

        if (stage == null) return;

        // ベッド読み込み。
        bedPrefab = Resources.Load("bed1") as GameObject;

        foreach(ModelItem e in stage.Models)
        {
            switch (e.Category)
            {
                case (int)Category.BED:
                    GameObject obj = Instantiate(bedPrefab, new Vector3(0, 0, 10), Quaternion.identity);
                    ModelStatus item = obj.GetComponent<ModelStatus>();
                    item.ID = e.ID;
                    item.Name = e.Name;
                    item.Path = e.Path;
                    item.IsAnchor = true;
                    obj.name = e.Name;
                    anchorStore.Load(e.Path, obj);
                    modelStatuses.Add(item);
                    break;
            }
            
        }
    }

    /// <summary>
    /// ステージデータのロード。
    /// </summary>
    /// <returns></returns>
    private Stage LoadStage()
    {
        string adress = Application.persistentDataPath + "SaveModel.json";

        if(!File.Exists(adress))
        {
            FileInfo files = new FileInfo(adress);

            string result = "";

            try
            {
                using (StreamReader sr = new StreamReader(files.OpenRead(), System.Text.Encoding.UTF8))
                {
                    result = sr.ReadToEnd();
                    return JsonUtility.FromJson<Stage>(result);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

}

[Serializable]
public class Stage
{
    public List<ModelItem> Models;
}

[Serializable]
public class ModelItem
{

    /// <summary>
    /// ID。
    /// </summary>
    public int ID;

    /// <summary>
    /// World Anchor使用時のパスフレーズ。
    /// </summary>
    public string Path;

    /// <summary>
    /// オブジェクトの名前。
    /// </summary>
    public string Name;

    /// <summary>
    /// モデルのカテゴリー。
    /// </summary>
    public int Category;
}

public enum Category
{
    BED,
}

