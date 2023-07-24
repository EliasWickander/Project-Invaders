using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Client_OnClientDisconnectedEvent", menuName = "Custom/Game Events/Client/OnClientDisconnectedEvent")]
public class Client_OnClientDisconnectedEvent : GameEvent<OnClientDisconnectedEventData> { }