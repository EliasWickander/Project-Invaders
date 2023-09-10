using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathNode
{
    public WorldGridTile m_tile;
    public PathNode Parent { get; set; }
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost => GCost + HCost;

    public PathNode(WorldGridTile tile)
    {
        m_tile = tile;
        GCost = 0;
        HCost = 0;
        Parent = null;
    }
}

public class Pathfinding
{
    /// <summary>
    /// Finds closest path between start to end node within area of nodes
    /// </summary>
    /// <param name="area">Area to traverse</param>
    /// <param name="startNode">Start node</param>
    /// <param name="endNode">End node</param>
    /// <returns></returns>
    public List<PathNode> FindPath(List<WorldGridTile> area, WorldGridTile startNode, WorldGridTile endNode)
    {
        Dictionary<WorldGridTile, PathNode> pathNodes = new Dictionary<WorldGridTile, PathNode>();

        foreach (WorldGridTile ownedTile in area)
        {
            pathNodes.Add(ownedTile, new PathNode(ownedTile));	
        }
		
        List<PathNode> openSet = new List<PathNode>();
        List<PathNode> closedSet = new List<PathNode>();

        PathNode startPfNode = new PathNode(startNode);
        PathNode endPfNode = new PathNode(endNode);
		
        openSet.Add(startPfNode);

        PathNode currentNode = startPfNode;

        bool pathFound = false;
        while (pathFound == false)
        {
            float minFCost = openSet.Min(x => x.FCost);
            currentNode = openSet.First(x => x.FCost == minFCost);

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (WorldGridTile neighbour in PlayGrid.Instance.GetNeighbours(currentNode.m_tile, false))
            {
                if (neighbour == endPfNode.m_tile)
                {
                    pathFound = true;
                    break;
                }
				
                if(!pathNodes.ContainsKey(neighbour))
                    continue;

                PathNode neighbourPfNode = pathNodes[neighbour];

                if(closedSet.Contains(neighbourPfNode))
                    continue;

                float tentativeGCost = currentNode.GCost + DistanceSquared(currentNode, neighbourPfNode);

                if (tentativeGCost < neighbourPfNode.GCost || !openSet.Contains(neighbourPfNode))
                {
                    neighbourPfNode.GCost = tentativeGCost;
                    neighbourPfNode.HCost = Heuristic(neighbourPfNode, endPfNode);
                    neighbourPfNode.Parent = currentNode;
					
                    if(!openSet.Contains(neighbourPfNode))
                        openSet.Add(neighbourPfNode);
                }
            }

            if (openSet.Count == 0)
            {
                Debug.Log("No path found");
                break;
            }
        }

        if (pathFound)
        {
            List<PathNode> path = new List<PathNode>();
            path.Add(endPfNode);
            
            while (currentNode != null)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        return null;
    }

    private float DistanceSquared(PathNode first, PathNode second)
    {
        return Mathf.Sqrt(Mathf.Pow(first.m_tile.m_gridPos.x - second.m_tile.m_gridPos.x, 2) + Mathf.Pow(first.m_tile.m_gridPos.y - second.m_tile.m_gridPos.y, 2));
    }

    private float Heuristic(PathNode first, PathNode second)
    {
        return Mathf.Abs(first.m_tile.m_gridPos.x - second.m_tile.m_gridPos.x) + Mathf.Abs(first.m_tile.m_gridPos.y - second.m_tile.m_gridPos.y);
    }
}