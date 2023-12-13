using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Client_OnPlayerLeftGameEvent", menuName = "Custom/Game Events/Client/OnPlayerLeftGameEvent")]
public class Client_OnPlayerLeftGameEvent : GameEvent<OnPlayerLeftGameEventData> { }