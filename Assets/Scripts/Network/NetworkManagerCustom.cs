using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PlayerProfile
{
    public NetworkConnectionToClient m_connection;
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
    public OnPreGameEndedEvent m_onPreGameEndedEvent;

    [Header("Game Events")] 
    public OnPlayerCreatedEvent m_onPlayerCreatedEvent;
    public OnGameStartedEvent m_onGameStartedEvent;
    public List<LobbyRoomPlayer> LobbyPlayers { get; } = new List<LobbyRoomPlayer>();
    public List<PreGamePlayer> PreGamePlayers { get; } = new List<PreGamePlayer>();
    public List<Player> GamePlayers { get; } = new List<Player>();

    private Dictionary<NetworkConnectionToClient, PlayerProfile> m_playerProfiles = new Dictionary<NetworkConnectionToClient, PlayerProfile>();

    private List<Transform> m_availableStartPoints;
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        NetworkServer.RegisterHandler<CreateLobbyPlayerMessage>(OnCreateLobbyPlayer);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        
        //Create lobby player when client connects (assumes player can only connect to lobby)
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

    /// <summary>
    /// On connected to server
    /// </summary>
    /// <param name="conn"></param>
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        
        //If no more players allowed in server, disconnect
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if(m_onServerConnectedEvent != null)
            m_onServerConnectedEvent.Raise(new OnServerConnectedEventData() {m_connection = conn});
    }

    /// <summary>
    /// On disconnected from server
    /// </summary>
    /// <param name="conn">Connection</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if(conn.identity == null)
            return;
        
        //Remove player from appropriate list on disconnect
        LobbyRoomPlayer lobbyPlayer = conn.identity.GetComponent<LobbyRoomPlayer>();
        
        if (lobbyPlayer != null)
        {
            LobbyPlayers.Remove(lobbyPlayer);

            NotifyLobbyPlayersClientDisconnected(lobbyPlayer.PlayerIndex);   
        }
        else
        {
            PreGamePlayer preGamePlayer = conn.identity.GetComponent<PreGamePlayer>();

            if (preGamePlayer != null)
            {
                PreGamePlayers.Remove(preGamePlayer);
            }
            else
            {
                Player gamePlayer = conn.identity.GetComponent<Player>();

                if (gamePlayer != null)
                {
                    GamePlayers.Remove(gamePlayer);
                }
            }
        }

        if(m_onServerDisconnectedEvent != null)
            m_onServerDisconnectedEvent.Raise(new OnServerDisconnectedEventData() {m_connection = conn});
        
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// On server stopped
    /// </summary>
    public override void OnStopServer()
    {
        LobbyPlayers.Clear();
        PreGamePlayers.Clear();
        GamePlayers.Clear();
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        
        if(m_onStartHostEvent)
            m_onStartHostEvent.Raise();
    }

    public void NotifyLobbyPlayersClientDisconnected(int clientIndex)
    {
        foreach(LobbyRoomPlayer player in LobbyPlayers)
            player.HandleClientDisconnected(clientIndex);
    }

    /// <summary>
    /// Host lobby
    /// </summary>
    public void HostLobby()
    {
        if(NetworkServer.active || NetworkClient.active)
            return;

        if(m_onStartHostAttemptEvent)
            m_onStartHostAttemptEvent.Raise();

        StartCoroutine(HostLobbyAsync());
    }

    //Add some delay to host lobby
    public IEnumerator HostLobbyAsync()
    {
        yield return new WaitForSeconds(0.1f);

        StartHost();
    }

    /// <summary>
    /// Join Lobby with ip
    /// </summary>
    /// <param name="ip">ip address</param>
    public void JoinLobby(string ip)
    {
        if(NetworkClient.active)
            return;

        networkAddress = ip;
        
        if(m_onClientConnectionAttemptEvent)
            m_onClientConnectionAttemptEvent.Raise();

        StartCoroutine(JoinLobbyAsync());
    }
    
    //Add some delay to join lobby
    private IEnumerator JoinLobbyAsync()
    {
        yield return new WaitForSeconds(0.1f);

        StartClient();
    }

    /// <summary>
    /// Called when player joins lobby
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerJoinedLobby(LobbyRoomPlayer player)
    {
        if(player == null)
            return;
        
        LobbyPlayers.Add(player);

        if(m_onPlayerJoinedLobbyEvent != null)
            m_onPlayerJoinedLobbyEvent.Raise(new OnPlayerJoinedLobbyEventData() {m_player = player});
    }

    /// <summary>
    /// Called when player joins pre game
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerJoinedPreGame(PreGamePlayer player)
    {
        if(player == null)
            return;
        
        PreGamePlayers.Add(player);

        if(m_onPlayerJoinedPreGameEvent != null)
            m_onPlayerJoinedPreGameEvent.Raise(new OnPlayerJoinedPreGameEventData() {m_player = player});
    }
    
    /// <summary>
    /// Called when player joins game
    /// </summary>
    /// <param name="player"></param>
    public void OnPlayerJoinedGame(Player player, bool calledOnServer = false)
    {
        if(player == null)
            return;
        
        GamePlayers.Add(player);

        if (calledOnServer)
        {
            if (GamePlayers.Count >= PreGamePlayers.Count)
            {
                //All players connected to game. Start game for real
                if(m_onGameStartedEvent != null)
                    m_onGameStartedEvent.Raise(new OnGameStartedEventData() {m_isOwned = true, m_connectionType = ConnectionType.Server});

                foreach (Player gamePlayer in GamePlayers)
                    gamePlayer.OnGameStarted();
            }
        }
    }

    /// <summary>
    /// Disconnects client from server
    /// </summary>
    public void DisconnectClient()
    {
        StopClient();
    }

    /// <summary>
    /// Called when scene has changed on server
    /// </summary>
    /// <param name="sceneName">Name of new scene</param>
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

    /// <summary>
    /// On scene loaded for specific player
    /// </summary>
    /// <param name="connection">Connection</param>
    /// <param name="playerProfile">Player Profile</param>
    private void SceneLoadedForPlayer(NetworkConnectionToClient connection, PlayerProfile playerProfile)
    {
        if (Utils.IsSceneActive(m_gameScene))
        {
            //Replace lobby player with pre-game player
            PreGamePlayer preGamePlayerInstance = Instantiate(m_preGamePlayerPrefab);
            preGamePlayerInstance.SetDisplayName(playerProfile.m_displayName);
            
            NetworkServer.ReplacePlayerForConnection(connection, preGamePlayerInstance.gameObject, true);

            m_availableStartPoints = new List<Transform>(startPositions);
        }
    }
    
    /// <summary>
    /// Start game
    /// </summary>
    public void StartPreGame()
    {
        //If starting from lobby, change scene to game on server side
        if (SceneManager.GetActiveScene().path == m_lobbyScene)
        {
            ServerChangeScene(m_gameScene);
        }
    }

    /// <summary>
    /// Called on server when CreateLobbyPlayer network event is invoked
    /// </summary>
    /// <param name="conn">Connection</param>
    /// <param name="message">Data used for lobby player creation</param>
    /// <exception cref="Exception"></exception>
    private void OnCreateLobbyPlayer(NetworkConnectionToClient conn, CreateLobbyPlayerMessage message)
    {
        if (m_lobbyPlayerPrefab == null)
            throw new Exception("Lobby Player Prefab is null. Aborting creation of lobby player");

        LobbyRoomPlayer lobbyPlayerInstance = Instantiate(m_lobbyPlayerPrefab);

        //If first player in lobby, make him leader
        if (LobbyPlayers.Count == 0)
            lobbyPlayerInstance.IsLeader = true;
        
        NetworkServer.AddPlayerForConnection(conn, lobbyPlayerInstance.gameObject);
        
        m_playerProfiles.Add(conn, new PlayerProfile() {m_connection = conn, m_displayName = "Placeholder_Name"});
    }

    /// <summary>
    /// Get player profile from connection
    /// </summary>
    /// <param name="connection">Connection</param>
    /// <returns>Player profile associated with connection</returns>
    public PlayerProfile GetPlayerProfileFromConnection(NetworkConnectionToClient connection)
    {
        if (!m_playerProfiles.ContainsKey(connection))
        {
            Debug.LogError("No player profile associated with connection " + connection);
            return null;
        }

        return m_playerProfiles[connection];
    }

    public void StartGame()
    {
        Debug.LogError("starting game");

        foreach (PreGamePlayer preGamePlayer in PreGamePlayers)
        {
            //Replace pre-game player with game player
            PlayerProfile playerProfile = GetPlayerProfileFromConnection(preGamePlayer.connectionToClient);

            if (playerProfile != null)
            {
                GameObject oldPlayerObject = playerProfile.m_connection.identity.gameObject;

                Transform startPoint = GetStartPosition();

                Player gamePlayerInstance = startPoint != null
                    ? Instantiate(m_gamePlayerPrefab, startPoint.position, startPoint.rotation) 
                    : Instantiate(m_gamePlayerPrefab, Vector3.zero, Quaternion.identity);

                gamePlayerInstance.SetPlayerId(Guid.NewGuid().ToString());
                gamePlayerInstance.SetDisplayName(playerProfile.m_displayName);
            
                NetworkServer.ReplacePlayerForConnection(playerProfile.m_connection, gamePlayerInstance.gameObject, true);

                Destroy(oldPlayerObject, 0.1f);
                
                if(m_onPlayerCreatedEvent != null)
                    m_onPlayerCreatedEvent.Raise(new OnPlayerCreatedEventData() {m_isOwned = true, m_connectionType = ConnectionType.Server, m_player = gamePlayerInstance, m_spawnTransform = startPoint});
            }
            
            preGamePlayer.OnGameStarted();
        }
        
        if(m_onPreGameEndedEvent != null)
            m_onPreGameEndedEvent.Raise(new OnPreGameEndedEventData() {m_isOwned = true, m_connectionType = ConnectionType.Server});
    }
    
    public bool CanStartGame()
    {
        if (!Utils.IsSceneActive(m_gameScene) || PreGamePlayers.Count <= 0)
            return false;
        
        foreach (PreGamePlayer preGamePlayer in PreGamePlayers)
        {
            if (!preGamePlayer.HasSelectedElement)
                return false;
        }

        return true;
    }

    public override Transform GetStartPosition()
    {
        if (m_availableStartPoints.Count > 0)
            return m_availableStartPoints[Random.Range(0, m_availableStartPoints.Count - 1)];
        
        return null;
    }

    public void EnableStartPoint(Transform startPoint)
    {
        if (m_availableStartPoints.Contains(startPoint) || !startPositions.Contains(startPoint))
        {
            Debug.LogWarning("Attempting to enable invalid start point. Something is wrong", gameObject);
            return;
        }

        m_availableStartPoints.Add(startPoint);
    }
    
    public void DisableStartPoint(Transform startPoint)
    {
        if (!m_availableStartPoints.Contains(startPoint))
        {
            Debug.LogWarning("Attempting to disable invalid start point. Something is wrong", gameObject);
            return;
        }

        m_availableStartPoints.Remove(startPoint);
    }
}