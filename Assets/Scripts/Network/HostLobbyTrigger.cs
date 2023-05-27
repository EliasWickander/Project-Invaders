using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostLobbyTrigger : MonoBehaviour
{
    public void Trigger()
    {
        if (NetworkManagerCustom.Instance != null)
        {
            NetworkManagerCustom.Instance.HostLobby();
        }
    }
}
