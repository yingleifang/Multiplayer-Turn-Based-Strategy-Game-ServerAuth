using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameBootStrap : MonoBehaviour
{
    [SerializeField]
    ServerLogic serverLogic;
    [SerializeField]
    ClientLogic clientLogic;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Instantiate(serverLogic);
        }
        else
        {
            Instantiate(clientLogic);
        }
    }
}