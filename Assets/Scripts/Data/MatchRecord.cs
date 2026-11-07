using System.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

// 对局记录的保存与读取工具
public static class MatchRecord
{
    // 序列化后的整场对局数据
    [Serializable]
    private class MatchData
    {
        // 对局中的所有棋盘状态
        public List<BoardData> boards = new List<BoardData>();
    }

    // 可序列化的棋盘状态
    [Serializable]
    private class BoardData
    {
        // 棋盘行数
        public int rows;

        // 棋盘列数
        public int columns;

        // 按行展开的棋子数据
        public int[] pieces;

        // 当前是否轮到红方
        public bool isRed;
    }

    // 保存目录名称
    private const string SaveFolderName = "save";

    // 将对局棋盘序列化并保存到以当前时间命名的文件
    public static void WriterMatch(List<Board> boards)
    {
        if (boards == null)
        {
            Debug.LogError("保存对局失败：棋盘记录为空。");
            return;
        }

        // 对局文件的保存目录
        string directory = Path.Combine(Application.persistentDataPath, SaveFolderName);

        try
        {
            Directory.CreateDirectory(directory);

            // 待写入文件的对局数据
            MatchData matchData = new MatchData();
            foreach (Board board in boards)
            {
                if (board == null || board.board == null)
                {
                    continue;
                }

                // 当前棋盘的行数
                int rows = board.board.GetLength(0);
                // 当前棋盘的列数
                int columns = board.board.GetLength(1);
                // 展开后的棋子数据
                int[] pieces = new int[rows * columns];
                for (int row = 0; row < rows; row++)
                {
                    for (int column = 0; column < columns; column++)
                    {
                        pieces[row * columns + column] = (int)board.board[row, column];
                    }
                }

                matchData.boards.Add(new BoardData
                {
                    rows = rows,
                    columns = columns,
                    pieces = pieces,
                    isRed = board.isRed
                });
            }

            // 使用年月日和时分秒毫秒组成合法文件名
            string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff", CultureInfo.InvariantCulture) + ".json";
            // 对局文件的完整路径
            string filePath = Path.Combine(directory, fileName);
            // 序列化后的对局文本
            string json = JsonUtility.ToJson(matchData);
            File.WriteAllText(filePath, json, new UTF8Encoding(false));
            Debug.Log($"保存对局成功！文件路径：{filePath}");
        }
        catch (Exception exception)
        {
            Debug.LogError("保存对局失败：" + exception.Message);
        }
    }

    // 获取保存目录中所有对局文件的名称（不含扩展名）
    public static List<string> ReadLibrary
    {
        get
        {
            // 对局文件列表
            List<string> names = new List<string>();
            // 对局文件的保存目录
            string directory = Path.Combine(Application.persistentDataPath, SaveFolderName);
            if (!Directory.Exists(directory))
            {
                return names;
            }

            try
            {
                // 保存目录中的 JSON 文件路径
                string[] files = Directory.GetFiles(directory, "*.json");
                foreach (string file in files)
                {
                    names.Add(Path.GetFileNameWithoutExtension(file));
                }
                names.Sort(StringComparer.Ordinal);
            }
            catch (Exception exception)
            {
                Debug.LogError("读取对局列表失败：" + exception.Message);
            }

            return names;
        }
    }

    // 根据文件名读取并还原一场对局
    public static List<Board> ReadMatch(string name)
    {
        // 读取到的棋盘状态列表
        List<Board> boards = new List<Board>();
        if (string.IsNullOrWhiteSpace(name))
        {
            return boards;
        }

        // 对局文件的保存目录
        string directory = Path.Combine(Application.persistentDataPath, SaveFolderName);
        // 仅使用文件名，避免传入路径访问保存目录以外的文件
        string fileName = Path.GetFileNameWithoutExtension(name) + ".json";
        // 对局文件的完整路径
        string filePath = Path.Combine(directory, fileName);
        if (!File.Exists(filePath))
        {
            return boards;
        }

        try
        {
            // 文件中的序列化对局数据
            MatchData matchData = JsonUtility.FromJson<MatchData>(File.ReadAllText(filePath, Encoding.UTF8));
            if (matchData == null || matchData.boards == null)
            {
                return boards;
            }

            foreach (BoardData boardData in matchData.boards)
            {
                if (boardData == null || boardData.rows <= 0 || boardData.columns <= 0
                    || boardData.pieces == null || boardData.pieces.Length != boardData.rows * boardData.columns)
                {
                    continue;
                }

                // 还原后的二维棋子数组
                PieceType[,] pieces = new PieceType[boardData.rows, boardData.columns];
                for (int row = 0; row < boardData.rows; row++)
                {
                    for (int column = 0; column < boardData.columns; column++)
                    {
                        pieces[row, column] = (PieceType)boardData.pieces[row * boardData.columns + column];
                    }
                }

                boards.Add(new Board(pieces, boardData.isRed));
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("读取对局失败：" + exception.Message);
        }

        return boards;
    }
}
