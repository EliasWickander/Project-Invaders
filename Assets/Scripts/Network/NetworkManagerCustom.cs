using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerCustom : NetworkManager
{
    public static NetworkManagerCustom Instance => singleton as NetworkManagerCustom;
    
    [Scene]
    [SerializeField]
    private string m_lobbyScene;

    [SerializeField]
    private int m_minPlayers = 1;
    
    [Header("Lobby Room")] 
    [SerializeField]
    private LobbyRoomPlayer m_roomPlayerPrefab;

    public OnStartHostAttemptEvent m_onStartHostAttemptEvent;
    public OnStartHostEvent m_onStartHostEvent;
    public OnClientConnectionAttemptEvent m_onClientConnectionAttemptEvent;
    public OnClientConnectedEvent m_onClientConnectedEvent;
    public OnClientDisconnectedEvent m_onClientDisconnectedEvent;
    public OnClientErrorEvent m_onClientErrorEvent;
    public OnServerConnectedEvent m_onServerConnectedEvent;
    public OnServerAddPlayerEvent m_onServerAddPlayerEvent;
    public OnServerDisconnectedEvent m_onServerDisconnectedEvent;
    public OnPlayerJoinedLobbyEvent m_onPlayerJoinedLobbyEvent;

    public List<LobbyRoomPlayer> RoomPlayers { get; } = new List<LobbyRoomPlayer>();

    public override void OnStartServer()
    {
        base.OnStartServer();

        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        GameObject[] spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");
        
        foreach(GameObject prefab in spawnablePrefabs)
            NetworkClient.RegisterPrefab(prefab);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        if(m_onClientConnectedEvent)
            m_onClientConnectedEvent.Raise();;
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        if(m_onClientDisconnectedEvent)
            m_onClientDisconnectedEvent.Raise(new OnClientDisconnectedEventData() {m_wasSelf = true, m_isLocalScope = true});
    }

    public override void OnClientError(TransportError error, string reason)
    {
        base.OnClientError(error, reason);

        Debug.Log(error + " " + reason);
        if(m_onClientErrorEvent)
            m_onClientErrorEvent.Raise(error);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if (SceneManager.GetActiveScene().path != m_lobbyScene)
        {
            conn.Disconnect();
            return;
        }

        if(m_onServerConnectedEvent != null)
            m_onServerConnectedEvent.Raise(new OnServerConnectedEventData() {m_connection = conn});
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if(conn.identity == null)
            return;

        LobbyRoomPlayer player = conn.identity.GetComponent<LobbyRoomPlayer>();

        int clientIndex = player.PlayerIndex;
        
        RoomPlayers.Remove(player);
        
        if(m_onServerDisconnectedEvent != null)
            m_onServerDisconnectedEvent.Raise(new OnServerDisconnectedEventData() {m_connection = conn});
        
        NotifyPlayersReadyState();
        NotifyPlayersClientDisconnected(clientIndex);
        
        base.OnServerDisconnect(conn);
    }
    
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().path == m_lobbyScene)
        {
            LobbyRoomPlayer roomPlayerInstance = Instantiate(m_roomPlayerPrefab);

            //If this instance is first player
            if (RoomPlayers.Count == 0)
            {
                roomPlayerInstance.IsLeader = true;
            }
            
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
            
            if(m_onServerAddPlayerEvent != null)
                m_onServerAddPlayerEvent.Raise(new OnServerAddPlayerEventData() {m_connection = conn});
        }
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        
        if(m_onStartHostEvent)
            m_onStartHostEvent.Raise();
    }

    public void NotifyPlayersReadyState()
    {
        foreach(LobbyRoomPlayer player in RoomPlayers)
            player.HandleReadyToStart(IsReadyToStart());
    }

    public void NotifyPlayersClientDisconnected(int clientIndex)
    {
        foreach(LobbyRoomPlayer player in RoomPlayers)
            player.HandleClientDisconnected(clientIndex);
    }

    private bool IsReadyToStart()
    {
        if(numPlayers < m_minPlayers)
            return false;

        foreach (LobbyRoomPlayer player in RoomPlayers)
        {
            if (!player.IsReady)
                return false;
        }

        return true;
    }
    
    public void HostLobby()
    {
        if(NetworkServer.active || NetworkClient.active)
            return;

        if(m_onStartHostAttemptEvent)
            m_onStartHostAttemptEvent.Raise();

        StartCoroutine(HostLobbyAsync());
    }

    public IEnumerator HostLobbyAsync()
    {
        yield return new WaitForSeconds(0.1f);

        StartHost();
    }
    
    public void JoinLobby()
    {
        if(NetworkClient.active)
            return;
        
        m_onClientConnectionAttemptEvent.Raise();
        
        StartCoroutine(JoinLobbyAsync());
    }

    public void JoinLobby(string ip)
    {
        if(NetworkClient.active)
            return;

        networkAddress = ip;
        
        if(m_onClientConnectionAttemptEvent)
            m_onClientConnectionAttemptEvent.Raise();

        StartCoroutine(JoinLobbyAsync());
    }
    
    //Make sure to call StartClient after one frame so event has time to trigger
    private IEnumerator JoinLobbyAsync()
    {
        yield return new WaitForSeconds(0.1f);

        StartClient();
    }

    public void OnPlayerJoinedLobby(LobbyRoomPlayer player)
    {
        if(player == null)
            return;
        
        RoomPlayers.Add(player);

        if(m_onPlayerJoinedLobbyEvent != null)
            m_onPlayerJoinedLobbyEvent.Raise(new OnPlayerJoinedLobbyEventData() {m_player = player});
    }

    public void DisconnectClient()
    {
        StopClient();
    }
}