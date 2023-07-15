using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Data/Game Data", fileName = "New Game Data")]
public class GameData : ScriptableObject
{
    [SerializeField] 
    private ElementData[] m_elements;

    public ElementData[] Elements => m_elements;
}
