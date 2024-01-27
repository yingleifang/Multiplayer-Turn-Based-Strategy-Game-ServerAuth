using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGroundTruth : NetworkBehaviour
{
    public static NetworkGroundTruth Instance { get; private set; }

    [SerializeField]
    NetworkVariable<int> currentActivePlayer = new NetworkVariable<int>(0);

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void SetCurrentActivePlayer(int id)
    {
        currentActivePlayer.Value = id;
    }
    public NetworkVariable<int> GetCurrentActivePlayer()
    {
        return currentActivePlayer;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SetCurrentActivePlayer(((int)BaseLogic.PlayerFaction.Player1));
        }
    }
}
