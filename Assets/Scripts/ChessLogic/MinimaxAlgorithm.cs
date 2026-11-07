using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// 提供并行极大极小搜索与局面评估
public static class MinimaxAlgorithm
{
    private const int SearchDepth = 6;
    private const int MateScore = 1000000;
    private const int InfinityScore = MateScore + SearchDepth;

    // 搜索候选局面及其静态评估分数
    private struct MoveCandidate
    {
        public Board board;
        public int score;

        public MoveCandidate(Board board, int score)
        {
            this.board = board;
            this.score = score;
        }
    }

    // 并行搜索当前局面下的最佳下一步棋
    public static Board NextBoard(Board currentBoard)
    {
        if (currentBoard == null || currentBoard.board == null)
        {
            return currentBoard;
        }

        List<MoveCandidate> candidates = GenerateMoves(currentBoard);
        if (candidates.Count == 0)
        {
            return currentBoard;
        }

        OrderMoves(candidates, currentBoard.isRed);
        Task<int>[] searchTasks = new Task<int>[candidates.Count];
        for (int index = 0; index < candidates.Count; index++)
        {
            Board candidateBoard = candidates[index].board;
            searchTasks[index] = Task.Run(() => Search(candidateBoard, SearchDepth - 1, -InfinityScore, InfinityScore));
        }
        Task.WaitAll(searchTasks);

        int bestIndex = 0;
        for (int index = 1; index < searchTasks.Length; index++)
        {
            if (currentBoard.isRed
                ? searchTasks[index].Result > searchTasks[bestIndex].Result
                : searchTasks[index].Result < searchTasks[bestIndex].Result)
            {
                bestIndex = index;
            }
        }

        return candidates[bestIndex].board;
    }

    // 公开的局面评估，正分表示红方占优，负分表示黑方占优
    public static int Evaluate(Board currentBoard)
    {
        if (currentBoard == null || currentBoard.board == null)
        {
            return 0;
        }

        int score = 0;
        bool hasRedGeneral = false;
        bool hasBlackGeneral = false;
        for (int row = 0; row < currentBoard.board.GetLength(0); row++)
        {
            for (int column = 0; column < currentBoard.board.GetLength(1); column++)
            {
                PieceType piece = currentBoard.board[row, column];
                if (piece == PieceType.空)
                {
                    continue;
                }

                bool isRedPiece = IsRedPiece(piece);
                int pieceScore = GetPieceScore(piece, row, column);
                score += isRedPiece ? pieceScore : -pieceScore;
                hasRedGeneral |= piece == PieceType.红将;
                hasBlackGeneral |= piece == PieceType.黑将;
            }
        }

        if (!hasRedGeneral)
        {
            return -MateScore;
        }
        if (!hasBlackGeneral)
        {
            return MateScore;
        }

        return score;
    }

    // 使用 Alpha-Beta 剪枝搜索后续局面
    private static int Search(Board currentBoard, int depth, int alpha, int beta)
    {
        int evaluation = Evaluate(currentBoard);
        if (depth <= 0 || evaluation >= MateScore || evaluation <= -MateScore)
        {
            return evaluation;
        }

        List<MoveCandidate> candidates = GenerateMoves(currentBoard);
        if (candidates.Count == 0)
        {
            int ply = SearchDepth - depth;
            return currentBoard.isRed ? -MateScore + ply : MateScore - ply;
        }

        OrderMoves(candidates, currentBoard.isRed);
        if (currentBoard.isRed)
        {
            int bestScore = -InfinityScore;
            foreach (MoveCandidate candidate in candidates)
            {
                bestScore = Math.Max(bestScore, Search(candidate.board, depth - 1, alpha, beta));
                alpha = Math.Max(alpha, bestScore);
                if (alpha >= beta)
                {
                    break;
                }
            }
            return bestScore;
        }

        int worstScore = InfinityScore;
        foreach (MoveCandidate candidate in candidates)
        {
            worstScore = Math.Min(worstScore, Search(candidate.board, depth - 1, alpha, beta));
            beta = Math.Min(beta, worstScore);
            if (alpha >= beta)
            {
                break;
            }
        }
        return worstScore;
    }

    // 生成当前行棋方的所有合法后继局面
    private static List<MoveCandidate> GenerateMoves(Board currentBoard)
    {
        List<MoveCandidate> candidates = new List<MoveCandidate>();
        for (int fromRow = 0; fromRow < currentBoard.board.GetLength(0); fromRow++)
        {
            for (int fromColumn = 0; fromColumn < currentBoard.board.GetLength(1); fromColumn++)
            {
                PieceType piece = currentBoard.board[fromRow, fromColumn];
                if (piece == PieceType.空 || IsRedPiece(piece) != currentBoard.isRed)
                {
                    continue;
                }

                Point from = new Point(fromRow, fromColumn);
                for (int toRow = 0; toRow < currentBoard.board.GetLength(0); toRow++)
                {
                    for (int toColumn = 0; toColumn < currentBoard.board.GetLength(1); toColumn++)
                    {
                        Point to = new Point(toRow, toColumn);
                        if (currentBoard.NextBoard(from, to, out Board nextBoard))
                        {
                            candidates.Add(new MoveCandidate(nextBoard, Evaluate(nextBoard)));
                        }
                    }
                }
            }
        }

        return candidates;
    }

    // 优先搜索静态评估较好的局面以提升剪枝效率
    private static void OrderMoves(List<MoveCandidate> candidates, bool isRed)
    {
        candidates.Sort((first, second) => isRed
            ? second.score.CompareTo(first.score)
            : first.score.CompareTo(second.score));
    }

    // 计算单枚棋子的物质分与位置分
    private static int GetPieceScore(PieceType piece, int row, int column)
    {
        bool isRedPiece = IsRedPiece(piece);
        int advancement = isRedPiece ? 9 - row : row;
        int centrality = 4 - Math.Abs(column - 4);

        switch (piece)
        {
            case PieceType.红将:
            case PieceType.黑将:
                return 10000;
            case PieceType.红车:
            case PieceType.黑车:
                return 900 + advancement * 2 + centrality * 3;
            case PieceType.红马:
            case PieceType.黑马:
                return 450 + advancement * 2 + centrality * 8;
            case PieceType.红炮:
            case PieceType.黑炮:
                return 450 + advancement + centrality * 5;
            case PieceType.红象:
            case PieceType.黑象:
            case PieceType.红士:
            case PieceType.黑士:
                return 200;
            case PieceType.红兵:
                return 100 + advancement * 10 + centrality * 4 + (row <= 4 ? 30 : 0);
            case PieceType.黑兵:
                return 100 + advancement * 10 + centrality * 4 + (row >= 5 ? 30 : 0);
            default:
                return 0;
        }
    }

    // 判断棋子是否属于红方
    private static bool IsRedPiece(PieceType piece)
    {
        return piece >= PieceType.红兵 && piece <= PieceType.红将;
    }
}
