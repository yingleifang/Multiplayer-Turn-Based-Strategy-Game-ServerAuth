using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ClientStartGameUIManager : MonoBehaviour
{
#if !UNITY_SERVER
    [SerializeField]
    private Canvas _mainCanvas;

    [SerializeField]
    private SignInScreen _signInScreenPrefab;

    [SerializeField]
    private SignUpScreen _signUpScreenPrefab;

    [SerializeField]
    private LobbyManager _lobbyManagerPrefab;

    [SerializeField]
    private LobbyMessageUI _connectingUI;

    [SerializeField]
    private ConfirmationCodeScreen _confirmationScreenPrefab;

    [SerializeField]
    private ConfirmationSuccessScreen _confirmationSuccessScreenPrefab;

    private SignInScreen _signInScreen;
    private SignUpScreen _signUpScreen;
    private ConfirmationCodeScreen _confirmationScreen;
    private ConfirmationSuccessScreen _confirmationSuccessScreen;

    public static ClientStartGameUIManager Instance { get; private set; }
    private void Awake()
    {
        // prevent the game going to sleep when the window loses focus
        Application.runInBackground = true;
        // Just 60 frames per second is enough
        Application.targetFrameRate = 60;
        Instance = this;
    }

    private void Start()
    {
        ShowSignIn();
    }

    private void ShowSignIn()
    {
        _signInScreen = _signInScreen != null ? _signInScreen : Instantiate(_signInScreenPrefab, _mainCanvas.transform);
        _signInScreen.SetSubmitAction(SignIn);
        _signInScreen.SetShowSignUpAction(() =>
        {
            _signInScreen.Hide();
            ShowSignUp();
        });


        _signInScreen.Show();
    }

    private void ShowSignUp()
    {
        _signUpScreen = _signUpScreen != null ? _signUpScreen : Instantiate(_signUpScreenPrefab, _mainCanvas.transform);
        _signUpScreen.SetSubmitAction(SignUp);
        _signUpScreen.Show();
    }

    private void ShowConfirmSignUp(string email)
    {
        _confirmationScreen = _confirmationScreen != null ? _confirmationScreen : Instantiate(_confirmationScreenPrefab, _mainCanvas.transform);
        _confirmationScreen.SetSubmitAction(code => ConfirmSignUp(email, code));
        _confirmationScreen.Show();
    }

    private void SignIn(string email, string password)
    {
        _signInScreen.SetInteractable(false);
        _signInScreen.SetResultText(string.Empty);
        //SignInResponse response = _gameLift.SignIn(email, password);
        _signInScreen.SetInteractable(true);

        //if (!response.Success && response.ErrorCode != ErrorCode.UserNotConfirmed)
        //{
        //    _logger.Write($"Sign-in error {response.ErrorCode}: {response.ErrorMessage}", LogType.Error);
        //    _signInScreen.SetResultText(response.ErrorMessage ?? Strings.ErrorUnknown);
        //    return;
        //}

        //_signInScreen.Hide();

        //if (!response.Success && response.ErrorCode == ErrorCode.UserNotConfirmed)
        //{
        //    ShowConfirmSignUp(email);
        //    return;
        //}

        _signInScreen.SetInteractable(false);
        StartLobby();
    }

    private void SignUp(string email, string password)
    {
        _signUpScreen.SetInteractable(false);
        _signUpScreen.SetResultText(string.Empty);
        //SignUpResponse response = _gameLift.SignUp(email, password);
        _signUpScreen.SetInteractable(true);

        //if (!response.Success)
        //{
        //    _logger.Write($"Sign-up error {response.ErrorCode}: {response.ErrorMessage}", LogType.Error);
        //    _signUpScreen.SetResultText(response.ErrorMessage ?? Strings.ErrorUnknown);
        //    return;
        //}

        _signUpScreen.Hide();
        ShowConfirmSignUp(email);
    }

    private void ConfirmSignUp(string email, string code)
    {
        _confirmationScreen.SetInteractable(false);
        _confirmationScreen.SetResultText(string.Empty);
        //ConfirmSignUpResponse response = _gameLift.ConfirmSignUp(email, code);
        _confirmationScreen.SetInteractable(true);

        //if (!response.Success)
        //{
        //    _logger.Write($"Confirmation error {response.ErrorCode}: {response.ErrorMessage}", LogType.Error);
        //    _confirmationScreen.SetResultText(response.ErrorMessage ?? Strings.ErrorUnknown);
        //    return;
        //}

        _confirmationScreen.Hide();
        ShowConfirmationSuccess();
    }

    private void ShowConfirmationSuccess()
    {
        _confirmationSuccessScreen = _confirmationSuccessScreen != null
            ? _confirmationSuccessScreen
            : Instantiate(_confirmationSuccessScreenPrefab, _mainCanvas.transform);
        _confirmationSuccessScreen.SetSubmitAction(ConfirmSuccess);
        _confirmationSuccessScreen.Show();
        _confirmationSuccessScreen.SetResultText("Your account is confirmed.");
    }

    private void ConfirmSuccess()
    {
        _confirmationSuccessScreen.Hide();
        ShowSignIn();
    }
    public void StartLobby()
    {
        Debug.Log("Start Lobby");
        _signInScreen.Hide();
        Instantiate(_lobbyManagerPrefab, _mainCanvas.transform);
        Instantiate(_connectingUI, _mainCanvas.transform);
    }
#endif
}