using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CustomToolkit.UnityMVVM;
using UnityEngine;

public class PreGameScreenViewModel : ViewModelMonoBehaviour
{
    private PreGamePlayer m_localPlayer;

    private PropertyChangedEventArgs m_selectedElementProp = new PropertyChangedEventArgs(nameof(SelectedElement));
    private SelectableElementViewModel m_selectedElement = null;

    [Binding]
    public SelectableElementViewModel SelectedElement
    {
        get
        {
            return m_selectedElement;
        }
        set
        {
            m_selectedElement = value;
            OnPropertyChanged(m_selectedElementProp);
        }
    }
    
    private PropertyChangedEventArgs m_hasLockedInElementProp = new PropertyChangedEventArgs(nameof(HasLockedInElement));
    private bool m_hasLockedInElement = false;

    [Binding]
    public bool HasLockedInElement
    {
        get
        {
            return m_hasLockedInElement;
        }
        set
        {
            m_hasLockedInElement = value;
            OnPropertyChanged(m_hasLockedInElementProp);
        }
    }

    public void OnPlayerJoinedPreGame(OnPlayerJoinedPreGameEventData data)
    {
        if(data.m_player == null)
            return;

        if (data.m_player.isOwned)
        {
            m_localPlayer = data.m_player;
        }

        Debug.LogError($"Player joined\n" +
                       $"Display Name: {data.m_player.DisplayName}\n" +
                       $"Connection to client: {data.m_player.connectionToClient}\n" +
                       $"Connection to server: {data.m_player.connectionToServer}");
    }

    [Binding]
    public void LockInSelectedElement()
    {
        if(SelectedElement == null)
            return;
        
        HasLockedInElement = true;
        
        m_localPlayer.SelectElement();
    }
}
