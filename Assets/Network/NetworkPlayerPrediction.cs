using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NetworkPlayerInput
{
	public int m_tick;
	public Vector3 m_input;
}

public struct NetworkPlayerState
{
	public int m_tick;
	public Vector3 m_position;
}

public class NetworkPlayerPrediction : ClientPrediction
{
	
}
