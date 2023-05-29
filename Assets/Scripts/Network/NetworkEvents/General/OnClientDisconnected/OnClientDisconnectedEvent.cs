using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "OnClientDisconnected", menuName = "Custom/Events/Network/OnClientDisconnectedEvent")]
public class OnClientDisconnectedEvent : GameEvent<OnClientDisconnectedEventData> 
{
}
