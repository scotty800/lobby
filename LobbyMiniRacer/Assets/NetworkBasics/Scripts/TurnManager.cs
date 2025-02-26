using UnityEngine;
using Mirror;
using System.Collections.Generic;

namespace MirrorBasics
{
    public class TurnManager : NetworkBehaviour
    {
        List<Player> players = new List<Player>();
        public void AddPlayer (Player _player)
        {
            players.Add(_player);
            Debug.Log($"Joueur ajouté au TurnManager : {_player.name}");
        }
    }
}
