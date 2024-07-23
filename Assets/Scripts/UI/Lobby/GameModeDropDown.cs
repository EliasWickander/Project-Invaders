using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameModeDropDown : MonoBehaviour
{
    [SerializeField] 
    private TMPro.TMP_Dropdown m_dropdown;

    [SerializeField] 
    private IntVariable m_selectedGameModeVariable;
    
    private void OnEnable()
    {
        m_dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDisable()
    {
        m_dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }

    private void Start()
    {
        SetupDropdown();
    }
    
    private void OnDropdownValueChanged(int index)
    {
        m_selectedGameModeVariable.Value = index;
    }

    private void SetupDropdown()
    {
        m_dropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();
        
        foreach(var gameMode in GameClient.Instance.GameData.GameModes)
            dropdownOptions.Add(new TMP_Dropdown.OptionData(gameMode.DisplayName));

        m_dropdown.AddOptions(dropdownOptions);
    }
}
