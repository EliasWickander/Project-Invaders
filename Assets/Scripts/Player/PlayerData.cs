using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Custom/Data/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Territory")]
    [SerializeField]
    private int m_startTerritoryRadius = 2;
    public int StartTerritoryRadius => m_startTerritoryRadius;

    [Header("General")]
    [SerializeField]
    private float m_moveSpeed = 1;
    public float MoveSpeed => m_moveSpeed;
}
