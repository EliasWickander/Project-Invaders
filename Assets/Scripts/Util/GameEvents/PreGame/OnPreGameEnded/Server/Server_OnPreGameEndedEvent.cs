using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnPreGameEndedEvent", menuName = "Custom/Game Events/Server/OnPreGameEndedEvent")]
public class Server_OnPreGameEndedEvent : GameEvent<OnPreGameEndedEventData> { }