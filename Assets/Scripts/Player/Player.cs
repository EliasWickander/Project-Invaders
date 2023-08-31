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
    public Vector3 CurrentMoveDirection => m_currentMoveDirection;

    private float m_moveTimer = 0;

    private List<WorldGridTile> m_nodeTrail = new List<WorldGridTile>();

    private WorldGridTile m_lastOwnedTileSteppedOn = null;

    private List<WorldGridTile> m_enclosedCornerNodes = new List<WorldGridTile>();

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
            if (m_nodeTrail.Count > 0)
            { 
                //Step 1: Enclose loop
                //Step 2: Do flood fill algorithm to fill all tiles inside of loop until loop edge reached
                bool loopSuccess = EncloseLoop(m_lastOwnedTileSteppedOn, tile, m_nodeTrail, out List<WorldGridTile> loop);

                NewFill(loop);
                // WorldGridTile floodStartTile = null;
                //
                // if (floodStartTile != null)
                // {
                //     FloodFill(loop, floodStartTile);
                // }

                // foreach (WorldGridTile loopTile in loop)
                // {
                //     PlayGrid.Instance.SetTileOwner(loopTile.m_gridPos, PlayerId);
                // }

                // List<WorldGridTile> surroundedTiles = GetTilesWithinTrail(m_nodeTrail);
                //
                // foreach (var nodeToOwn in surroundedTiles)
                // {
                //     if (nodeToOwn.TileStatus.OwnerPlayerId != PlayerId)
                //     {
                //         playGrid.SetTileOwner(nodeToOwn.m_gridPos, PlayerId);   
                //     }
                // }

                m_nodeTrail.Clear();   
                m_enclosedCornerNodes.Clear();
            }

            m_lastOwnedTileSteppedOn = tile;
        }
    }

    private void NewFill(List<WorldGridTile> loop)
    {
        PlayGrid playGrid = PlayGrid.Instance;
        
        Dictionary<int, List<WorldGridTile>> potentialConnections = new Dictionary<int, List<WorldGridTile>>();
        
        foreach (WorldGridTile tile in loop)
        {
            WorldGridTile upNeighbour = playGrid.GetNeighbour(tile, Vector2Int.up);
            WorldGridTile downNeighbour = playGrid.GetNeighbour(tile, Vector2Int.down);

            if ((upNeighbour != null && loop.Contains(upNeighbour)) || (downNeighbour != null && loop.Contains(downNeighbour)))
            {
                if (!potentialConnections.ContainsKey(tile.m_gridPos.y))
                    potentialConnections.Add(tile.m_gridPos.y, new List<WorldGridTile>() {tile});
                else
                    potentialConnections[tile.m_gridPos.y].Add(tile);
            }
        }
        
        foreach (KeyValuePair<int, List<WorldGridTile>> tentativeConnection in potentialConnections)
        {
            Tuple<WorldGridTile, WorldGridTile> xConnection = FindFurthestTiles(tentativeConnection.Value);
            
            WorldGridTile first = xConnection.Item1;
            WorldGridTile second = xConnection.Item2;

            Vector2Int fillDirection = second.m_gridPos.x > first.m_gridPos.x ? Vector2Int.right : Vector2Int.left;
            
            WorldGridTile current = first;
            
            while (current != second)
            {
                playGrid.SetTileOwner(current.m_gridPos, PlayerId);
            
                current = playGrid.GetNeighbour(current, fillDirection);
            }
            
            //Also fill last node
            playGrid.SetTileOwner(current.m_gridPos, PlayerId);
        }
    }

    private Tuple<WorldGridTile, WorldGridTile> FindFurthestTiles(List<WorldGridTile> tiles)
    {
        if (tiles == null)
            return null;

        if (tiles.Count <= 0)
            return null;

        if (tiles.Count == 1)
            return Tuple.Create(tiles[0], tiles[0]);

        if (tiles.Count == 2)
            return Tuple.Create(tiles[0], tiles[1]);
        
        WorldGridTile first = null;
        WorldGridTile second = null;
        int maxDistance = 0;

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = i + 1; j < tiles.Count; j++)
            {
                int xDelta = Mathf.Abs(tiles[i].m_gridPos.x - tiles[j].m_gridPos.x);
                if (xDelta > maxDistance)
                {
                    maxDistance = xDelta;
                    first = tiles[i];
                    second = tiles[j];
                }
            }
        }

        return Tuple.Create(first, second);
    }
    private void FloodFill(List<WorldGridTile> loop, WorldGridTile startTile)
    {
        List<WorldGridTile> openSet = new List<WorldGridTile>();
        List<WorldGridTile> closedSet = new List<WorldGridTile>();
        openSet.Add(startTile);

        Debug.DrawRay(startTile.transform.position, Vector3.up * 10, Color.red, 0.5f);

        foreach (WorldGridTile tile in loop)
        {
            Debug.DrawRay(tile.transform.position, Vector3.up * 10, Color.green, 0.5f);
        }
        int count = 0;

        while (openSet.Count > 0)
        {
            if (count >= 100)
            {
                Debug.Log("Failed to do flood fill. Took too long");
                Debug.Break();
                break;   
            }

            WorldGridTile current = openSet[0];
            //PlayGrid.Instance.SetTileOwner(current.m_gridPos, PlayerId);

            Debug.DrawRay(current.transform.position, Vector3.up * 5, Color.blue, 0.5f);
            openSet.Remove(current);
            closedSet.Add(current);
            
            foreach (WorldGridTile neighbour in PlayGrid.Instance.GetNeighbours(current))
            {
                if (!loop.Contains(neighbour) && !closedSet.Contains(neighbour))
                {
                    openSet.Add(neighbour);
                    Debug.Break();
                }
            }

            count++;
        }

        Debug.Log("flood fill done");
    }
    
    private bool EncloseLoop(WorldGridTile startTile, WorldGridTile endTile, List<WorldGridTile> trail, out List<WorldGridTile> result)
    {
        result = trail;

        List<PathNode> path = Pathfinding.FindPath(this, startTile, endTile);

        if (path != null)
        {
            foreach (PathNode pathNode in path)
            {
                PlayGrid.Instance.SetTilePendingOwner(pathNode.m_tile.m_gridPos, PlayerId);
                result.Add(pathNode.m_tile);
            }
        }

        return path != null;
    }
    private List<WorldGridTile> GetTilesWithinTrail(List<WorldGridTile> trail)
    {
        if (trail.Count <= 0)
            return null;
        
        //Scanline filling algorithm?
        WorldGridTile startTile = trail[0];
        WorldGridTile endTile = trail[trail.Count - 1];

        return null;
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
