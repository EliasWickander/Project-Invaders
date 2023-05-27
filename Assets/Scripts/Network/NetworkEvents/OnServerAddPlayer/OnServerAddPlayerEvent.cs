using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "OnServerAddPlayerEvent", menuName = "Custom/Events/Network/OnServerAddPlayerEvent")]
public class OnServerAddPlayerEvent : GameEvent<OnServerAddPlayerEventData> { }
