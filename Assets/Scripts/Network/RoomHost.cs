using Unity.Netcode;
using UnityEngine;

// 负责网络玩家阵营分配和走子同步
public class RoomHost : NetworkBehaviour
{
    // 玩家阵营，零表示红方，一表示黑方，负数表示未分配
    private readonly NetworkVariable<int> playerSide = new NetworkVariable<int>(
        -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // 订阅玩家阵营更新并分配服务器侧阵营
    public override void OnNetworkSpawn()
    {
        playerSide.OnValueChanged += OnPlayerSideChanged;
        if (IsServer)
        {
            AssignPlayerSide();
        }

        ApplyLocalPlayerSide();
    }

    // 取消玩家阵营更新订阅
    public override void OnNetworkDespawn()
    {
        playerSide.OnValueChanged -= OnPlayerSideChanged;
    }

    // 客户端申请走子，由服务器校验后广播给对局双方
    public bool RequestMove(Point from, Point destination)
    {
        if (!IsSpawned || !IsOwner || playerSide.Value < 0
            || NetworkManager == null || !NetworkManager.IsConnectedClient)
        {
            return false;
        }

        RequestMoveServerRpc(from.x, from.y, destination.x, destination.y);
        return true;
    }

    // 为房主随机分配阵营，为加入者分配另一方
    private void AssignPlayerSide()
    {
        if (NetworkManager.ConnectedClientsIds.Count > 2)
        {
            Debug.LogWarning("房间仅支持两名玩家。", this);
            return;
        }

        if (OwnerClientId == NetworkManager.ServerClientId)
        {
            playerSide.Value = Random.Range(0, 2);
            return;
        }

        // 获取房主玩家对象及其已分配阵营
        if (NetworkManager.ConnectedClients.TryGetValue(NetworkManager.ServerClientId, out NetworkClient hostClient)
            && hostClient.PlayerObject != null)
        {
            RoomHost hostPlayer = hostClient.PlayerObject.GetComponent<RoomHost>();
            if (hostPlayer != null && hostPlayer.playerSide.Value >= 0)
            {
                playerSide.Value = 1 - hostPlayer.playerSide.Value;
                return;
            }
        }

        Debug.LogError("房主阵营尚未分配，无法为加入者分配阵营。", this);
    }

    // 处理阵营同步并更新本地棋盘控制器
    private void OnPlayerSideChanged(int previousSide, int currentSide)
    {
        ApplyLocalPlayerSide();
    }

    // 将本地拥有者的阵营交给场景棋盘控制器
    private void ApplyLocalPlayerSide()
    {
        if (!IsOwner || playerSide.Value < 0)
        {
            return;
        }

        BoardController boardController = FindObjectOfType<BoardController>();
        if (boardController == null)
        {
            Debug.LogError("场景中未找到 BoardController。", this);
            return;
        }

        boardController.SetNetworkPlayer(this, playerSide.Value == 0);
    }

    // 校验走法并在服务器棋盘应用后广播坐标
    [ServerRpc]
    private void RequestMoveServerRpc(int fromRow, int fromColumn, int toRow, int toColumn,
        ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId || playerSide.Value < 0)
        {
            return;
        }

        // 获取场景棋盘并按玩家阵营校验、执行走法
        BoardController boardController = FindObjectOfType<BoardController>();
        bool moverIsRed = playerSide.Value == 0;
        Point from = new Point(fromRow, fromColumn);
        Point destination = new Point(toRow, toColumn);
        if (boardController == null || !boardController.ApplyNetworkMove(from, destination, moverIsRed))
        {
            return;
        }

        BroadcastMoveClientRpc(fromRow, fromColumn, toRow, toColumn, moverIsRed);
    }

    // 将已校验的走法广播到其他客户端棋盘
    [ClientRpc]
    private void BroadcastMoveClientRpc(int fromRow, int fromColumn, int toRow, int toColumn, bool moverIsRed)
    {
        if (IsServer)
        {
            return;
        }

        // 在客户端场景棋盘应用服务器广播的走法
        BoardController boardController = FindObjectOfType<BoardController>();
        Point from = new Point(fromRow, fromColumn);
        Point destination = new Point(toRow, toColumn);
        if (boardController == null || !boardController.ApplyNetworkMove(from, destination, moverIsRed))
        {
            Debug.LogError("客户端棋盘未能应用服务器广播的走法。", this);
        }
    }
}
