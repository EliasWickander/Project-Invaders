using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Data/Element Data", fileName = "New Element Data")]
public class ElementData : ScriptableObject
{
    [SerializeField] 
    private Sprite m_icon;

    public Sprite Icon => m_icon;

    [SerializeField] 
    private bool m_isLocked = false;

    public bool IsLocked => m_isLocked;
}
