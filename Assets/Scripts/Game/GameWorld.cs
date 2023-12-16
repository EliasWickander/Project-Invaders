using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAddedEventArgs
{
    public Player m_player;
}

public class PlayerRemovedEventArgs
{
    public Player m_player;
}

public class GameWorld
{
    public Pathfinding Pathfinding { get; private set; }

    //Player id to player object
    private Dictionary<string, Player> PlayersDictionary { get; set; } = new Dictionary<string, Player>();
    public List<Player> Players { get; set; } = new List<Player>();

    public static Player LocalPlayer = null;
    public static event EventHandler<PlayerAddedEventArgs> OnPlayerAddedEvent;
    public static event EventHandler<PlayerRemovedEventArgs> OnPlayerRemovedEvent;

    public GameWorld()
    {
        Pathfinding = new Pathfinding();
    }

    public void AddPlayerToWorld(Player player)
    {
        if(player == null)
            return;

        if (PlayersDictionary.ContainsValue(player))
        {
            Debug.LogError($"Player {player} already added to world");
            return;
        }

        if (PlayersDictionary.ContainsKey(player.PlayerId))
        {
            Debug.LogError($"Player Id {player.PlayerId} is already associated with another player. Something is wrong");
            return;
        }

        Players.Add(player);
        PlayersDictionary.Add(player.PlayerId, player);

        if (player.isOwned)
	        LocalPlayer = player;

        OnPlayerAddedEvent?.Invoke(this, new PlayerAddedEventArgs() {m_player = player});
    }

    public void RemovePlayerFromWorld(Player player)
    {
        if(player == null)
            return;

        if (!PlayersDictionary.ContainsKey(player.PlayerId))
        {
            Debug.LogError($"Player {player} is not added to world. Something is wrong");
            return;
        }

        Players.Remove(player);
        PlayersDictionary.Remove(player.PlayerId);

        if (LocalPlayer == player)
	        LocalPlayer = null;

        OnPlayerRemovedEvent?.Invoke(this, new PlayerRemovedEventArgs() {m_player = player});
    }

    public Player GetPlayerFromId(string id)
    {
        if (!PlayersDictionary.ContainsKey(id))
        {
            Debug.LogError("No player associated with id " + id);
            return null;
        }

        return PlayersDictionary[id];
    }
}
