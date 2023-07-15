using System;
using System.ComponentModel;
using CustomToolkit.UnityMVVM;
using UnityEngine;

[Binding]
public class SelectableElementViewModel : ViewModel
{
    private ElementData m_data = null;

    public ElementData Data
    {
        get
        {
            return m_data;
        }
        set
        {
            m_data = value;
        }
    }
    
    private PropertyChangedEventArgs m_iconProp = new PropertyChangedEventArgs(nameof(Icon));
    private Sprite m_icon = null;

    [Binding]
    public Sprite Icon
    {
        get
        {
            return m_icon;
        }
        set
        {
            m_icon = value;
            OnPropertyChanged(m_iconProp);
        }
    }

    private PropertyChangedEventArgs m_isLockedProp = new PropertyChangedEventArgs(nameof(IsLocked));
    private bool m_isLocked = false;

    [Binding]
    public bool IsLocked
    {
        get
        {
            return m_isLocked;
        }
        set
        {
            m_isLocked = value;
            OnPropertyChanged(m_isLockedProp);
        }
    }
    public event Action<SelectableElementViewModel> OnSelected;
    
    [Binding]
    public void Select()
    {
        OnSelected?.Invoke(this);
    }
}