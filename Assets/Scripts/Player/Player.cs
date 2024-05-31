using System;
using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Mirror;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkPlayer))]
public class Player : NetworkBehaviour
{
    [SerializeField]
    private PlayerData m_playerData;

    public PlayerData PlayerData => m_playerData;

    [SerializeField]
    private NetworkPlayer m_networkPlayer;

    public NetworkPlayer NetworkPlayer => m_networkPlayer;
    
    [SerializeField]
    private Client_OnGameStartedEvent m_onGameStartedClientEvent;

    [SerializeField]
    private Server_OnTileSteppedOnEvent m_onTileSteppedOnServerEvent;

    [SerializeField]
    private Client_OnTileSteppedOnEvent m_onTileSteppedOnClientEvent;

    [SerializeField]
    private Server_OnPlayerSpawnedEvent m_onPlayerSpawnedServerEvent;

    [SerializeField]
    private Client_OnPlayerSpawnedEvent m_onPlayerSpawnedClientEvent;

    [SerializeField]
    private Server_OnPlayerKilledEvent m_onPlayerKilledServerEvent;

    [SerializeField]
    private Client_OnPlayerKilledEvent m_onPlayerKilledClientEvent;

    [SyncVar]
    private string m_displayName;
    public string DisplayName => m_displayName;

    [SyncVar]
    public string PlayerId;

    [SyncVar(hook = nameof(OnDead))] 
    public bool IsDead;

    private WorldGridTile m_currentTile = null;
    private Vector3 m_currentMoveDirection = Vector3.zero;
    public Vector3 CurrentMoveDirection => m_currentMoveDirection;

    public double MoveTimer { get; set; }
    public WorldGridTile m_lastOwnedTileSteppedOn = null;

    private Transform m_spawnTransform = null;

    public Transform SpawnTransform
    {
        get
        {
            return m_spawnTransform;
        }
        set
        {
            m_spawnTransform = value;
        }
    }

    private WorldGridTile m_spawnTile;
    public WorldGridTile SpawnTile => m_spawnTile;

    public List<Vector2Int> PendingTiles { get; set; } = new List<Vector2Int>();

    public Transform m_visualContainer;
    
    private void Awake()
    {
	    m_networkPlayer = GetComponent<NetworkPlayer>();
    }

    [Server]
    public void OnSpawned(Transform spawnTransform, WorldGridTile spawnTile)
    {
	    IsDead = false;

        SpawnTransform = spawnTransform;
        m_spawnTile = spawnTile;
        m_currentTile = m_spawnTile;
		PendingTiles.Clear();
		SetMoveDirection(Vector3.zero);
        
        if(m_onPlayerSpawnedServerEvent != null)
            m_onPlayerSpawnedServerEvent.Raise(new OnPlayerSpawnedGameEventData() {m_player = this, m_startTerritoryRadius = m_playerData.StartTerritoryRadius});

        var playerCurrentState = m_networkPlayer.GetState(NetworkSimulation.Instance.CurrentTick);

        OnSpawnedRpc(playerCurrentState, spawnTransform, spawnTile.m_gridPos);
    }

    [ClientRpc]
    private void OnSpawnedRpc(NetworkPlayerState playerStateOnSpawn, Transform spawnTransform, Vector2Int spawnTilePos)
    {
	    SpawnTransform = spawnTransform;
	    m_spawnTile = PlayGrid.Instance.GetNode(spawnTilePos.x, spawnTilePos.y);
	    m_currentTile = m_spawnTile;
	    PendingTiles.Clear();
        SetMoveDirection(Vector3.zero);

        if (m_networkPlayer != null)
        {
            m_networkPlayer.Messenger.LatestServerState = playerStateOnSpawn;

            m_networkPlayer.SetActive(true);
        }
        
        if(m_onPlayerSpawnedClientEvent != null)
            m_onPlayerSpawnedClientEvent.Raise(new OnPlayerSpawnedGameEventData() {m_player = this, m_startTerritoryRadius = m_playerData.StartTerritoryRadius});
    }
    
    [Server]
    public void Kill()
    {
        if(m_onPlayerKilledServerEvent != null)
            m_onPlayerKilledServerEvent.Raise(new OnPlayerKilledGameEventData() {m_player = this});   
        
        IsDead = true;

        KillRpc();

        ResetState();
    }

    [ClientRpc]
    private void KillRpc()
    {
        if (!NetworkServer.active)
        {
            //When killed, pause prediction/network simulation. We're just a dead body until spawned again
            m_networkPlayer?.SetActive(false);
            
            if(m_onPlayerKilledClientEvent != null)
                m_onPlayerKilledClientEvent.Raise(new OnPlayerKilledGameEventData() {m_player = this});   
        }
    }

    private void OnDead(bool oldValue, bool newValue)
    {
        if(oldValue == newValue)
            return;

        m_visualContainer.gameObject.SetActive(!newValue);
    }

    [ClientRpc]
    public void OnGameStarted()
    {
        if(!isOwned)
            return;

        if(m_onGameStartedClientEvent != null)
            m_onGameStartedClientEvent.Raise(new OnGameStartedEventData() {});
    }

    public override void OnStartClient()
    {
        GameClient.Instance.GameWorld.AddPlayerToWorld(this);

        //Add input controller if locally owned player object
        if (isOwned)
        {
            PlayerInputController inputController = gameObject.AddComponent<PlayerInputController>();
            inputController.SetTarget(this);
        }

        NetworkManagerCustom.Instance.OnPlayerJoinedGame(this);
    }

    public override void OnStopClient()
    {
        GameClient.Instance.GameWorld.RemovePlayerFromWorld(this);

        NetworkManagerCustom.Instance.OnPlayerLeftGame(this);
    }

    public void SetMoveDirection(Vector3 dir)
    {
	    m_currentMoveDirection = dir;
    }
    
    public void Move(Vector3 direction)
    {
	    PlayGrid playGrid = PlayGrid.Instance;
	    WorldGridTile targetTile = playGrid.GetNeighbour(m_currentTile, new Vector2Int((int)direction.x, (int)direction.z));

	    if (targetTile != null && targetTile != m_currentTile)
	    {
		    transform.rotation = Quaternion.LookRotation(direction);
	                
		    StepOnTile(targetTile.m_gridPos);
	    }
    }

    private void StepOnTile(Vector2Int tilePos)
    {
        PlayGrid playGrid = PlayGrid.Instance;
        WorldGridTile tile = playGrid.GetNode(tilePos.x, tilePos.y);

        string pendingOwnerPlayerId = tile.TileStatus.PendingOwnerPlayerId;
        
        m_currentTile = tile;

        transform.position = tile.transform.position;

        if (NetworkServer.active)
        {
	        if(m_onTileSteppedOnServerEvent != null)
		        m_onTileSteppedOnServerEvent.Raise(new OnTileSteppedOnEventData() {m_player = this, m_tilePos = tilePos});   
        }

        if (NetworkClient.active)
        {
	        if(m_onTileSteppedOnClientEvent != null)
		        m_onTileSteppedOnClientEvent.Raise(new OnTileSteppedOnEventData() {m_player = this, m_tilePos = tilePos});
        }

        if (m_currentTile.TileStatus.OwnerPlayerId == PlayerId)
            m_lastOwnedTileSteppedOn = m_currentTile;

        if (NetworkServer.active)
        {
            if (!string.IsNullOrEmpty(pendingOwnerPlayerId))
            {
                if (pendingOwnerPlayerId == PlayerId)
                {
                    //If stepped on own trail, kill player
                    Kill();
                }
                else
                {
                    //If stepped on other player's trail, kill player and take their territory
                    Player steppedOnPlayer = GameClient.Instance.GameWorld.GetPlayerFromId(pendingOwnerPlayerId);
		        
                    TileManager.Instance.ChangeTileOwnership(pendingOwnerPlayerId, PlayerId);
                    steppedOnPlayer.Kill();
                }
            }      
        }
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        m_displayName = displayName;
    }

    [Server]
    public void SetPlayerId(string id)
    {
        PlayerId = id;
    }

    private void ResetState()
    {
        m_currentTile = null;
        m_lastOwnedTileSteppedOn = null;
        m_spawnTransform = null;
        m_spawnTile = null;
        m_currentMoveDirection = Vector3.zero;
    }
}
