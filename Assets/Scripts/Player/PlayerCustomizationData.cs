using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Customization Data", menuName = "Custom/Data/Player Customization Data")]
public class PlayerCustomizationData : ScriptableObject
{
    [SerializeField]
    private Material m_trailMaterial;
    public Material TrailMaterial => m_trailMaterial;
    
    [SerializeField]
    private Material m_territoryMaterial;
    public Material TerritoryMaterial => m_territoryMaterial;
}
