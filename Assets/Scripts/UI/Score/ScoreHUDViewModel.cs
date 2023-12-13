using System;
using System.Collections.Generic;
using CustomToolkit.UnityMVVM;
using UnityEngine;

[Binding]
public class ScoreHUDViewModel : ViewModelMonoBehaviour
{
	private ObservableList<ScoreEntryViewModel> m_scoreEntries = new ObservableList<ScoreEntryViewModel>();

	[Binding]
	public ObservableList<ScoreEntryViewModel> ScoreEntries => m_scoreEntries;

	private Dictionary<Player, ScoreEntryViewModel> m_scoreEntryByPlayer = new Dictionary<Player, ScoreEntryViewModel>();

	private void Start()
	{
		GameClient gameClient = GameClient.Instance;

		if(gameClient == null)
			return;

		//On start add all existing players to score
		m_scoreEntries.Clear();

		foreach (Player player in gameClient.GameWorld.Players)
			AddPlayerToScore(player);
	}

	private void Update()
	{
		if(m_scoreEntries.Count <= 0)
			return;

		SortEntriesByScore();

		foreach (ScoreEntryViewModel scoreEntry in m_scoreEntries)
			scoreEntry.Update();
	}

	[Binding]
	public void AddPlayerToScore(OnPlayerJoinedGameEventData playerJoinedEventData)
	{
		AddPlayerToScore(playerJoinedEventData.m_player);
	}

	private void AddPlayerToScore(Player player)
	{
		if (m_scoreEntryByPlayer.ContainsKey(player))
		{
			Debug.LogError("Attempted to add player to score but existing score entry is already associated with this player. Something is wrong");
			return;
		}

		ScoreEntryViewModel newEntry = new ScoreEntryViewModel();
		newEntry.SetPlayer(player);

		m_scoreEntries.Add(newEntry);
		m_scoreEntryByPlayer.Add(player, newEntry);
	}

	[Binding]
	public void RemovePlayerFromScore(OnPlayerLeftGameEventData playerLeftEventData)
	{
		RemovePlayerFromScore(playerLeftEventData.m_player);
	}

	private void RemovePlayerFromScore(Player player)
	{
		if (!m_scoreEntryByPlayer.ContainsKey(player))
		{
			Debug.LogError("Attempted to remove player from score but no existing score entry is associated with this player. Something is wrong");
			return;
		}

		m_scoreEntries.Remove(m_scoreEntryByPlayer[player]);
		m_scoreEntryByPlayer.Remove(player);
	}

	private void SortEntriesByScore()
	{
		for (int i = 0; i < m_scoreEntries.Count - 1; i++)
		{
			for (int j = 0; j < m_scoreEntries.Count - i - 1; j++)
			{
				if (m_scoreEntries[j].Progress < m_scoreEntries[j + 1].Progress)
				{
					m_scoreEntries.Swap(j, j + 1);
				}
			}
		}
	}
}
