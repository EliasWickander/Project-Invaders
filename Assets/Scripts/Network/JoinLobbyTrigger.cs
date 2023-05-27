using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class JoinLobbyTrigger : MonoBehaviour
{
    public void Trigger()
    {
        if (NetworkManagerCustom.Instance != null)
        {
            NetworkManagerCustom.Instance.JoinLobby();
        }
    }
    
    public void Trigger(string ip)
    {
        if (NetworkManagerCustom.Instance != null)
        {
            NetworkManagerCustom.Instance.JoinLobby(ip);
        }
    }

    public void Trigger(TMP_InputField ip)
    {
        if (NetworkManagerCustom.Instance != null && ip != null)
        {
            NetworkManagerCustom.Instance.JoinLobby(ip.text);
        }
    }
}
