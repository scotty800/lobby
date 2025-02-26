using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System; // Pour les listes normales
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace MirrorBasics
{
    [System.Serializable]
    public class Match
    {
        public  bool publicMatch;
        public bool inMatch;
        public bool matchFull;
        public string matchID;
        public List<uint> playerNetIDs = new List<uint>(); // On stocke les netId des joueurs

        public Match(string matchID, uint playerNetID)
        {
            this.matchID = matchID;
            playerNetIDs.Add(playerNetID);
        }

        public Match() { }
    }

    [System.Serializable]
    public class SyncListMatch : SyncList<Match> { }

    [System.Serializable]
    public class SyncListString : SyncList<string> { }

    public class MatchMaker : NetworkBehaviour
    {
        public static MatchMaker instance;

        public SyncListMatch matches = new SyncListMatch();
        public SyncListString MatchIDs = new SyncListString();

        [SerializeField] GameObject turnManagerPrefab;

        void Start()
        {
            instance = this;
        }

        public bool HostGame(string _matchID, NetworkIdentity _playerIdentity, bool publicMatch ,out int playerIndex)
        {
            playerIndex = -1;

            if (!MatchIDs.Contains(_matchID))
            {
                MatchIDs.Add(_matchID);
                Match match = new Match(_matchID, _playerIdentity.netId); // Stocker netId
                match.publicMatch = publicMatch;
                matches.Add(match);
                Debug.Log($"Match generated");
                _playerIdentity.GetComponent<Player>().currentMatch = match;
                playerIndex = 1;
                return true;
            }
            else
            {
                Debug.Log($"Match ID already exists");
                return false;
            }
        }

        public bool JoinGame(string _matchID, NetworkIdentity _playerIdentity, out int playerIndex)
        {
            playerIndex = -1;

            if (MatchIDs.Contains(_matchID))
            {
                for (int i = 0; i < matches.Count; i++)
                {
                    if (matches[i].matchID == _matchID)
                    {
                        matches[i].playerNetIDs.Add(_playerIdentity.netId);
                        playerIndex = matches[i].playerNetIDs.Count;
                        break;
                    }
                }
                Debug.Log($"Match Joined");
                return true;
            }
            else
            {
                Debug.Log($"Match ID does not exists");
                return false;
            }
        }

        public bool SearchGame(GameObject _player, out int playerIndex, out string matchID)
        {
            playerIndex = - 1;
            matchID = string.Empty;

            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].publicMatch && !matches[i].matchFull && !matches[i].inMatch)
                {
                    matchID = matches[i].matchID;
                    if (JoinGame (matchID, _player.GetComponent<NetworkIdentity>(), out playerIndex))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void BeginGame(string _matchID)
        {
            GameObject newTurnManager = Instantiate(turnManagerPrefab);
            newTurnManager.GetComponent<NetworkMatch>().matchId = _matchID.ToGuid();

            NetworkServer.Spawn(newTurnManager);
            TurnManager turnManager = newTurnManager.GetComponent<TurnManager>();

            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].matchID == _matchID)
                {
                    foreach (var player in matches[i].playerNetIDs)
                    {
                        Debug.Log($"Tentative de récupération du joueur avec NetID: {player}");
                        if (NetworkServer.spawned.TryGetValue(player, out NetworkIdentity playerIdentity))
                        {
                            Player _player = playerIdentity.GetComponent<Player>();
                            Debug.Log($"Joueur trouvé : {_player.name} (NetID: {player})");

                            turnManager.AddPlayer(_player);
                            _player.StartGame();
                        }
                        else
                        {
                            Debug.LogWarning($"Le joueur avec NetID {player} n'a pas été trouvé dans NetworkServer.spawned.");
                        }
                    }
                    break;
                }
            }
        }

        public static string GetRandomMatchID()
        {
            string _id = string.Empty;
            for (int i = 0; i < 5; i++)
            {
                int random = UnityEngine.Random.Range(0, 36); // Correction ici
                if (random < 26)
                {
                    _id += (char)(random + 65);
                }
                else
                {
                    _id += (random - 26).ToString();
                }
            }
            Debug.Log($"Random Match ID: {_id}");
            return _id;
        }

        public void PlayerDisconnected(Player player, string _matchID)
        {
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].matchID == _matchID)
                {
                    int playerIndex = matches[i].playerNetIDs.IndexOf(player.netId);
                    matches[i].playerNetIDs.RemoveAt(playerIndex);
                    Debug.Log($"Player disconnected from match {_matchID} | {matches[i].playerNetIDs.Count} players remaining");

                    if (matches[i].playerNetIDs.Count == 0)
                    {
                        Debug.Log($"No more players in Match. Terminating {_matchID}");
                        matches.RemoveAt(i);
                        MatchIDs.Remove (_matchID);
                    }
                    break;
                }
            }
            
        }
    }

    public static class MatchExtentions
    {
        public static Guid ToGuid(this string id)
        {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] inputBytes = Encoding.Default.GetBytes(id);
            byte[] hashBytes = provider.ComputeHash(inputBytes);

            return new Guid(hashBytes);
        }
    }
}