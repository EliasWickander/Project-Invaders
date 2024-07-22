using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CustomToolkit.UnityMVVM;
using UnityEngine;

[Binding]
public class ElementSelectionGridViewModel : ViewModelMonoBehaviour
{
    private PropertyChangedEventArgs m_selectableElementsProp = new PropertyChangedEventArgs(nameof(SelectableElements));
    private ObservableList<SelectableElementViewModel> m_selectableElements = new ObservableList<SelectableElementViewModel>();

    [Binding]
    public ObservableList<SelectableElementViewModel> SelectableElements
    {
        get
        {
            return m_selectableElements;
        }
        set
        {
            m_selectableElements = value;
            OnPropertyChanged(m_selectableElementsProp);
        }
    }

    private PropertyChangedEventArgs m_selectedElementProp = new PropertyChangedEventArgs(nameof(SelectedElement));
    private SelectableElementViewModel m_selectedElement = null;

    [Binding]
    public SelectableElementViewModel SelectedElement
    {
        get
        {
            return m_selectedElement;
        }
        set
        {
            m_selectedElement = value;
            OnPropertyChanged(m_selectedElementProp);
            OnPropertyChanged(m_hasSelectionProp);
        }
    }

    private PropertyChangedEventArgs m_hasSelectionProp = new PropertyChangedEventArgs(nameof(HasSelection));

    [Binding] 
    public bool HasSelection => SelectedElement != null;

    private void Start()
    {
        SpawnElements();
    }

    private void OnDestroy()
    {
        DespawnElements();
    }

    private void SpawnElements()
    {
        GameClient client = GameClient.Instance;

        if (client != null)
        {
            GameData gameData = client.GameData;

            for (int i = 0; i < gameData.Elements.Length; i++)
            {
                ElementData elementData = gameData.Elements[i];

                SpawnElementFromData(elementData);
            }
        }
        
        //First element is selected by default
        if(m_selectableElements.Count > 0)
            m_selectableElements[0].Select();
    }

    private void DespawnElements()
    {
        foreach (SelectableElementViewModel selectableElement in m_selectableElements)
            selectableElement.OnSelected -= OnElementSelected;

        m_selectableElements.Clear();
    }
    
    private void SpawnElementFromData(ElementData data)
    {
        SelectableElementViewModel selectableElementInstance = new SelectableElementViewModel();
        selectableElementInstance.Data = data;
        selectableElementInstance.OnSelected += OnElementSelected;
        
        SelectableElements.Add(selectableElementInstance);
    }

    private void OnElementSelected(SelectableElementViewModel selectedElement)
    {
        if (SelectedElement != selectedElement)
        {
            SelectedElement = selectedElement;
        }
    }
}
