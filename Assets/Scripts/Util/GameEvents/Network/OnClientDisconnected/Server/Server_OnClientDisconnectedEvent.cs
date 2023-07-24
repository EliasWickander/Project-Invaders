using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnClientDisconnectedEvent", menuName = "Custom/Game Events/Server/OnClientDisconnectedEvent")]
public class Server_OnClientDisconnectedEvent : GameEvent<OnClientDisconnectedEventData> { }