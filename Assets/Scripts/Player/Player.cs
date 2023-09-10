using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] 
    private PlayerData m_playerData;

    public PlayerData PlayerData => m_playerData;
    
    [SerializeField] 
    private Client_OnGameStartedEvent m_onGameStartedClientEvent;
    
    [SerializeField] 
    private Server_OnTileSteppedOnEvent m_onTileSteppedOnServerEvent;
    
    [SerializeField] 
    private Client_OnTileSteppedOnEvent m_onTileSteppedOnClientEvent;

    [SyncVar] 
    private string m_displayName;

    [SyncVar]
    public string PlayerId;
    
    private WorldGridTile m_currentTile = null;
    private Vector3 m_currentMoveDirection = Vector3.zero;
    public Vector3 CurrentMoveDirection => m_currentMoveDirection;

    private float m_moveTimer = 0;

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
    
    public event Action<Player> OnSpawnedEventServer; 
    public event Action<Player> OnDeathEventServer;
    
    private void Start()
    {
        m_currentTile = PlayGrid.Instance.GetNode(transform.position);
    }

    [Server]
    public void OnSpawned(Transform spawnTransform)
    {
        SpawnTransform = spawnTransform;
        
        OnSpawnedEventServer?.Invoke(this);
    }

    [Server]
    public void Kill()
    {
        OnDeathEventServer?.Invoke(this);
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
        
        NetworkManagerCustom.Instance.OnPlayerJoinedGame(this, isServer);
    }
    
    public override void OnStopClient()
    {
        GameClient.Instance.GameWorld.RemovePlayerFromWorld(this);
        
        NetworkManagerCustom.Instance.GamePlayers.Remove(this);
    }

    [Command]
    public void SetMoveDirection(Vector3 dir)
    {
        m_currentMoveDirection = dir;
    }
    
    private void Update()
    {
        if(isServer)
            HandleMovement();
    }

    [Server]
    private void HandleMovement()
    {
        if (m_currentMoveDirection != Vector3.zero)
        {
            if (m_moveTimer >= m_playerData.MoveSpeed)
            {
                PlayGrid playGrid = PlayGrid.Instance;
                WorldGridTile targetTile = playGrid.GetNeighbour(m_currentTile, new Vector2Int((int)m_currentMoveDirection.x, (int)m_currentMoveDirection.z));

                if (targetTile != null && targetTile != m_currentTile)
                {
                    StepOnTile(targetTile.m_gridPos);
                }
                
                m_moveTimer = 0;
            }
            else
            {
                m_moveTimer += Time.deltaTime;
            }
        }
    }

    [Server]
    private void StepOnTile(Vector2Int tilePos)
    {
        PlayGrid playGrid = PlayGrid.Instance;
        WorldGridTile tile = playGrid.GetNode(tilePos.x, tilePos.y);

        m_currentTile = tile;

        transform.position = tile.transform.position;
        transform.rotation = Quaternion.LookRotation(m_currentMoveDirection);
        
        if(m_onTileSteppedOnServerEvent != null)
            m_onTileSteppedOnServerEvent.Raise(new OnTileSteppedOnEventData() {m_player = this, m_tilePos = tilePos});
        
        if (m_currentTile.TileStatus.OwnerPlayerId == PlayerId)
            m_lastOwnedTileSteppedOn = m_currentTile;
        
        StepOnNodeRpc(tilePos);
    }

    [ClientRpc]
    private void StepOnNodeRpc(Vector2Int tilePos)
    {
        if(m_onTileSteppedOnClientEvent != null)
            m_onTileSteppedOnClientEvent.Raise(new OnTileSteppedOnEventData() {m_player = this, m_tilePos = tilePos});
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
}
