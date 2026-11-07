using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    [Header("空棋子")]
    [SerializeField]
    private Sprite kong;
    [Header("红兵")]
    [SerializeField]
    private Sprite redBing;
    [Header("红车")]
    [SerializeField]
    private Sprite redChe;
    [Header("红马")]
    [SerializeField]
    private Sprite redMa;
    [Header("红炮")]
    [SerializeField]
    private Sprite redPao;
    [Header("红象")]
    [SerializeField]
    private Sprite redXiang;
    [Header("红士")]
    [SerializeField]
    private Sprite redShi;
    [Header("红将")]
    [SerializeField]
    private Sprite redJiang;
    [Header("黑兵")]
    [SerializeField]
    private Sprite blackBing;
    [Header("黑车")]
    [SerializeField]
    private Sprite blackChe;
    [Header("黑马")]
    [SerializeField]
    private Sprite blackMa;
    [Header("黑炮")]
    [SerializeField]
    private Sprite blackPao;
    [Header("黑象")]
    [SerializeField]
    private Sprite blackXiang;
    [Header("黑士")]
    [SerializeField]
    private Sprite blackShi;
    [Header("黑将")]
    [SerializeField]
    private Sprite blackJiang;

    // 根据棋子类型返回对应的精灵
    public Sprite GetPiece(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.空:
                return kong;
            case PieceType.红兵:
                return redBing;
            case PieceType.红车:
                return redChe;
            case PieceType.红马:
                return redMa;
            case PieceType.红炮:
                return redPao;
            case PieceType.红象:
                return redXiang;
            case PieceType.红士:
                return redShi;
            case PieceType.红将:
                return redJiang;
            case PieceType.黑兵:
                return blackBing;
            case PieceType.黑车:
                return blackChe;
            case PieceType.黑马:
                return blackMa;
            case PieceType.黑炮:
                return blackPao;
            case PieceType.黑象:
                return blackXiang;
            case PieceType.黑士:
                return blackShi;
            case PieceType.黑将:
                return blackJiang;
            default:
                return null;
        }
    }
}
