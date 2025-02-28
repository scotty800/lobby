using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace MirrorBasics
{
    public class Player : NetworkBehaviour
    {
        public static Player localPlayer;

        [SyncVar] public string matchID;
        [SyncVar] public int playerIndex;
        [SyncVar] public Match currentMatch;

        [SyncVar(hook = nameof(OnCarSelected))]
        public string selectedCar;
        NetworkMatch networkMatch;

        GameObject PlayerLobbyUI;

        void Awake()
        {
            networkMatch = GetComponent<NetworkMatch>();
        }

        public override void OnStartClient()
        {
            if (isLocalPlayer)
            {
                localPlayer = this;
            }
            else
            {
                Debug.Log($"Spawning other player UI");
                PlayerLobbyUI = UILobby.instance.SpawnPlayerUIPrefab(this);
            }
        }

        public override void OnStopClient()
        {
            Debug.Log($"Client stopped");
            ClientDisconnect();
        }

        public override void OnStopServer()
        {
            Debug.Log($"Client stopped on server");
            ServerDisconnect();
        }

        /*
            HOST MATCH
        */
        public void HostGame(bool publicMatch)
        {
            string matchID = MatchMaker.GetRandomMatchID();
            CmdHostGame(matchID, publicMatch);
        }

        [Command]
        void CmdHostGame(string _matchID, bool publicMatch)
        {
            matchID = _matchID;
            if (MatchMaker.instance.HostGame(_matchID, GetComponent<NetworkIdentity>(), publicMatch, out playerIndex))
            {
                Debug.Log($"<color=green>Game hosted successfully</color>");
                networkMatch.matchId = _matchID.ToGuid();
                TargetHostGame(true, _matchID, playerIndex);
            }
            else
            {
                Debug.Log($"<color=red>Game hosting failed</color>");
                TargetHostGame(false, _matchID, playerIndex);
            }
        }

        [TargetRpc]
        void TargetHostGame(bool success, string _matchID, int _playerIndex)
        {
            playerIndex = _playerIndex;
            matchID = _matchID;
            Debug.Log($"MatchID {matchID} == {_matchID}");
            UILobby.instance.HostSuccess(success, _matchID);
        }

        /*
            JOIN MATCH
        */

        public void JoinGame(string _inputID)
        {
            CmdJoinGame(_inputID);
        }

        [Command]
        void CmdJoinGame(string _matchID)
        {
            matchID = _matchID;
            if (MatchMaker.instance.JoinGame(_matchID, GetComponent<NetworkIdentity>(), out playerIndex))
            {
                Debug.Log($"<color=green>Game hosted successfully</color>");
                networkMatch.matchId = _matchID.ToGuid();
                TargetJoinGame(true, _matchID, playerIndex);
            }
            else
            {
                Debug.Log($"<color=red>Game hosting failed</color>");
                TargetJoinGame(false, _matchID, playerIndex);
            }
        }

        [TargetRpc]
        void TargetJoinGame(bool success, string _matchID, int _playerIndex)
        {
            playerIndex = _playerIndex;
            matchID = _matchID;
            Debug.Log($"MatchID {matchID} == {_matchID}");
            UILobby.instance.JoinSuccess(success, _matchID);
        }

        /* 
            SEARCH MATCH
        */

        public void SearchGame()
        {
            CmdSearchGame();
        }

        [Command]
        void CmdSearchGame()
        {
            if (MatchMaker.instance.SearchGame(gameObject, out playerIndex, out matchID))
            {
                Debug.Log($"<color=green>Game Found</color>");
                networkMatch.matchId = matchID.ToGuid();
                TargetSearchGame(true, matchID, playerIndex);
            }
            else
            {
                Debug.Log($"<color=red>Game Not Found</color>");
                TargetSearchGame(false, matchID, playerIndex);
            }
        }

        [TargetRpc]
        public void TargetSearchGame(bool success, string _matchID, int _playerIndex)
        {
            playerIndex = _playerIndex;
            matchID = _matchID;
            Debug.Log($"MatchID {matchID} == {_matchID}");
            UILobby.instance.SearchSuccess(success, _matchID);
        }

        /*
            BEGIN MATCH
        */

        public void BeginGame()
        {
            CmdBeginGame();
        }

        [Command]
        void CmdBeginGame()
        {
            MatchMaker.instance.BeginGame(matchID);
            Debug.Log($"<color=red>Game Begining</color>");
        }

        public void StartGame()
        {
            TargetBeginGame();
        }

        [TargetRpc]
        void TargetBeginGame()
        {
            Debug.Log($"MatchID {matchID} | Beginning");

            // Trouver l'EventSystem existant et le désactiver avant de charger une nouvelle scène
            EventSystem existingEventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (existingEventSystem != null)
            {
                existingEventSystem.gameObject.SetActive(false);
                Debug.Log("EventSystem existant désactivé avant le chargement de la scène 2");
            }
            SceneManager.LoadScene(2, LoadSceneMode.Additive);
            Debug.Log("Scene 2 loaded");
        }

        /*
            DISCONNECT MATCH
        */

        public void DisconnectGame()
        {
            CmdDisconnectGame();
        }

        [Command]
        void CmdDisconnectGame()
        {
            ServerDisconnect();
        }

        void ServerDisconnect()
        {
            MatchMaker.instance.PlayerDisconnected(this, matchID);
            networkMatch.matchId = string.Empty.ToGuid();
            RpcDisconnectGame();
        }

        [ClientRpc]
        void RpcDisconnectGame()
        {
            ClientDisconnect();
        }

        void ClientDisconnect()
        {
            if (PlayerLobbyUI != null)
            {
                Destroy(PlayerLobbyUI);
            }
        }

        [Command]
        public void CmdSetSelectedCar(string carName)
        {
            selectedCar = carName;
        }

        void OnCarSelected(string oldCar, string newCar)
        {
            // Mettre à jour l'UI si nécessaire
        }

    }
}