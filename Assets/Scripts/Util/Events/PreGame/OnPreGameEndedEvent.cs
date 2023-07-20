using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "OnPreGameEnded", menuName = "Custom/NetworkEvents/OnPreGameEndedEvent")]
public class OnPreGameEndedEvent : NetworkGameEvent<OnPreGameEndedEventData>
{

}
