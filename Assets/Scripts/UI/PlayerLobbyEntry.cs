using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerLobbyEntry : MonoBehaviour
{
    [SerializeField] 
    private TMP_Text m_displayName;

    [SerializeField] 
    private GameObject m_isReadyText;
    
    [SerializeField] 
    private GameObject m_isNotReadyText;

    public void SetPlayerOwner(LobbyRoomPlayer player)
    {
        if (player == null)
        {
            Reset();
            return;
        }

        m_displayName.text = player.DisplayName;
        
        m_isNotReadyText.SetActive(true);
        m_isReadyText.SetActive(false);
    }

    public void Reset()
    {
        m_displayName.text = "Waiting for player...";
        m_isReadyText.SetActive(false);
        m_isNotReadyText.SetActive(false);
    }
}
