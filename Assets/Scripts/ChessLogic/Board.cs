using System.Collections;
using System.Collections.Generic;

public class Board
{
    public PieceType[,] board{ get; private set; }
    // 当前应由红方行棋时为 true
    public bool isRed { get; private set; }

    public Board()
    {
        this.board = new PieceType[10, 9];
        this.isRed = true;
        InitializeStartingPosition();
    }

    // 创建指定棋盘并设置当前行棋方
    public Board(PieceType[,] board, bool isRed = true)
    {
        this.board = board;
        this.isRed = isRed;
    }

    public bool NextBoard(Point from,Point to,out Board board)
    {
        // 默认返回空局面，非法走法不会产生新棋盘
        board = null;
        if (this.board == null || this.board.GetLength(0) != 10 || this.board.GetLength(1) != 9
            || !IsInsideBoard(from) || !IsInsideBoard(to) || (from.x == to.x && from.y == to.y))
        {
            return false;
        }

        // 获取起点棋子和目标位置棋子
        PieceType movingPiece = this.board[from.x, from.y];
        PieceType targetPiece = this.board[to.x, to.y];
        if (movingPiece == PieceType.空 || IsRedPiece(movingPiece) != this.isRed
            || IsSameSide(movingPiece, targetPiece)
            || !IsMovementValid(from, to, movingPiece, targetPiece))
        {
            return false;
        }

        // 在复制的棋盘上执行走子
        PieceType[,] nextPieces = (PieceType[,])this.board.Clone();
        nextPieces[to.x, to.y] = movingPiece;
        nextPieces[from.x, from.y] = PieceType.空;
        Board nextBoard = new Board(nextPieces, !this.isRed);

        // 不允许走子后己方将帅仍处于被将状态
        if (nextBoard.IsInCheck(IsRedPiece(movingPiece)))
        {
            return false;
        }

        board = nextBoard;
        return true;
    }

    // 判断坐标是否位于棋盘范围内
    private bool IsInsideBoard(Point point)
    {
        return point.x >= 0 && point.x < this.board.GetLength(0)
            && point.y >= 0 && point.y < this.board.GetLength(1);
    }

    // 判断指定棋子的移动方式是否合法
    private bool IsMovementValid(Point from, Point to, PieceType piece, PieceType target)
    {
        int rowDelta = to.x - from.x;
        int columnDelta = to.y - from.y;
        int rowDistance = System.Math.Abs(rowDelta);
        int columnDistance = System.Math.Abs(columnDelta);
        bool isRed = IsRedPiece(piece);

        switch (piece)
        {
            case PieceType.红车:
            case PieceType.黑车:
                return (rowDelta == 0 || columnDelta == 0) && CountPiecesBetween(from, to) == 0;

            case PieceType.红马:
            case PieceType.黑马:
                if (rowDistance == 2 && columnDistance == 1)
                {
                    return this.board[from.x + System.Math.Sign(rowDelta), from.y] == PieceType.空;
                }
                if (rowDistance == 1 && columnDistance == 2)
                {
                    return this.board[from.x, from.y + System.Math.Sign(columnDelta)] == PieceType.空;
                }
                return false;

            case PieceType.红象:
            case PieceType.黑象:
                return rowDistance == 2 && columnDistance == 2
                    && this.board[from.x + rowDelta / 2, from.y + columnDelta / 2] == PieceType.空
                    && (isRed ? to.x >= 5 : to.x <= 4);

            case PieceType.红士:
            case PieceType.黑士:
                return rowDistance == 1 && columnDistance == 1 && IsInsidePalace(to, isRed);

            case PieceType.红将:
            case PieceType.黑将:
                if (columnDelta == 0 && target == (isRed ? PieceType.黑将 : PieceType.红将)
                    && CountPiecesBetween(from, to) == 0)
                {
                    return true;
                }
                return rowDistance + columnDistance == 1 && IsInsidePalace(to, isRed);

            case PieceType.红炮:
            case PieceType.黑炮:
                if (rowDelta != 0 && columnDelta != 0)
                {
                    return false;
                }
                int piecesBetween = CountPiecesBetween(from, to);
                return target == PieceType.空 ? piecesBetween == 0 : piecesBetween == 1;

            case PieceType.红兵:
                return rowDelta == -1 && columnDelta == 0
                    || from.x <= 4 && rowDelta == 0 && columnDistance == 1;

            case PieceType.黑兵:
                return rowDelta == 1 && columnDelta == 0
                    || from.x >= 5 && rowDelta == 0 && columnDistance == 1;

            default:
                return false;
        }
    }

    // 统计起点和终点之间的棋子数量
    private int CountPiecesBetween(Point from, Point to)
    {
        int rowStep = System.Math.Sign(to.x - from.x);
        int columnStep = System.Math.Sign(to.y - from.y);
        int row = from.x + rowStep;
        int column = from.y + columnStep;
        int count = 0;

        while (row != to.x || column != to.y)
        {
            if (this.board[row, column] != PieceType.空)
            {
                count++;
            }
            row += rowStep;
            column += columnStep;
        }

        return count;
    }

    // 判断位置是否在指定一方的九宫内
    private bool IsInsidePalace(Point point, bool isRed)
    {
        return point.y >= 3 && point.y <= 5 && (isRed ? point.x >= 7 : point.x <= 2);
    }

    // 判断棋子是否属于红方
    private bool IsRedPiece(PieceType piece)
    {
        return piece >= PieceType.红兵 && piece <= PieceType.红将;
    }

    // 判断棋子是否属于黑方
    private bool IsBlackPiece(PieceType piece)
    {
        return piece >= PieceType.黑兵 && piece <= PieceType.黑将;
    }

    // 判断两个棋子是否属于同一方
    private bool IsSameSide(PieceType first, PieceType second)
    {
        return IsRedPiece(first) && IsRedPiece(second)
            || IsBlackPiece(first) && IsBlackPiece(second);
    }

    // 判断指定一方的将帅是否被将
    // 判断指定一方的将帅是否被将
    public bool IsInCheck(bool isRed)
    {
        PieceType general = isRed ? PieceType.红将 : PieceType.黑将;
        int generalRow = -1;
        int generalColumn = -1;

        for (int row = 0; row < this.board.GetLength(0); row++)
        {
            for (int column = 0; column < this.board.GetLength(1); column++)
            {
                if (this.board[row, column] == general)
                {
                    generalRow = row;
                    generalColumn = column;
                    break;
                }
            }
            if (generalRow >= 0)
            {
                break;
            }
        }

        // 没有将帅的局面不进行将军检测
        if (generalRow < 0)
        {
            return false;
        }

        Point generalPoint = new Point(generalRow, generalColumn);
        for (int row = 0; row < this.board.GetLength(0); row++)
        {
            for (int column = 0; column < this.board.GetLength(1); column++)
            {
                PieceType piece = this.board[row, column];
                if (piece == PieceType.空 || IsRedPiece(piece) == isRed)
                {
                    continue;
                }

                Point piecePoint = new Point(row, column);
                if (IsMovementValid(piecePoint, generalPoint, piece, general))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // 判断当前行棋方是否绝杀
    public bool IsCheckmate()
    {
        if (this.board == null)
        {
            return false;
        }

        if (!HasGeneral(this.isRed))
        {
            return true;
        }

        if (!IsInCheck(this.isRed))
        {
            return false;
        }

        for (int fromRow = 0; fromRow < this.board.GetLength(0); fromRow++)
        {
            for (int fromColumn = 0; fromColumn < this.board.GetLength(1); fromColumn++)
            {
                PieceType piece = this.board[fromRow, fromColumn];
                if (piece == PieceType.空 || IsRedPiece(piece) != this.isRed)
                {
                    continue;
                }

                Point from = new Point(fromRow, fromColumn);
                for (int toRow = 0; toRow < this.board.GetLength(0); toRow++)
                {
                    for (int toColumn = 0; toColumn < this.board.GetLength(1); toColumn++)
                    {
                        if (NextBoard(from, new Point(toRow, toColumn), out _))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    // 判断指定一方的将帅是否仍在棋盘上
    private bool HasGeneral(bool isRed)
    {
        PieceType general = isRed ? PieceType.红将 : PieceType.黑将;
        foreach (PieceType piece in this.board)
        {
            if (piece == general)
            {
                return true;
            }
        }

        return false;
    }

    // 按中国象棋初始位置摆放棋子
    private void InitializeStartingPosition()
    {
        // 先将棋盘所有位置初始化为空
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int column = 0; column < board.GetLength(1); column++)
            {
                board[row, column] = PieceType.空;
            }
        }

        // 黑方底线
        board[0, 0] = PieceType.黑车;
        board[0, 1] = PieceType.黑马;
        board[0, 2] = PieceType.黑象;
        board[0, 3] = PieceType.黑士;
        board[0, 4] = PieceType.黑将;
        board[0, 5] = PieceType.黑士;
        board[0, 6] = PieceType.黑象;
        board[0, 7] = PieceType.黑马;
        board[0, 8] = PieceType.黑车;

        // 黑方炮位和兵位
        board[2, 1] = PieceType.黑炮;
        board[2, 7] = PieceType.黑炮;
        board[3, 0] = PieceType.黑兵;
        board[3, 2] = PieceType.黑兵;
        board[3, 4] = PieceType.黑兵;
        board[3, 6] = PieceType.黑兵;
        board[3, 8] = PieceType.黑兵;

        // 红方兵位和炮位
        board[6, 0] = PieceType.红兵;
        board[6, 2] = PieceType.红兵;
        board[6, 4] = PieceType.红兵;
        board[6, 6] = PieceType.红兵;
        board[6, 8] = PieceType.红兵;
        board[7, 1] = PieceType.红炮;
        board[7, 7] = PieceType.红炮;

        // 红方底线
        board[9, 0] = PieceType.红车;
        board[9, 1] = PieceType.红马;
        board[9, 2] = PieceType.红象;
        board[9, 3] = PieceType.红士;
        board[9, 4] = PieceType.红将;
        board[9, 5] = PieceType.红士;
        board[9, 6] = PieceType.红象;
        board[9, 7] = PieceType.红马;
        board[9, 8] = PieceType.红车;
    }
}
