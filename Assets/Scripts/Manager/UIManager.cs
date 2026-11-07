using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// UI菜单数据
[System.Serializable]
public struct MenuData
{
    // 菜单名称
    public string Name;

    // UI菜单对象
    public GameObject Menu;
}

// UI菜单管理器
public class UIManager : MonoBehaviour
{
    // UI管理器单例
    public static UIManager Instance { get; private set; }

    // 可在面板配置的菜单列表
    public List<MenuData> MenuList = new List<MenuData>();

    // 用于记录已打开菜单的栈
    private readonly Stack<MenuData> _menuStack = new Stack<MenuData>();

    // 全局游戏状态信息对象
    public static GameInfo gameInfo;

    // 初始化单例并清理重复对象
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameInfo=new GameInfo();
    }



    private void Start()
    {
        OpenMenu("主菜单");
    }

    // 销毁时清除单例引用
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>打开指定名称的UI菜单。</summary>
    /// <param name="name">要打开的菜单名称</param>
    public void OpenMenu(string name)
    {
        bool isReload = name == "this"; // 是否重新加载当前菜单
        if (isReload)
        {
            if (_menuStack.Count == 0)
            {
                return;
            }

            name = _menuStack.Peek().Name;
        }

        // 查找名称匹配的菜单
        int index = MenuList.FindIndex(menuData => menuData.Name == name);
        if (index < 0)
        {
            return;
        }

        // 获取要打开的菜单数据
        MenuData menuData = MenuList[index];
        if (menuData.Menu == null)
        {
            return;
        }

        if (isReload)
        {
            MenuData currentMenu = _menuStack.Pop(); // 当前已打开的菜单
            Destroy(currentMenu.Menu);
        }

        // 先停用当前栈顶菜单
        if (_menuStack.Count > 0)
        {
            _menuStack.Peek().Menu.SetActive(false);
        }

        // 实例化菜单并记录到栈中
        menuData.Menu = Instantiate(menuData.Menu);
        menuData.Menu.SetActive(true);
        _menuStack.Push(menuData);
        AudioManager.Instance.PlaySfx("按钮点击");
    }

    /// <summary>销毁当前菜单并返回上一层菜单。</summary>
    public void GoBack()
    {
        if (_menuStack.Count == 0)
        {
            return;
        }

        // 销毁并移除栈顶菜单
        MenuData menuData = _menuStack.Pop();
        Destroy(menuData.Menu);

        // 激活新的栈顶菜单
        if (_menuStack.Count > 0)
        {
            _menuStack.Peek().Menu.SetActive(true);
        }
        else if (Application.isEditor)
            EditorApplication.ExitPlaymode();
        else Application.Quit();

    }
}
