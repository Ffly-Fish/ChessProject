using System.Collections;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour,ISquareController
{
    [SerializeField]
    private GameObject SelectionIndicator;//选中点标识
    [SerializeField]
    private GameObject ValidMoveIndicator;//可落子点标识
    [SerializeField]
    private GameObject PathIndicator;//移动路径标识
    [SerializeField]
    private AIType AIType;//AI算法
    [SerializeField]
    private bool isRedHuman = true; // 红方是否由人类控制
    [SerializeField]
    private bool isBlackHuman = true; // 黑方是否由人类控制
    [SerializeField]
    private Text winText;
    // 红方和黑方的计时文本
    [SerializeField]
    private Text redTime, blackTime;
    // 标记对局是否已经结束
    private bool isBattleEnded;
    // 红方和黑方已用时间
    private float redElapsedTime;
    private float blackElapsedTime;
    // 上次显示的整秒数
    private int displayedRedSeconds = -1;
    private int displayedBlackSeconds = -1;
    private Point nullPoint = new Point(-100, -100);
    private List<Board> boards = new List<Board>();//棋盘记录
    private Square[,] squares = new Square[10, 9];//棋格表
    // 当前场景棋盘对应的网络玩家
    private RoomHost roomHost;
    // 标记是否处于网络对局
    private bool isNetworkGame;
    // 网络对局中本地玩家执红方或黑方
    private bool networkIsRed;
    private GameObject selectionIndicatorInstance; // 选中点标识实例
    private readonly List<GameObject> validMoveIndicators = new List<GameObject>(); // 可落子点标识实例
    private readonly List<GameObject> pathIndicators = new List<GameObject>(); // 移动路径标识实例
    [SerializeField]
    private GameObject SquareGO;
    private Point to;//起点

    private void Awake()
    {
        boards.Add(new Board());
        to = nullPoint;//该点代表未赋值
    }
    private void Start()
    {
        AIType = UIManager.gameInfo.AIType;
        isRedHuman = UIManager.gameInfo.isRedHuman;
        isBlackHuman = UIManager.gameInfo.isBlackHuman;
        float zoomCorrection​ = 0.574f;
        float objectScaling​ = this.transform.localScale.x;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                GameObject squareObj = Instantiate(SquareGO, new Vector3((j - 4f) * zoomCorrection * objectScaling, (4.5f - i) * zoomCorrection * objectScaling, 0) + this.transform.position, Quaternion.identity);
                squareObj.transform.SetParent(this.transform);
                squareObj.transform.localScale = new Vector3(1, 1, 1);
                squareObj.name = "Square_" + i + "_" + j;
                Square square = squareObj.GetComponent<Square>();
                square.Create(new Point(i, j), boards[0].board[i, j], this);
                squares[i, j] = square;
            }
        }
        UpdateTimeDisplay();
        AudioManager.Instance.PlaySfx("开局");
        AskAIIfNeeded();
    }

    // 累计当前行棋方用时并刷新计时显示
    private void Update()
    {
        if (!isBattleEnded && boards.Count > 0)
        {
            if (boards[boards.Count - 1].isRed)
            {
                redElapsedTime += Time.deltaTime;
            }
            else
            {
                blackElapsedTime += Time.deltaTime;
            }
        }

        UpdateTimeDisplay();
    }

    // 仅在计时秒数变化时更新文本
    private void UpdateTimeDisplay()
    {
        int redSeconds = Mathf.FloorToInt(redElapsedTime);
        if (redTime != null && redSeconds != displayedRedSeconds)
        {
            redTime.text = FormatTime(redSeconds);
            displayedRedSeconds = redSeconds;
        }

        int blackSeconds = Mathf.FloorToInt(blackElapsedTime);
        if (blackTime != null && blackSeconds != displayedBlackSeconds)
        {
            blackTime.text = FormatTime(blackSeconds);
            displayedBlackSeconds = blackSeconds;
        }
    }

    // 将秒数格式化为分秒文本
    private string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}：{seconds:00}";
    }

    public void AskAI()
    {
        if (isBattleEnded || isNetworkGame)
        {
            return;
        }

        switch (AIType)
        {
            case AIType.极大极小化算法:
            case AIType.强化学习算法:
                Board currentBoard = boards[boards.Count - 1]; // AI 搜索的局面
                Task<Board> searchTask = Task.Run(() => MinimaxAlgorithm.NextBoard(currentBoard));
                StartCoroutine(WaitForAI(searchTask, currentBoard));
                break;
        }
    }

    // 等待 AI 搜索完成后在主线程执行走棋
    private IEnumerator WaitForAI(Task<Board> searchTask, Board searchBoard)
    {
        while (!searchTask.IsCompleted)
        {
            yield return null;
        }

        if (searchTask.IsCanceled)
        {
            yield break;
        }
        if (searchTask.IsFaulted)
        {
            Debug.LogException(searchTask.Exception);
            yield break;
        }
        if (!ReferenceEquals(boards[boards.Count - 1], searchBoard))
        {
            yield break;
        }

        TryMove(searchTask.Result);
    }

    // 根据 AI 返回的新局面提取走法并执行
    public bool TryMove(Board nextBoard)
    {
        Board currentBoard = boards[boards.Count - 1]; // 当前棋盘状态
        if (nextBoard == null || ReferenceEquals(nextBoard, currentBoard)
            || nextBoard.board == null
            || nextBoard.board.GetLength(0) != currentBoard.board.GetLength(0)
            || nextBoard.board.GetLength(1) != currentBoard.board.GetLength(1))
        {
            return false;
        }

        Point from = nullPoint; // AI 走棋起点
        Point destination = nullPoint; // AI 走棋终点
        for (int i = 0; i < currentBoard.board.GetLength(0); i++)
        {
            for (int j = 0; j < currentBoard.board.GetLength(1); j++)
            {
                PieceType oldPiece = currentBoard.board[i, j]; // 走棋前的棋子
                PieceType newPiece = nextBoard.board[i, j]; // 走棋后的棋子
                if (oldPiece == newPiece)
                {
                    continue;
                }

                if (oldPiece != PieceType.空 && newPiece == PieceType.空)
                {
                    if (from != nullPoint)
                    {
                        return false;
                    }
                    from = new Point(i, j);
                }
                else if (newPiece != PieceType.空)
                {
                    if (destination != nullPoint)
                    {
                        return false;
                    }
                    destination = new Point(i, j);
                }
            }
        }

        return from != nullPoint && destination != nullPoint && TryMove(from, destination);
    }

    // 处理棋格点击、棋子选择及合法走棋
    public void GetPoint(Point point)
    {
        if (isBattleEnded || !IsCurrentSideHuman()
            || isNetworkGame && boards[boards.Count - 1].isRed != networkIsRed)
        {
            return;
        }

        Board currentBoard = boards[boards.Count - 1]; // 当前棋盘状态
        if (point.x < 0 || point.x >= 10 || point.y < 0 || point.y >= 9)
        {
            return;
        }

        PieceType selectedPiece = currentBoard.board[point.x, point.y]; // 点击位置的棋子
        if (to == nullPoint)
        {
            if (IsCurrentSidePiece(selectedPiece))
            {
                to = point;
                ShowSelectionIndicators(currentBoard);
            }
            return;
        }

        if (IsCurrentSidePiece(selectedPiece))
        {
            to = point;
            ShowSelectionIndicators(currentBoard);
            return;
        }

        if (!TryMove(to, point))
        {
            to = nullPoint;
            ClearSelectionIndicators();
            return;
        }
    }

    /// <summary>尝试执行指定的走棋，成功时推进棋盘状态。</summary>
    /// <param name="from">起始棋格坐标</param>
    /// <param name="destination">目标棋格坐标</param>
    public bool TryMove(Point from, Point destination)
    {
        if (isBattleEnded)
        {
            return false;
        }

        if (isNetworkGame)
        {
            Board currentBoard = boards[boards.Count - 1];
            if (currentBoard.isRed != networkIsRed
                || !currentBoard.NextBoard(from, destination, out _)
                || roomHost == null)
            {
                return false;
            }

            return roomHost.RequestMove(from, destination);
        }

        return ApplyMove(from, destination);
    }

    // 配置本地棋盘的网络玩家身份
    public void SetNetworkPlayer(RoomHost player, bool isRed)
    {
        roomHost = player;
        networkIsRed = isRed;
        isNetworkGame = true;
        isRedHuman = true;
        isBlackHuman = true;
        StopAllCoroutines();
    }

    // 在场景棋盘上应用服务器广播的走法
    public bool ApplyNetworkMove(Point from, Point destination, bool moverIsRed)
    {
        if (!isNetworkGame || boards.Count == 0 || boards[boards.Count - 1].isRed != moverIsRed)
        {
            return false;
        }

        return ApplyMove(from, destination);
    }

    // 验证并执行走法，成功时推进棋盘状态
    private bool ApplyMove(Point from, Point destination)
    {
        if (isBattleEnded)
        {
            return false;
        }

        Board currentBoard = boards[boards.Count - 1]; // 当前棋盘状态
        if (!currentBoard.NextBoard(from, destination, out Board nextBoard))
        {
            return false;
        }

        PieceType movePiece = currentBoard.board[from.x, from.y]; // 移动的棋子
        if (!IsCurrentSidePiece(movePiece))
        {
            return false;
        }

        PieceType capturedPiece = currentBoard.board[destination.x, destination.y]; // 目标位置原有棋子
        boards.Add(nextBoard);
        squares[from.x, from.y].Change(PieceType.空);
        squares[destination.x, destination.y].Change(movePiece);

        if (capturedPiece != PieceType.空)
        {
            AudioManager.Instance.PlaySfx("吃子");
        }
        else
        {
            AudioManager.Instance.PlaySfx("落子");
        }

        if (nextBoard.IsInCheck(!currentBoard.isRed))
        {
            AudioManager.Instance.PlaySfx("将军");
        }

        to = nullPoint;
        ClearSelectionIndicators();
        ShowPathIndicators(from, destination);
        if (nextBoard.IsCheckmate())
        {
            EndBattle(currentBoard.isRed);
            return true;
        }

        AskAIIfNeeded();
        return true;
    }

    // 判断棋子是否属于当前行棋方
    private bool IsCurrentSidePiece(PieceType piece)
    {
        if (boards[boards.Count - 1].isRed)
        {
            return piece >= PieceType.红兵 && piece <= PieceType.红将;
        }

        return piece >= PieceType.黑兵 && piece <= PieceType.黑将;
    }

    // 判断当前行棋方是否由人类控制
    private bool IsCurrentSideHuman()
    {
        return boards[boards.Count - 1].isRed ? isRedHuman : isBlackHuman;
    }

    // 当前回合由 AI 控制时启动 AI 走棋
    private void AskAIIfNeeded()
    {
        if (!isNetworkGame && !IsCurrentSideHuman())
        {
            AskAI();
        }
    }

    // 撤销最近一步棋并恢复上一局面
    public void Undo()
    {
        to = nullPoint;
        ClearSelectionIndicators();
        ClearPathIndicators();
        if (boards.Count <= 1)
        {
            return;
        }

        Board currentBoard = boards[boards.Count - 1]; // 撤销前的棋盘状态
        bool hasHumanPlayer = isRedHuman || isBlackHuman; // 是否存在人类玩家
        do
        {
            boards.RemoveAt(boards.Count - 1);
            if (boards.Count <= 1 || !hasHumanPlayer || IsCurrentSideHuman())
            {
                break;
            }
        }
        while (boards.Count > 1);

        Board previousBoard = boards[boards.Count - 1]; // 撤销后的棋盘状态
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (currentBoard.board[i, j] != previousBoard.board[i, j])
                {
                    squares[i, j].Change(previousBoard.board[i, j]);
                }
            }
        }

        AudioManager.Instance.PlaySfx("悔棋");
        AskAIIfNeeded();
    }

    // 显示选中棋子及其所有合法落点
    private void ShowSelectionIndicators(Board currentBoard)
    {
        if (SelectionIndicator != null)
        {
            if (selectionIndicatorInstance == null)
            {
                selectionIndicatorInstance = CreateIndicator(SelectionIndicator, to);
            }

            SetIndicatorPosition(selectionIndicatorInstance, to);
        }

        ClearValidMoveIndicators();
        if (ValidMoveIndicator == null)
        {
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                Point target = new Point(i, j); // 候选落点
                if (!currentBoard.NextBoard(to, target, out _))
                {
                    continue;
                }

                GameObject indicator = CreateIndicator(ValidMoveIndicator, target);
                if (indicator != null)
                {
                    indicator.SetActive(true);
                    validMoveIndicators.Add(indicator);
                }
            }
        }
    }

    // 在指定棋格创建标识实例
    private GameObject CreateIndicator(GameObject template, Point point)
    {
        if (template == null || squares[point.x, point.y] == null)
        {
            return null;
        }

        GameObject indicator = Instantiate(template, squares[point.x, point.y].transform.position, template.transform.rotation);
        indicator.transform.SetParent(transform, true);
        indicator.SetActive(false);
        return indicator;
    }

    // 将标识移动到指定棋格
    private void SetIndicatorPosition(GameObject indicator, Point point)
    {
        if (indicator != null && squares[point.x, point.y] != null)
        {
            indicator.transform.position = squares[point.x, point.y].transform.position;
            indicator.SetActive(true);
        }
    }

    // 清除选中点及所有合法落点标识
    private void ClearSelectionIndicators()
    {
        if (selectionIndicatorInstance != null)
        {
            selectionIndicatorInstance.SetActive(false);
        }

        ClearValidMoveIndicators();
    }

    // 清除所有可落子点标识
    private void ClearValidMoveIndicators()
    {
        foreach (GameObject indicator in validMoveIndicators)
        {
            if (indicator != null)
            {
                Destroy(indicator);
            }
        }

        validMoveIndicators.Clear();
    }

    // 在移动起点和终点显示路径标识
    private void ShowPathIndicators(Point from, Point to)
    {
        ClearPathIndicators();
        if (PathIndicator == null)
        {
            return;
        }

        GameObject fromIndicator = CreateIndicator(PathIndicator, from);
        GameObject toIndicator = CreateIndicator(PathIndicator, to);
        if (fromIndicator != null)
        {
            fromIndicator.SetActive(true);
            pathIndicators.Add(fromIndicator);
        }
        if (toIndicator != null)
        {
            toIndicator.SetActive(true);
            pathIndicators.Add(toIndicator);
        }
    }

    // 清除移动路径标识
    private void ClearPathIndicators()
    {
        foreach (GameObject indicator in pathIndicators)
        {
            if (indicator != null)
            {
                Destroy(indicator);
            }
        }

        pathIndicators.Clear();
    }

    //比赛结束
    private void EndBattle(bool isRedWin)
    {
        if (isBattleEnded)
        {
            return;
        }

        isBattleEnded = true;
        MatchRecord.WriterMatch(boards);
        bool isWinnerHuman = isRedWin ? isRedHuman : isBlackHuman;
        AudioManager.Instance.PlaySfx(isWinnerHuman ? "获胜" : "失败");

        if (winText == null)
        {
            return;
        }

        winText.text = isRedWin ? "红 胜 ！" : "黑 胜 ！";
        winText.gameObject.SetActive(true);
        Transform textTransform = winText.transform;
        Vector3 originalScale = textTransform.localScale;
        textTransform.DOKill();
        textTransform.localScale = Vector3.zero;

        Sequence winSequence = DOTween.Sequence();
        winSequence.Append(textTransform.DOScale(originalScale * 1.2f, 0.6f).SetEase(Ease.OutBack));
        winSequence.Join(textTransform.DOLocalRotate(new Vector3(0f, 0f, 360f), 0.8f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic));
        winSequence.Append(textTransform.DOScale(originalScale, 0.2f).SetEase(Ease.OutSine));
    }
}
