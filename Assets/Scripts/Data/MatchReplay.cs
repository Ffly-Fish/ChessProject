using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 显示并浏览已保存的对局记录
public class MatchReplay : MonoBehaviour
{
    // 对局中的棋盘状态记录
    private List<Board> boards = new List<Board>();

    // 场景中的棋格组件
    private Square[,] squares = new Square[10, 9];

    // 当前显示的棋盘记录索引
    private int index = 0;

    // 棋格预制体
    [SerializeField]
    private GameObject SquareGO;

    // 加载对局记录并创建初始棋盘
    void Start()
    {
        // 当前选中的对局文件名
        string fileName = UIManager.gameInfo != null ? UIManager.gameInfo.fileName : null;
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("未选择要回放的对局记录。", this);
            return;
        }

        boards = MatchRecord.ReadMatch(fileName);
        if (boards.Count == 0 || boards[0] == null || boards[0].board == null
            || boards[0].board.GetLength(0) != 10 || boards[0].board.GetLength(1) != 9)
        {
            Debug.LogError("对局记录为空或棋盘数据格式无效。", this);
            return;
        }

        if (SquareGO == null)
        {
            Debug.LogError("棋格预制体未设置。", this);
            return;
        }

        // 棋盘位置缩放修正值
        float zoomCorrection = 0.574f;
        // 当前物体的缩放值
        float objectScaling = transform.localScale.x;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                // 创建棋格对象
                GameObject squareObj = Instantiate(SquareGO, new Vector3((j - 4f) * zoomCorrection * objectScaling, (4.5f - i) * zoomCorrection * objectScaling, 0) + this.transform.position, Quaternion.identity);
                squareObj.transform.SetParent(transform);
                squareObj.transform.localScale = new Vector3(1, 1, 1);
                squareObj.name = "Square_" + i + "_" + j;

                // 初始化棋格组件
                Square square = squareObj.GetComponent<Square>();
                if (square == null)
                {
                    Debug.LogError("棋格预制体中未找到 Square 组件。", squareObj);
                    Destroy(squareObj);
                    continue;
                }

                square.Create(new Point(i, j), boards[0].board[i, j], null);
                squares[i, j] = square;
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx("开局");
        }
    }

    // 显示下一条棋盘记录
    public void NextBoard()
    {
        if (index >= boards.Count - 1)
        {
            return;
        }

        index++;
        UpdateBoard();
    }

    // 显示上一条棋盘记录
    public void LastBoard()
    {
        if (index <= 0)
        {
            return;
        }

        index--;
        UpdateBoard();
    }

    // 显示最后一个棋盘局面
    public void ShowLatestBoard()
    {
        if (boards.Count == 0)
        {
            return;
        }

        index = boards.Count - 1;
        UpdateBoard();
    }

    // 根据当前索引刷新棋盘上的棋子
    private void UpdateBoard()
    {
        // 当前要显示的棋盘状态
        Board board = boards[index];
        for (int row = 0; row < 10; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                if (squares[row, column] != null)
                {
                    squares[row, column].Change(board.board[row, column]);
                }
            }
        }
    }
}
