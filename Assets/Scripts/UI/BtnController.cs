using System.Collections;
using UnityEngine;

// 按钮控制器
public class BtnController : MonoBehaviour
{
    // 按钮点击后打开的菜单名称
    public string nextUIName = "返回";

    // 点击时是否将红方设置为人类控制
    public bool setRedHuman;

    // 点击时是否将红方设置为电脑控制
    public bool setRedAI;

    // 点击时是否将黑方设置为人类控制
    public bool setBlackHuman;

    // 点击时是否将黑方设置为电脑控制
    public bool setBlackAI;

    // 点击时是否使用极大极小化算法
    public bool setMinimaxAlgorithm;

    // 点击时是否使用强化学习算法
    public bool setReinforcementLearningAlgorithm;

    public bool setFileName;
    public string fileName;

    // 初始化按钮点击事件
    void Start()
    {
        transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
            if (setRedHuman)
                SetRedHuman();
            if (setRedAI)
                SetRedAI();
            if (setBlackHuman)
                SetBlackHuman();
            if (setBlackAI)
                SetBlackAI();
            if (setMinimaxAlgorithm)
                SetMinimaxAlgorithm();
            if (setReinforcementLearningAlgorithm)
                SetReinforcementLearningAlgorithm();
            if (setFileName)
                SetFileName();

            if (nextUIName == "返回")
                UIManager.Instance.GoBack();
            else
                UIManager.Instance.OpenMenu(nextUIName);
        });
    }

    // 将红方设置为人类控制
    private void SetRedHuman()
    {
        UIManager.gameInfo.isRedHuman = true;
    }

    // 将红方设置为电脑控制
    private void SetRedAI()
    {
        UIManager.gameInfo.isRedHuman = false;
    }

    // 将黑方设置为人类控制
    private void SetBlackHuman()
    {
        UIManager.gameInfo.isBlackHuman = true;
    }

    // 将黑方设置为电脑控制
    private void SetBlackAI()
    {
        UIManager.gameInfo.isBlackHuman = false;
    }

    // 将 AI 设置为极大极小化算法
    private void SetMinimaxAlgorithm()
    {
        UIManager.gameInfo.AIType = AIType.极大极小化算法;
    }

    // 将 AI 设置为强化学习算法
    private void SetReinforcementLearningAlgorithm()
    {
        UIManager.gameInfo.AIType = AIType.强化学习算法;
    }

    private void SetFileName()
    {
        UIManager.gameInfo.fileName = fileName;
    }
}
