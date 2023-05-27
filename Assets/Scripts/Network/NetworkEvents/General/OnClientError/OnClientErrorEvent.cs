using Mirror;
using UnityEngine;
using CustomToolkit.Events;
[CreateAssetMenu(fileName = "OnClientErrorEvent", menuName = "Custom/Events/Network/OnClientErrorEvent")]
public class OnClientErrorEvent : GameEvent<TransportError>
{

}
