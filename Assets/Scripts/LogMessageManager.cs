using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogMessageManager : MonoBehaviour {

    /// <summary>
    /// デバッグログ用テキストオブジェクト
    /// </summary>
    [SerializeField, Tooltip("デバッグログ用テキストオブジェクト")]
    private GameObject p_TargetDebugPanelObject;
    /// <summary>
    /// デバッグログ用テキスト
    /// </summary>
    private UnityEngine.UI.Text p_Text;

    /// <summary>
    /// 表示行数
    /// </summary>
    [SerializeField, Tooltip("表示行数")]
    private int p_LineNum = 17;

    /// <summary>
    /// 保持テキスト
    /// </summary>
    private string p_TextMessage;

    /// <summary>
    /// 初期化関数
    /// </summary>
    private void Start()
    {
        // Logメッセージイベント追加
        Application.logMessageReceived += LogMessageOutput;

        p_Text = p_TargetDebugPanelObject.GetComponent<UnityEngine.UI.Text>();
    }

    /// <summary>
    /// Logメッセージイベント処理
    /// </summary>
    private void LogMessageOutput(string condition, string stackTrace, LogType type)
    {
        string textmessage = p_TextMessage;
        textmessage += condition + System.Environment.NewLine;

        string newline = System.Environment.NewLine;
        string[] lines = textmessage.Split(new string[] { newline }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > p_LineNum)
        {
            textmessage = "";
            for (int line = lines.Length - p_LineNum; line < lines.Length; line++)
            {
                textmessage += lines[line] + System.Environment.NewLine;
            }
        }

        p_TextMessage = textmessage;
        p_Text.text = textmessage;
    }
}
