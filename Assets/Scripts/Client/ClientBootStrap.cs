using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_SERVER
public class ClientBootStrap : MonoBehaviour
{
    [SerializeField]
    ClientLogic clientLogic;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(clientLogic);
    }
}
#endif