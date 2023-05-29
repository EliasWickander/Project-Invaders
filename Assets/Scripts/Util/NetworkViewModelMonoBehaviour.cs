using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CustomToolkit.UnityMVVM;
using Mirror;
using UnityEngine;

public class NetworkViewModelMonoBehaviour : NetworkBehaviour, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
        
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
        
    protected void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        PropertyChanged?.Invoke(this, args);
    }
}
