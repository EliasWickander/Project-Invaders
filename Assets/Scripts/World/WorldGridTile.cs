using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGridTile : MonoBehaviour
{
    [SerializeField] 
    private MeshRenderer m_meshRenderer;

    public void UpdateMaterial(Material material)
    {
        m_meshRenderer.material = material;
    }
}
