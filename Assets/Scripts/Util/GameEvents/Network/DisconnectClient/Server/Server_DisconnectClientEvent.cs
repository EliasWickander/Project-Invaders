using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_DisconnectClientEvent", menuName = "Custom/Game Events/Server/DisconnectClientEvent")]
public class Server_DisconnectClientEvent : GameEvent<DisconnectClientEventData> { }