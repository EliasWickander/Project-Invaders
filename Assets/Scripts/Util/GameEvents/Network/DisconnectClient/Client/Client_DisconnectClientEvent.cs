using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Client_DisconnectClientEvent", menuName = "Custom/Game Events/Client/DisconnectClientEvent")]
public class Client_DisconnectClientEvent : GameEvent<DisconnectClientEventData> { }