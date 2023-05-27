using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CustomToolkit.UnityMVVM;
using TMPro;
using UnityEngine;

public class PlayerLobbyEntryViewModel : ViewModelMonoBehaviour
{
    private PropertyChangedEventArgs m_isActiveProp = new PropertyChangedEventArgs(nameof(IsActive));
    private bool m_isActive = false;

    [Binding]
    public bool IsActive
    {
        get
        {
            return m_isActive;
        }
        set
        {
            if(m_isActive == value)
                return;
            
            m_isActive = value;
            OnPropertyChanged(m_isActiveProp);
        }
    }

    private PropertyChangedEventArgs m_displayNameProp = new PropertyChangedEventArgs(nameof(DisplayName));
    private string m_displayName = "Waiting for player...";

    [Binding]
    public string DisplayName
    {
        get
        {
            return m_displayName;
        }
        set
        {
            m_displayName = value;
            OnPropertyChanged(m_displayNameProp);
        }
    }

    private PropertyChangedEventArgs m_isReadyProp = new PropertyChangedEventArgs(nameof(IsReady));
    private bool m_isReady = false;

    [Binding]
    public bool IsReady
    {
        get
        {
            return m_isReady;
        }
        set
        {
            m_isReady = value;
            OnPropertyChanged(m_isReadyProp);
        }
    }
    
    public void SetPlayerOwner(LobbyRoomPlayer player)
    {
        if (player == null)
        {
            Reset();
            return;
        }

        DisplayName = player.DisplayName;
        IsReady = player.IsReady;
        IsActive = true;
    }

    public void Reset()
    {
        IsActive = false;
        DisplayName = "Waiting for player...";
        IsReady = false;
    }
}
