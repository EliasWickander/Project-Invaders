using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] 
    private string m_playerId = "Player";

    public string PlayerId => m_playerId;

    [SerializeField]
    private Material m_trailMaterial;

    public Material TrailMaterial => m_trailMaterial;
    
    [SerializeField] 
    private Material m_ownerMaterial;

    public Material OwnerMaterial => m_ownerMaterial;
    
    [SerializeField] 
    private float m_moveSpeed = 1;
    
    private WorldGrid m_worldGrid;

    private Vector3 m_currentMoveDirection = Vector3.zero;

    private float m_moveTimer = 0;

    private List<WorldGridNode> m_nodeTrail = new List<WorldGridNode>();

    public List<WorldGridNode> m_ownedNodes = new List<WorldGridNode>();
    

    private void Start()
    {
        m_worldGrid = WorldGrid.Instance;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 moveInputDir = GetMoveInputDir();

        if (moveInputDir != Vector3.zero)
        {
            m_currentMoveDirection = moveInputDir;

            m_moveTimer = m_moveSpeed;
        }
        
        if (m_currentMoveDirection != Vector3.zero)
        {
            if (m_moveTimer >= m_moveSpeed)
            {
                WorldGridNode currentNode = m_worldGrid.Grid.GetNode(transform.position);
                
                WorldGridNode targetNode = m_worldGrid.Grid.GetNeighbour(currentNode, new Vector2Int((int)m_currentMoveDirection.x, (int)m_currentMoveDirection.z));

                if (targetNode != null && targetNode != currentNode)
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

    private Vector3 GetMoveInputDir()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            return Vector3.forward;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            return Vector3.back;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            return Vector3.left;
        
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            return Vector3.right;

        return Vector3.zero;
    }

    private void StepOnNode(WorldGridNode node)
    {
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
}
