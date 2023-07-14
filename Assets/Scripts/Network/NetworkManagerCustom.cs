using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlayerProfile
{
    public string m_displayName = "Placeholder";
}

public struct CreateLobbyPlayerMessage : NetworkMessage
{

}

public class NetworkManagerCustom : NetworkManager
{

    public static NetworkManagerCustom Instance => singleton as NetworkManagerCustom;
    
    [SerializeField]
    private int m_minPlayers = 1;

    public int MinPlayers => m_minPlayers;
    
    [Header("Scenes")]
    [Scene]
    [SerializeField]
    private string m_lobbyScene;

    [Scene] 
    [SerializeField] 
    private string m_gameScene;

    [Header("Lobby Room")] 
    [SerializeField]
    private LobbyRoomPlayer m_lobbyPlayerPrefab;

    [Header("Game")] 
    [SerializeField] 
    private PreGamePlayer m_preGamePlayerPrefab;
    
    [SerializeField] 
    private Player m_gamePlayerPrefab;
    
    [Header("Network Events")]
    public OnStartHostAttemptEvent m_onStartHostAttemptEvent;
    public OnStartHostEvent m_onStartHostEvent;
    public OnClientConnectionAttemptEvent m_onClientConnectionAttemptEvent;
    public OnClientConnectedEvent m_onClientConnectedEvent;
    public OnClientDisconnectedEvent m_onClientDisconnectedEvent;
    public OnClientErrorEvent m_onClientErrorEvent;
    public OnServerConnectedEvent m_onServerConnectedEvent;
    public OnServerAddPlayerEvent m_onServerAddPlayerEvent;
    public OnServerDisconnectedEvent m_onServerDisconnectedEvent;
    
    [Header("Lobby Events")]
    public OnPlayerJoinedLobbyEvent m_onPlayerJoinedLobbyEvent;

    [Header("Pre Game Events")] 
    public OnPlayerJoinedPreGameEvent m_onPlayerJoinedPreGameEvent;
    
    public List<LobbyRoomPlayer> RoomPlayers { get; } = new List<LobbyRoomPlayer>();
    public List<Player> GamePlayers { get; } = new List<Player>();
    public List<PreGamePlayer> PreGamePlayers { get; } = new List<PreGamePlayer>();

    private Dictionary<NetworkConnectionToClient, PlayerProfile> m_playerProfiles = new Dictionary<NetworkConnectionToClient, PlayerProfile>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        NetworkServer.RegisterHandler<CreateLobbyPlayerMessage>(OnCreateLobbyPlayer);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        
        CreateLobbyPlayerMessage createLobbyPlayerMsg = new CreateLobbyPlayerMessage();
        NetworkClient.Send(createLobbyPlayerMsg);
        
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

        if (player != null)
        {
            int clientIndex = player.PlayerIndex;
        
            RoomPlayers.Remove(player);
        
            if(m_onServerDisconnectedEvent != null)
                m_onServerDisconnectedEvent.Raise(new OnServerDisconnectedEventData() {m_connection = conn});

            NotifyPlayersClientDisconnected(clientIndex);   
        }

        base.OnServerDisconnect(conn);
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

    public void NotifyPlayersClientDisconnected(int clientIndex)
    {
        foreach(LobbyRoomPlayer player in RoomPlayers)
            player.HandleClientDisconnected(clientIndex);
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

    public void OnPlayerJoinedPreGame(PreGamePlayer player)
    {
        if(player == null)
            return;
        
        PreGamePlayers.Add(player);

        if(m_onPlayerJoinedPreGameEvent != null)
            m_onPlayerJoinedPreGameEvent.Raise(new OnPlayerJoinedPreGameEventData() {m_player = player});
    }
    
    public void DisconnectClient()
    {
        StopClient();
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        NetworkClient.Ready();
        
        foreach (KeyValuePair<NetworkConnectionToClient, PlayerProfile> playerProfilePair in m_playerProfiles)
        {
            NetworkConnectionToClient conn = playerProfilePair.Key;
            PlayerProfile playerProfile = playerProfilePair.Value;

            SceneLoadedForPlayer(conn, playerProfile);
        }
    }

    private void SceneLoadedForPlayer(NetworkConnectionToClient connection, PlayerProfile playerProfile)
    {
        if (Utils.IsSceneActive(m_gameScene))
        {
            PreGamePlayer preGamePlayerInstance = Instantiate(m_preGamePlayerPrefab);
            preGamePlayerInstance.SetDisplayName(playerProfile.m_displayName);

            //Replace room player with pre-game player
            NetworkServer.ReplacePlayerForConnection(connection, preGamePlayerInstance.gameObject, true);
        }
    }
    
    public void StartGame()
    {
        //If starting from lobby, change scene to game on server side
        if (SceneManager.GetActiveScene().path == m_lobbyScene)
        {
            ServerChangeScene(m_gameScene);
        }
    }

    private void OnCreateLobbyPlayer(NetworkConnectionToClient conn, CreateLobbyPlayerMessage message)
    {
        if (m_lobbyPlayerPrefab == null)
            throw new Exception("Lobby Player Prefab is null. Aborting creation of lobby player");
        
        m_playerProfiles.Add(conn, new PlayerProfile() {m_displayName = "Placeholder_Name"});
        
        LobbyRoomPlayer lobbyPlayerInstance = Instantiate(m_lobbyPlayerPrefab);

        //If first player in lobby, make him leader
        if (RoomPlayers.Count == 0)
            lobbyPlayerInstance.IsLeader = true;
        
        NetworkServer.AddPlayerForConnection(conn, lobbyPlayerInstance.gameObject);
    }

    public PlayerProfile GetPlayerProfileFromConnection(NetworkConnectionToClient connection)
    {
        if (!m_playerProfiles.ContainsKey(connection))
        {
            Debug.LogError("No player profile associated with connection " + connection);
            return null;
        }

        return m_playerProfiles[connection];
    }
}