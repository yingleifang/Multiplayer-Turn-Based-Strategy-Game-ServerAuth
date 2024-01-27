using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
#if !UNITY_SERVER
    [SerializeField] private TextMeshProUGUI messageText;
    private void Instance_OnMatchingStart(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Start()
    {
        Hide();
        LobbyManager.Instance.OnMatchingStart += Instance_OnMatchingStart;
    }

    public void Show()
    {
        Debug.Log("Show");
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
#endif
}