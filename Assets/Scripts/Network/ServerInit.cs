
#if UNITY_SERVER
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Multiplay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.IO;
using System.Threading.Tasks;

public class ServerInit : MonoBehaviour
{
    IServerQueryHandler serverQueryHandler;
    bool alreadyAutoAllocated = false;
    float alreadyAutoAllocateTimer = 9999999f;

    private const ushort k_DefaultMaxPlayers = 2;
    private const string k_DefaultServerName = "MyServerExample";
    private const string k_DefaultGameType = "MyGameType";
    private const string k_DefaultBuildId = "MyBuildId";
    private const string k_DefaultMap = "MyMap";
    async void Start()
    {
#if !UNITY_EDITOR
        InitializeUnityAuthentication();
        await MultiplayService.Instance.ReadyServerForPlayersAsync();
#else       
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnClientConnectedCallback += (client) =>
        {
            Debug.Log($"Client {client} has connected to the server");
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (client) =>
        {
            Debug.Log($"Client {client} has disconnected from the server");
        };
#endif
    }

    async void Update()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
#if !UNITY_EDITOR
        alreadyAutoAllocateTimer -= Time.deltaTime;
        if (alreadyAutoAllocateTimer <= 0f)
        {
            alreadyAutoAllocateTimer = 999f;
            MultiplayEventCallbacks_Allocate(null);
        }
        if (serverQueryHandler != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                serverQueryHandler.CurrentPlayers = (ushort)NetworkManager.Singleton.ConnectedClientsList.Count;
            }
            serverQueryHandler.UpdateServerCheck();
        }
#endif
        if (NetworkManager.Singleton.IsServer && NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            Debug.Log("Load MultiPlayer Scene");
            NetworkManager.Singleton.SceneManager.LoadScene("MultiPlayer", LoadSceneMode.Single);
#if !UNITY_EDITOR
            await MultiplayService.Instance.UnreadyServerAsync();
#endif
            gameObject.SetActive(false);
        }
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initOptions = new InitializationOptions();
            await UnityServices.InitializeAsync(initOptions);
            MultiplayEventCallbacks multiplayEventCallbacks = new MultiplayEventCallbacks();
            multiplayEventCallbacks.Allocate += MultiplayEventCallbacks_Allocate;
            multiplayEventCallbacks.Deallocate += MultiplayEventCallbacks_Deallocate;
            multiplayEventCallbacks.Error += MultiplayEventCallbacks_Error;
            multiplayEventCallbacks.SubscriptionStateChanged += MultiplayEventCallbacks_SubscriptionStateChanged;
            IServerEvents serverEvents = await MultiplayService.Instance.SubscribeToServerEventsAsync(multiplayEventCallbacks);
            serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(k_DefaultMaxPlayers, k_DefaultServerName, k_DefaultGameType, k_DefaultBuildId, k_DefaultMap);
            var serverConfig = MultiplayService.Instance.ServerConfig;
            if (serverConfig.AllocationId != "")
            {
                MultiplayEventCallbacks_Allocate(new MultiplayAllocation("", serverConfig.ServerId, serverConfig.AllocationId));
            }
        }
    }

    private void MultiplayEventCallbacks_Error(MultiplayError error)
    {
        Debug.Log("MultiplayEventCallbacks_Error");
    }

    private void MultiplayEventCallbacks_Deallocate(MultiplayDeallocation deallocation)
    {
        Debug.Log("MultiplayEventCallbacks_Deallocate");
    }

    private void MultiplayEventCallbacks_SubscriptionStateChanged(MultiplayServerSubscriptionState state)
    {
        Debug.Log("MultiplayEventCallbacks_SubscriptionStateChanged");
    }

    void MultiplayEventCallbacks_Allocate(MultiplayAllocation obj)
    {
        Debug.Log("Allocate");
        if (alreadyAutoAllocated)
        {
            Debug.Log("Already auto allocated!");
            return;
        }
        alreadyAutoAllocated = true;
        var serverConfig = MultiplayService.Instance.ServerConfig;
        string ipv4Address = "0.0.0.0";
        ushort port = serverConfig.Port;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipv4Address, port, "0.0.0.0");
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnClientConnectedCallback += (client) =>
        {
            Debug.Log($"Client {client} has connected to the server");
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (client) =>
        {
            Debug.Log($"Client {client} has disconnected from the server");
        };
    }
}
#endif