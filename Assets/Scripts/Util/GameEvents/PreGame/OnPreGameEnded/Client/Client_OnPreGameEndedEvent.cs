using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Client_OnPreGameEndedEvent", menuName = "Custom/Game Events/Client/OnPreGameEndedEvent")]
public class Client_OnPreGameEndedEvent : GameEvent<OnPreGameEndedEventData> { }