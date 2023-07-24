using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnClientConnectedEvent", menuName = "Custom/Game Events/Server/OnClientConnectedEvent")]
public class Server_OnClientConnectedEvent : GameEvent<OnClientConnectedEventData> { }