using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public Point position { get; private set; }

    private Sprite sprite;
    public SpriteRenderer spriteRenderer { get; private set; }

    private ISquareController squareController;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }
    public void Create(Point point, PieceType pieceType, ISquareController squareController)
    {
        this.position = point;
        this.sprite = SpriteManager.Instance.GetPiece(pieceType);
        this.squareController = squareController;

    }
    public void Change(PieceType pieceType)
    {
        spriteRenderer.sprite = SpriteManager.Instance.GetPiece(pieceType);
    }
    // 点击棋格时将坐标交给棋盘控制器处理
    public void OnMouseDown()
    {
        if (squareController != null)
            squareController.GetPoint(position);
    }
}
