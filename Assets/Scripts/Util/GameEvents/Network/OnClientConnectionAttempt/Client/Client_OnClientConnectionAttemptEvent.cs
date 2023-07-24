using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Client_OnClientConnectionAttemptEvent", menuName = "Custom/Game Events/Client/OnClientConnectionAttemptEvent")]
public class Client_OnClientConnectionAttemptEvent : GameEvent<OnClientConnectionAttemptEventData> { }