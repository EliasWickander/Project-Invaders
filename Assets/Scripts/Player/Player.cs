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
    private OnGameStartedEvent m_onGameStartedEvent;
    
    [SyncVar] 
    private string m_displayName;
    
    [SerializeField] 
    private string m_playerId = "Player";

    public string PlayerId => m_playerId;

    private CustomPlayerInput m_playerInput;

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
    private void Awake()
    {
        m_playerInput = new CustomPlayerInput();
    }

    private void OnEnable()
    {
        m_playerInput.Enable();

        m_playerInput.Movement.Movement.performed += OnMovementPerformed;
    }

    private void OnDisable()
    {
        m_playerInput.Disable();
        m_playerInput.Movement.Movement.performed -= OnMovementPerformed;
    }

    private void Start()
    {
        m_currentTile = WorldGrid.Instance.GetTile(transform.position);
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
        if(m_onGameStartedEvent != null)
            m_onGameStartedEvent.Raise(new OnGameStartedEventData() {m_isOwned = isOwned, m_connectionType = ConnectionType.Client});
    }

    public override void OnStartClient()
    {
        NetworkManagerCustom.Instance.OnPlayerJoinedGame(this, isServer);
    }

    public override void OnStopClient()
    {
        NetworkManagerCustom.Instance.GamePlayers.Remove(this);
    }

    private void OnMovementPerformed(InputAction.CallbackContext value)
    {
        Vector2 inputDir = value.ReadValue<Vector2>();

        m_currentMoveDirection = new Vector3(inputDir.x, 0, inputDir.y);
    }
    
    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (m_currentMoveDirection != Vector3.zero)
        {
            if (m_moveTimer >= m_playerData.MoveSpeed)
            {
                WorldGrid worldGrid = WorldGrid.Instance;
                WorldGridTile targetTile = worldGrid.GetNeighbour(m_currentTile, new Vector2Int((int)m_currentMoveDirection.x, (int)m_currentMoveDirection.z));

                if (targetTile != null && targetTile != m_currentTile)
                {
                    StepOnNode(targetTile);
                }
                
                m_moveTimer = 0;
            }
            else
            {
                m_moveTimer += Time.deltaTime;
            }
        }
    }

    private void StepOnNode(WorldGridTile tile)
    {
        m_currentTile = tile;
        
        transform.position = tile.transform.position;

        transform.rotation = Quaternion.LookRotation(m_currentMoveDirection);

        if (tile.m_ownerPlayerId != PlayerId)
        {
            tile.SetTrail(this);
            
            m_nodeTrail.Add(tile);
        }
        else
        {
            if(m_nodeTrail.Count <= 0)
                return;
            
            foreach (var nodeToOwn in GetNodesWithinTrail(m_nodeTrail))
            {
                if(nodeToOwn.m_ownerPlayerId != PlayerId)
                    nodeToOwn.SetOwner(this);
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
                WorldGrid worldGrid = WorldGrid.Instance;
                
                WorldGridTile currentTile = worldGrid.GetTile(x, y);

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
}
