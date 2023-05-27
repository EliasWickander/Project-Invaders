using System;
using System.Collections;
using System.Collections.Generic;
using CustomToolkit.UI;
using TMPro;
using UnityEngine;

public class PlayerNameInput : MonoBehaviour
{
    [SerializeField] 
    private TMP_InputField m_nameInputField;

    public const string c_playerPrefsDisplayNameKey = "DisplayName";

    private void Awake()
    {
        string savedDisplayName = PlayerPrefs.GetString(c_playerPrefsDisplayNameKey, "Random Name");

        m_nameInputField.text = savedDisplayName;
    }

    private void OnEnable()
    {
        m_nameInputField.onValueChanged.AddListener(OnDisplayNameChanged);
    }

    private void OnDisable()
    {
        m_nameInputField.onValueChanged.RemoveListener(OnDisplayNameChanged);
    }
    
    private void SavePlayerName()
    {
        string displayName = m_nameInputField.text;
        
        PlayerPrefs.SetString(c_playerPrefsDisplayNameKey, displayName);
    }
    
    private void OnDisplayNameChanged(string newName)
    {
        SavePlayerName();
    }
}
