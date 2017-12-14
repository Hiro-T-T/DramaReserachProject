using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelStatus : MonoBehaviour {

    /// <summary>
    /// ID。
    /// </summary>
    public int ID;

    /// <summary>
    /// World Anchor使用時のパス。
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

    /// <summary>
    /// アンカーがセットされているかどうか。
    /// </summary>
    public bool IsAnchor;

    /// <summary>
    /// 移動中かどうか。
    /// </summary>
    public bool IsMoving = false;

	// Update is called once per frame
	void Update () {
        this.transform.position = Camera.main.transform.position + new Vector3(0.2f, 0.2f, 0) + Camera.main.transform.forward * 2;
	}
}
