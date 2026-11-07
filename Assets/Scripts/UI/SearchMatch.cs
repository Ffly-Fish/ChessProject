using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 显示已保存对局列表
public class SearchMatch : MonoBehaviour
{
    // 对局列表条目预制体
    [SerializeField]
    private GameObject MatchFile;

    // 根据已保存的对局创建列表条目
    void Start()
    {
        if (MatchFile == null)
        {
            Debug.LogError("对局列表条目预制体未设置。", this);
            return;
        }

        // 已保存对局的文件名列表
        List<string> fileNames = MatchRecord.ReadLibrary;
        foreach (string fileName in fileNames)
        {
            // 创建并挂载对局列表条目
            GameObject matchFile = Instantiate(MatchFile, transform);

            // 设置条目中的文本内容
            TextMeshProUGUI text = matchFile.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = fileName;
            }
            else
            {
                Debug.LogError("对局列表条目中未找到 Text 组件。", matchFile);
            }

            // 设置条目按钮对应的对局文件名
            BtnController button = matchFile.GetComponentInChildren<BtnController>();
            if (button != null)
            {
                button.fileName = fileName;
                button.setFileName = true;
            }
            else
            {
                Debug.LogError("对局列表条目中未找到 BtnController 组件。", matchFile);
            }
        }
    }
}
