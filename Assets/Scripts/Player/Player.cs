using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] 
    private PlayerData m_playerData;

    public PlayerData PlayerData => m_playerData;
    
    [SerializeField] 
    private Client_OnGameStartedEvent m_onGameStartedClientEvent;
    
    [SyncVar] 
    private string m_displayName;

    [SyncVar]
    public string PlayerId;
    
    private WorldGridTile m_currentTile = null;
    private Vector3 m_currentMoveDirection = Vector3.zero;

    private float m_moveTimer = 0;

    private List<WorldGridTile> m_nodeTrail = new List<WorldGridTile>();

    public List<WorldGridTile> m_ownedNodes = new List<WorldGridTile>();

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
                    StepOnNode(targetTile.m_gridPos);
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
    private void StepOnNode(Vector2Int nodePos)
    {
        PlayGrid playGrid = PlayGrid.Instance;
        WorldGridTile tile = playGrid.GetNode(nodePos.x, nodePos.y);
        
        m_currentTile = tile;
        
        transform.position = tile.transform.position;
        transform.rotation = Quaternion.LookRotation(m_currentMoveDirection);
        
        if (tile.TileStatus.OwnerPlayerId != PlayerId)
        {
            playGrid.SetTilePendingOwner(tile.m_gridPos, PlayerId);
            
            //If walking on tile that's not owned, add it to trail
            m_nodeTrail.Add(tile);
        }
        else
        {
            //If walking on tile that's owned, add all nodes within trailed area to owned tiles
            if(m_nodeTrail.Count <= 0)
                return;
            
            foreach (var nodeToOwn in GetNodesWithinTrail(m_nodeTrail))
            {
                if(nodeToOwn.TileStatus.OwnerPlayerId != PlayerId)
                    playGrid.SetTileOwner(tile.m_gridPos, PlayerId);
            }

            m_nodeTrail.Clear();
        }
    }

    private List<WorldGridTile> GetNodesWithinTrail(List<WorldGridTile> trail)
    {
        // Identify the minimum and maximum X and Y coordinates
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (WorldGridTile node in trail)
        {
            if (node.m_gridPos.x < minX)
                minX = node.m_gridPos.x;
            if (node.m_gridPos.y < minY)
                minY = node.m_gridPos.y;
            if (node.m_gridPos.x > maxX)
                maxX = node.m_gridPos.x;
            if (node.m_gridPos.y > maxY)
                maxY = node.m_gridPos.y;
        }

        List<WorldGridTile> nodesWithinEnclosedArea = new List<WorldGridTile>();

        // Iterate over all nodes within the bounding box defined by the minimum and maximum coordinates
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                PlayGrid playGrid = PlayGrid.Instance;
                
                WorldGridTile currentTile = playGrid.GetNode(x, y);

                // If the current node is within the enclosed area, add it to the result
                if (!nodesWithinEnclosedArea.Contains(currentTile))
                {
                    nodesWithinEnclosedArea.Add(currentTile);
                }
            }
        }

        return nodesWithinEnclosedArea;
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
