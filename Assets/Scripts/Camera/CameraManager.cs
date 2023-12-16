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
	private CinemachineVirtualCamera m_defaultCamera;

	[SerializeField]
	private CinemachineVirtualCamera m_playerCamera;

	[SerializeField]
	private List<CinemachineVirtualCamera> m_virtualCameras = new List<CinemachineVirtualCamera>();
	public Camera Camera { get; private set; }
	public CinemachineVirtualCamera ActiveVirtualCamera { get; private set; }

	protected override void OnSingletonAwake()
	{
		base.OnSingletonAwake();

		foreach(CinemachineVirtualCamera virtualCamera in m_virtualCameras)
			virtualCamera.gameObject.SetActive(false);

		Camera = Camera.current;

		if(m_defaultCamera != null)
			ActivateCamera(m_defaultCamera);
	}

	private void OnEnable()
	{
		m_cinemachineBrain.m_CameraActivatedEvent.AddListener(OnCameraActivated);
	}

	private void OnDisable()
	{
		m_cinemachineBrain.m_CameraActivatedEvent.RemoveListener(OnCameraActivated);
	}

	public void ActivateCamera(ActivateCameraEventData eventData)
	{
		if (eventData.m_cameraToActivate == null)
		{
			Debug.LogError("Attempted to activate camera but camera to activate was null");
			return;
		}

		ActivateCamera(eventData.m_cameraToActivate);
	}

	public void ActivateCamera(CinemachineVirtualCamera cameraToActivate)
	{
		if(!m_virtualCameras.Contains(cameraToActivate))
			m_virtualCameras.Add(cameraToActivate);

		if (ActiveVirtualCamera == cameraToActivate)
		{
			Debug.LogWarning("Attempting to activate camera that was already active");
			return;
		}

		if(ActiveVirtualCamera != null)
			ActiveVirtualCamera.gameObject.SetActive(false);

		cameraToActivate.gameObject.SetActive(true);
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

		ActivateCamera(m_playerCamera);
	}

	public void OnPlayerSpawned(OnPlayerSpawnedGameEventData data)
	{
		Player spawnedPlayer = data.m_player;
		if (spawnedPlayer == GameWorld.LocalPlayer && ActiveVirtualCamera != m_playerCamera)
			SetupPlayerCamera(spawnedPlayer);
	}

	private void OnCameraActivated(ICinemachineCamera newCamera, ICinemachineCamera oldCamera)
	{
		if (newCamera is CinemachineVirtualCamera newVirtualCamera)
			ActiveVirtualCamera = newVirtualCamera;
		else
			ActiveVirtualCamera = null;
	}
}
