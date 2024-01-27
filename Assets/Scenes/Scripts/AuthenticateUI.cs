using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if !UNITY_SERVER
public class AuthenticateUI : MonoBehaviour {

    public static AuthenticateUI Instance { get; private set; }

    public string playerName;

    [SerializeField] private Button authenticateButton;
    private string _gameSceneName = "Lobby";

    private void Awake() {
        authenticateButton.onClick.AddListener(() => {
            Authenticate();
        });
        Instance = this;
    }
    public async void Authenticate()
    {
        playerName = "GuestPlayer" + Random.Range(1, 100000);
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        ClientStartGameUIManager.Instance.StartLobby();
    }

}
#endif