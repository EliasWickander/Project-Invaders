using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Client_OnPlayerKilledEvent", menuName = "Custom/Game Events/Client/OnPlayerKilledEvent")]
public class Client_OnPlayerKilledEvent : GameEvent<OnPlayerKilledGameEventData> { }