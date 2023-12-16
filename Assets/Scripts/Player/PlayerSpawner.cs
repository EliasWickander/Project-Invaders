using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private float m_respawnDuration = 2;

    [SerializeField]
    private Server_OnPlayerKilledEvent m_onPlayerKilledServerEvent;
    private PlayGrid m_playGrid;

    private void OnEnable()
    {
        if(m_onPlayerKilledServerEvent != null)
            m_onPlayerKilledServerEvent.RegisterListener(OnPlayerKilled);
    }

    private void OnDisable()
    {
        if(m_onPlayerKilledServerEvent != null)
            m_onPlayerKilledServerEvent.UnregisterListener(OnPlayerKilled);
    }

    private void Start()
    {
        m_playGrid = PlayGrid.Instance;
    }

    private void OnPlayerKilled(OnPlayerKilledGameEventData data)
    {
        StartCoroutine(SpawnPlayer(data.m_player, m_respawnDuration));
    }

    public void OnPlayerJoinedGame(OnPlayerJoinedGameEventData data)
    {
        Player player = data.m_player;
        Transform spawnTransform = player.SpawnTransform;
        WorldGridTile spawnTile = m_playGrid.GetNode(spawnTransform.position);

        player.OnSpawned(spawnTransform, spawnTile);
    }

    private IEnumerator SpawnPlayer(Player player, float delay)
    {
        if (delay <= 0)
        {
            SpawnPlayer(player);
            yield return null;
        }

        yield return new WaitForSeconds(delay);
        SpawnPlayer(player);
    }

    [Server]
    private void SpawnPlayer(Player player)
    {
        if(player == null)
            return;

        Transform spawnTransform = NetworkManagerCustom.Instance.GetStartPosition();
        Vector3 spawnPos = spawnTransform.position;

        WorldGridTile spawnTile = m_playGrid.GetNode(spawnPos);

        player.transform.position = spawnPos;

        player.OnSpawned(spawnTransform, spawnTile);
    }
}
