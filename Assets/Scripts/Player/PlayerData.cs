using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Custom/Data/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Trail")]
    [SerializeField]
    private Material m_trailMaterial;
    public Material TrailMaterial => m_trailMaterial;

    [Header("Territory")]
    [SerializeField]
    private int m_startTerritoryRadius = 2;
    public int StartTerritoryRadius => m_startTerritoryRadius;

    [SerializeField]
    private Material m_territoryMaterial;
    public Material TerritoryMaterial => m_territoryMaterial;



    [Header("General")]
    [SerializeField]
    private float m_moveSpeed = 1;
    public float MoveSpeed => m_moveSpeed;
}
