using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "OnServerDisconnectedEvent", menuName = "Custom/Events/Network/OnServerDisconnectedEvent")]
public class OnServerDisconnectedEvent : GameEvent<OnServerDisconnectedEventData> { }
