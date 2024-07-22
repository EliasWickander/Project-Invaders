using System.Collections;
using System.Collections.Generic;
using CustomToolkit.UnityMVVM;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Data/Element Data", fileName = "New Element Data")]
[Binding]
public class ElementData : ViewModelScriptableObject
{
    [SerializeField] 
    private string m_id;
    public string Id => m_id;
    
    [SerializeField] 
    private Sprite m_icon;

    [Binding]
    public Sprite Icon => m_icon;

    [SerializeField] 
    private PlayerCustomizationData m_playerCustomizationData;

    public PlayerCustomizationData PlayerCustomizationData => m_playerCustomizationData;
    
    [SerializeField] 
    private bool m_isLocked = false;

    [Binding]
    public bool IsLocked => m_isLocked;
}
