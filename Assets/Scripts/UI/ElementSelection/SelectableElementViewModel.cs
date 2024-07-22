using System;
using System.ComponentModel;
using CustomToolkit.UnityMVVM;
using UnityEngine;

[Binding]
public class SelectableElementViewModel : ViewModel
{
    private ElementData m_data = null;

    [Binding]
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
    
    public event Action<SelectableElementViewModel> OnSelected;
    
    [Binding]
    public void Select()
    {
        OnSelected?.Invoke(this);
    }
}