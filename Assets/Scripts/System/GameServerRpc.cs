using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameServerRpc : NetworkBehaviour
{
	public static GameServerRpc Instance { get; private set; }

	public bool deleteMode = false;

    public ClientRpcParams player1RpcParams;

    public ClientRpcParams player2RpcParams;
    ClientRpcParams GetClientRpcParams(ulong clientId)
    {
        if (clientId == player1RpcParams.Send.TargetClientIds[0])
        {
            return player2RpcParams;
        }
        else
        {
            return player1RpcParams;
        }
    }
    private void Awake()
    {
		if (Instance != null)
		{
			Debug.LogError("There's more than one GameManagerServer!");
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

    [ServerRpc(RequireOwnership = false)]
    public void RequestUnitInstantiationServerRpc(int x, int z, int cardId, ServerRpcParams serverRpcParams = default)
    {
        GameClientRpc.Instance.AddFeatureClientRpc(x, z, cardId, GetClientRpcParams(serverRpcParams.Receive.SenderClientId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUnitMovementServerRpc(int endX, int endZ, int unitIndex, ServerRpcParams serverRpcParams = default)
    {
        GameClientRpc.Instance.SyncUnitMovementClientRpc(endX, endZ, unitIndex, GetClientRpcParams(serverRpcParams.Receive.SenderClientId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUnitAttackServerRpc(int unitIndex, int targetType, int targetIndex, ServerRpcParams serverRpcParams = default)
    {
        GameClientRpc.Instance.SyncUnitAttackClientRpc(unitIndex, targetType, targetIndex, GetClientRpcParams(serverRpcParams.Receive.SenderClientId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerTurnEndedServerRpc()
    {
        NetworkGroundTruth.Instance.SetCurrentActivePlayer(NetworkGroundTruth.Instance.GetCurrentActivePlayer().Value == 1 ? 2 : 1);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestWeaponEquippingServerRpc(int x, int z, int cardId, ServerRpcParams serverRpcParams = default)
    {
        GameClientRpc.Instance.EquipWeaponClientRpc(x, z, cardId, GetClientRpcParams(serverRpcParams.Receive.SenderClientId));
    }


    //[ServerRpc(RequireOwnership = false)]
    //public void RequestSyncMapLocationServerRpc(ulong clientId, int x, int z, ulong objId)
    //{
    //    if (clientId == NetworkManager.Singleton.ConnectedClients[0].ClientId)
    //    {
    //        GameClientRpc.Instance.SyncMapLocationClientRpc(x, z, objId, player1RpcParams);
    //    }
    //    else
    //    {
    //        GameClientRpc.Instance.SyncMapLocationClientRpc(x, z, objId, player2RpcParams);
    //    }
    //}

}
