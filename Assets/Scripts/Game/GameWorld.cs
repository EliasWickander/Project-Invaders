using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWorld
{
    public Dictionary<string, Player> Players { get; set; } = new Dictionary<string, Player>();

    public void AddPlayerToWorld(Player player)
    {
        if(player == null)
            return;
        
        if (Players.ContainsValue(player))
        {
            Debug.LogError($"Player {player} already added to world");
            return;
        }

        if (Players.ContainsKey(player.PlayerId))
        {
            Debug.LogError($"Player Id {player.PlayerId} is already associated with another player. Something is wrong");
            return;
        }

        Players.Add(player.PlayerId, player);
    }
    
    public Player GetPlayerFromId(string id)
    {
        if (!Players.ContainsKey(id))
        {
            Debug.LogError("No player associated with id " + id);
            return null;
        }

        return Players[id];
    }
}
