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

    [SyncVar] 
    private string m_displayName;
    
    [SerializeField] 
    private string m_playerId = "Player";

    public string PlayerId => m_playerId;

    private CustomPlayerInput m_playerInput;
    
    private WorldGrid m_worldGrid;

    private WorldGridNode m_currentNode = null;
    private Vector3 m_currentMoveDirection = Vector3.zero;

    private float m_moveTimer = 0;

    private List<WorldGridNode> m_nodeTrail = new List<WorldGridNode>();

    public List<WorldGridNode> m_ownedNodes = new List<WorldGridNode>();
    
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
        m_worldGrid = WorldGrid.Instance;

        m_currentNode = m_worldGrid.Grid.GetNode(transform.position);
    }

    public override void OnStartClient()
    {
        NetworkManagerCustom.Instance.GamePlayers.Add(this);
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
                WorldGridNode targetNode = m_worldGrid.Grid.GetNeighbour(m_currentNode, new Vector2Int((int)m_currentMoveDirection.x, (int)m_currentMoveDirection.z));

                if (targetNode != null && targetNode != m_currentNode)
                {
                    StepOnNode(targetNode);
                }
                
                m_moveTimer = 0;
            }
            else
            {
                m_moveTimer += Time.deltaTime;
            }
        }
    }

    private void StepOnNode(WorldGridNode node)
    {
        m_currentNode = node;
        
        transform.position = node.m_worldPosition;

        transform.rotation = Quaternion.LookRotation(m_currentMoveDirection);

        if (node.m_ownerPlayerId != PlayerId)
        {
            node.SetTrail(this);
            
            m_nodeTrail.Add(node);
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

    private List<WorldGridNode> GetNodesWithinTrail(List<WorldGridNode> trail)
    {
        // Identify the minimum and maximum X and Y coordinates
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (WorldGridNode node in trail)
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

        List<WorldGridNode> nodesWithinEnclosedArea = new List<WorldGridNode>();

        // Iterate over all nodes within the bounding box defined by the minimum and maximum coordinates
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                WorldGridNode currentNode = m_worldGrid.Grid.GetNode(x, y);

                // If the current node is within the enclosed area, add it to the result
                if (!nodesWithinEnclosedArea.Contains(currentNode))
                {
                    nodesWithinEnclosedArea.Add(currentNode);
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
