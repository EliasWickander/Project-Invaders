using CustomToolkit.AdvancedTypes;
using UnityEngine;

public class PlayArea : NetworkedWorldGrid<WorldGridTile>
{
	public static PlayArea Instance { get; private set; }
	
    protected override void Awake()
    {
        Instance = this;
        
        base.Awake();
    }
    
    protected override void OnNodeCreated(WorldGridTile node)
    {
	    base.OnNodeCreated(node);
	    
	    float bias = 0.02f;

	    Transform nodeTransform = node.transform;
	    nodeTransform.localScale = new Vector3(NodeDiameter - bias, nodeTransform.localScale.y, NodeDiameter - bias);
	    nodeTransform.SetParent(transform);
	    
    }
}
