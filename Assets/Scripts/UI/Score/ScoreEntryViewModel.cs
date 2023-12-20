using System.Collections.Generic;
using System.ComponentModel;
using CustomToolkit.UnityMVVM;
using UnityEngine;

[Binding]
public class ScoreEntryViewModel : ViewModel
{
	private Player m_player;

	private PropertyChangedEventArgs m_playerNameProp = new PropertyChangedEventArgs(nameof(PlayerName));
	private string m_playerName = string.Empty;

	[Binding]
	public string PlayerName
	{
		get
		{
			return m_playerName;
		}
		set
		{
			if (m_playerName != value)
			{
				m_playerName = value;
				OnPropertyChanged(m_playerNameProp);
			}
		}
	}

	private PropertyChangedEventArgs m_progressProp = new PropertyChangedEventArgs(nameof(Progress));
	private float m_progress = 0.0f;

	[Binding]
	public float Progress
	{
		get
		{
			return m_progress;
		}
		set
		{
			if (m_progress != value)
			{
				m_progress = value;
				OnPropertyChanged(m_progressProp);
				OnPropertyChanged(m_progressDisplayTextProp);
			}
		}
	}
	private PropertyChangedEventArgs m_progressDisplayTextProp = new PropertyChangedEventArgs(nameof(ProgressDisplayText));

	[Binding]
	public string ProgressDisplayText => $"{Mathf.Clamp(Mathf.FloorToInt(m_progress * 100), 1, 100)}%";

	private PropertyChangedEventArgs m_isPlayerDeadProp = new PropertyChangedEventArgs(nameof(IsPlayerDead));
	private bool m_isPlayerDead = false;

	[Binding]
	public bool IsPlayerDead
	{
		get
		{
			return m_isPlayerDead;
		}
		set
		{
			if (m_isPlayerDead != value)
			{
				m_isPlayerDead = value;
				OnPropertyChanged(m_isPlayerDeadProp);
			}
		}
	}

	public void SetPlayer(Player player)
	{
		PlayerName = player.DisplayName;

		m_player = player;
	}

	public void Update()
	{
		if(m_player == null)
			return;

		IsPlayerDead = m_player.IsDead;

		UpdateTileProgress();
	}

	private void UpdateTileProgress()
	{
		if (IsPlayerDead)
		{
			Progress = 0.0f;
			return;
		}

		TileManager tileManager = TileManager.Instance;
		PlayGrid playGrid = PlayGrid.Instance;

		if(tileManager == null || playGrid == null)
			return;

		List<WorldGridTile> ownedTiles = tileManager.GetOwnedTiles(m_player.PlayerId);

		Progress = ownedTiles != null ? ownedTiles.Count / (float)playGrid.AmountTiles : 0.0f;
	}
}
