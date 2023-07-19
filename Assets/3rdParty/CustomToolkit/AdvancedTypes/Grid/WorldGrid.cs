using System.Collections.Generic;
using UnityEngine;

namespace CustomToolkit.AdvancedTypes
{
    public abstract class WorldGridNode : MonoBehaviour
    {
        public Vector2Int m_gridPos;
    }
    
    public abstract class WorldGrid<TNode> : MonoBehaviour where TNode : WorldGridNode, new()
    {
        [SerializeField] 
        private bool m_debug = false;
        
        [SerializeField] 
        private Color m_debugTileColor = Color.cyan;
        
        [SerializeField] 
        private bool m_createOnAwake = true;

        [SerializeField] 
        private TNode m_nodePrefab;
		
        [SerializeField]
        private Vector2 m_gridWorldSize = new Vector2(10, 10);
        public Vector2 GridWorldSize => m_gridWorldSize;
		
        [SerializeField]
        private float m_nodeRadius = 0.2f;
        public float NodeRadius => m_nodeRadius;
        public float NodeDiameter => NodeRadius * 2;
		
        private TNode[,] m_nodes;
        public TNode[,] Nodes => m_nodes;
		
        private Vector2Int m_gridSize;
        public Vector2Int GridSize => m_gridSize;

        protected virtual void Awake()
        {
            if(m_createOnAwake)
                CreateGrid();
        }

        public void CreateGrid()
        {
            if (m_nodePrefab == null)
            {
                Debug.LogError("Cannot create grid. Node Prefab is null");
                return;
            }
			
            CalcGridSize();
			
            m_nodes = new TNode[m_gridSize.x, m_gridSize.y];

            for (int x = 0; x < m_gridSize.x; x ++) 
            {
                for (int y = 0; y < m_gridSize.y; y ++) 
                {
                    Vector3 worldPoint = GetWorldPointFromGridPoint(x, y);
                    Vector2Int gridPos = new Vector2Int(x, y);

                    TNode newNode = Instantiate(m_nodePrefab, worldPoint, Quaternion.identity);
                    newNode.m_gridPos = gridPos;
					
                    m_nodes[x, y] = newNode;

                    OnNodeCreated(m_nodes[x, y]);
                }
            }
        }

        public bool IsInGridBounds(Vector3 position)
        {
            if (m_nodes.Length <= 0)
                return false;

            Vector3 bottomLeftPos = m_nodes[0, 0].transform.position;
            Vector3 topRightPos = m_nodes[m_gridSize.x - 1, m_gridSize.y - 1].transform.position;
            return position.x >= bottomLeftPos.x - m_nodeRadius 
                   && position.z >= bottomLeftPos.z - m_nodeRadius
                   && position.x <= topRightPos.x  + m_nodeRadius
                   && position.z <= topRightPos.z + m_nodeRadius;
        }
		
        /// <summary>
        /// Gets node by world position
        /// </summary>
        /// <param name="worldPosition">World position</param>
        /// <returns>Returns node closest to world position</returns>
        public TNode GetNode(Vector3 worldPosition)
        {
            TNode closestNode = null;
            float closestDist = Mathf.Infinity;
			
            for (int x = 0; x < m_gridSize.x; x++)
            {
                for (int y = 0; y < m_gridSize.y; y++)
                {
                    TNode node = m_nodes[x, y];

                    float sqrDistToNode = (worldPosition - m_nodes[x, y].transform.position).sqrMagnitude;

                    if (sqrDistToNode < closestDist)
                    {
                        closestDist = sqrDistToNode;
                        closestNode = node;
                    }
                }
            }

            return closestNode;
        }
		
        /// <summary>
        /// Gets node by grid position
        /// </summary>
        /// <param name="x">Grid X position</param>
        /// <param name="y">Grid Y position</param>
        /// <returns>Returns node in grid position</returns>
        public TNode GetNode(int x, int y)
        {
            if (x >= m_gridSize.x || y >= m_gridSize.y || x < 0 || y < 0)
                return null;

            return m_nodes[x, y];
        }
		
        /// <summary>
        /// Gets neighbour in a direction from node
        /// </summary>
        /// <param name="node">Node to get neighbour of</param>
        /// <param name="direction">Direction</param>
        /// <returns>Returns neighbour in direction</returns>
        public TNode GetNeighbour(TNode node, Vector2Int direction)
        {
            direction.x = Mathf.Clamp(direction.x, -1, 1);
            direction.y = Mathf.Clamp(direction.y, -1, 1);

            if (node == null || direction == Vector2Int.zero)
                return null;

            return GetNode(node.m_gridPos.x + direction.x, node.m_gridPos.y + direction.y);
        }
		
        /// <summary>
        /// Get neighbouring nodes
        /// </summary>
        /// <param name="node">Node to get neighbours of</param>
        /// <returns>Returns neighbouring nodes</returns>
        public List<TNode> GetNeighbours(TNode node)
        {
            List<TNode> neighbours = new List<TNode>();
	    
            if (node.m_gridPos.x > 0)
            {
                //Left
                neighbours.Add(GetNeighbour(node, new Vector2Int(-1, 0)));
	        
                //Left Down
                if(node.m_gridPos.y > 0)
                    neighbours.Add(GetNeighbour(node, new Vector2Int(-1, -1)));
	        
                //Left Up
                if(node.m_gridPos.y < m_gridSize.y - 1)
                    neighbours.Add(GetNeighbour(node, new Vector2Int(-1, 1)));
            }

            if (node.m_gridPos.x < m_gridSize.x - 1)
            {
                //Right
                neighbours.Add(GetNeighbour(node, new Vector2Int(1, 0)));
	        
                //Right Down
                if(node.m_gridPos.y > 0)
                    neighbours.Add(GetNeighbour(node, new Vector2Int(1, -1)));
	        
                //Right Up
                if(node.m_gridPos.y < m_gridSize.y - 1)
                    neighbours.Add(GetNeighbour(node, new Vector2Int(1, 1)));
            }
	    
            //Down
            if(node.m_gridPos.y > 0)
                neighbours.Add(GetNeighbour(node, new Vector2Int(0, -1)));
	    
            //Up
            if(node.m_gridPos.y < m_gridSize.y - 1)
                neighbours.Add(GetNeighbour(node, new Vector2Int(0, 1)));
	    
            return neighbours;
        }
		
        private void CalcGridSize()
        {
            m_gridSize.x = Mathf.RoundToInt(m_gridWorldSize.x/ NodeDiameter);
            m_gridSize.y = Mathf.RoundToInt(m_gridWorldSize.y/ NodeDiameter);
        }
        
        protected virtual void OnNodeCreated(TNode node)
        {
			
        }

        protected Vector3 GetWorldPointFromGridPoint(int x, int y)
        {
            return transform.position + Vector3.right * (x * NodeDiameter + m_nodeRadius) + Vector3.forward * (y * NodeDiameter + m_nodeRadius);
        }
        
        private void OnDrawGizmos()
        {
            if(!m_debug)
                return;
        
            CalcGridSize();

            Gizmos.color = m_debugTileColor;
        
            float bias = 0.02f;

            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    Vector3 worldPoint = GetWorldPointFromGridPoint(x, y);
                
                    Gizmos.DrawCube(worldPoint, new Vector3(NodeDiameter - bias, 0.1f, NodeDiameter - bias));
                }
            }
        
            Gizmos.color = Color.white;
        }
    }
}