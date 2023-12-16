using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using CustomToolkit.AdvancedTypes;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
	[SerializeField]
	private CinemachineBrain m_cinemachineBrain;

	[SerializeField]
	private CinemachineVirtualCamera m_playerCamera;

	public Camera Camera { get; private set; }

	protected override void OnSingletonAwake()
	{
		base.OnSingletonAwake();

		Camera = Camera.current;
	}

	private void SetupPlayerCamera(Player player)
	{
		if (m_playerCamera == null)
		{
			Debug.LogError("Attempted to setup player camera, but player camera is null");
			return;
		}

		if (player == null)
		{
			Debug.LogError("Attempted to setup player camera, but player is null");
			return;
		}

		Transform playerTransform = player.transform;
		m_playerCamera.Follow = playerTransform;
		m_playerCamera.LookAt = playerTransform;
	}

	public void OnPlayerSpawned(OnPlayerSpawnedGameEventData data)
	{
		Player spawnedPlayer = data.m_player;
		if (spawnedPlayer == GameWorld.LocalPlayer)
			SetupPlayerCamera(spawnedPlayer);
	}
}
