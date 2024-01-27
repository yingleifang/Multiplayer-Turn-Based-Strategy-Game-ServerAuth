using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Properties;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    NetworkVariable<int> playerID = new NetworkVariable<int>(0);
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void SetPlayerID(int id)
    {
        playerID.Value = id;
    }
    public int GetPlayerID()
    {
        return playerID.Value;
    }
}
