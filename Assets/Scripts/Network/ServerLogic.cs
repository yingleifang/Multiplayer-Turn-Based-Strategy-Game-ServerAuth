using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class ServerLogic : MonoBehaviour
{
    public NetworkPlayer player1;
    public NetworkPlayer player2;

    [SerializeField]
    public BasicFaction BasicFactionPrefab;
    [SerializeField]
    GameObject ServerSystems;
    [SerializeField]
    NetworkGroundTruth NetworkGroundTruthPrefab;
    [SerializeField]
    NetworkObject ServerRpc;
    [SerializeField]
    NetworkObject ClientRpc;

    public BasicFaction player1Faction;
    public BasicFaction player2Faction;

    public BasicFaction currentFaction;
    public static ServerLogic Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one ClientLogic!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(ServerRpc).GetComponent<NetworkObject>().Spawn();
        Instantiate(ClientRpc).GetComponent<NetworkObject>().Spawn();
        player1Faction = Instantiate(BasicFactionPrefab);
        player2Faction = Instantiate(BasicFactionPrefab);
        //HexGrid.Instance.LoadMap();
        //HexGrid.Instance.LoadFeature();
        InitPlayers();
        AssignFactionFeatures(player1Faction, player2Faction);
        Instantiate(NetworkGroundTruthPrefab).GetComponent<NetworkObject>().Spawn();
    }

    private void InitPlayers()
    {
        player1 = NetworkManager.Singleton.ConnectedClientsList[0].PlayerObject.GetComponent<NetworkPlayer>();
        player2 = NetworkManager.Singleton.ConnectedClientsList[1].PlayerObject.GetComponent<NetworkPlayer>();
        GameServerRpc.Instance.player1RpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { NetworkManager.Singleton.ConnectedClientsList[0].ClientId }
            }
        };
        GameServerRpc.Instance.player2RpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { NetworkManager.Singleton.ConnectedClientsList[1].ClientId }
            }
        };
        player1.SetPlayerID(((int)BaseLogic.PlayerFaction.Player1));
        player1Faction.SetPlayerId(BaseLogic.PlayerFaction.Player1);
        player2.SetPlayerID(((int)BaseLogic.PlayerFaction.Player2));
        player2Faction.SetPlayerId(BaseLogic.PlayerFaction.Player2);
        currentFaction = player1Faction;
    }
    protected void AssignFactionFeatures(Faction faction1, Faction faction2)
    {
        foreach (var target in HexGrid.Instance.bases)
        {
            if (target.playerID == faction1.playerID)
            {
                faction1.myBases.Add(target);
            }
            else
            {
                faction2.myBases.Add(target);
            }
        }
        foreach (var target in HexGrid.Instance.spawnPoints)
        {
            if (target.playerID == faction1.playerID)
            {
                faction1.myspawnPoints.Add(target);
            }
            else
            {
                faction2.myspawnPoints.Add(target);
            }
        }
    }
}
