using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MirrorBasics
{
    public class UILobby : MonoBehaviour
    {
        public static UILobby instance;

        [Header("Host Join")]
        [SerializeField] InputField JoinMatchInput;
        [SerializeField] List<Selectable> lobbySelectables = new List<Selectable>();
        [SerializeField] Canvas lobbyCanvas;
        [SerializeReference] Canvas searchCanvas;

        [Header("Lobby")]
        [SerializeField] Transform UIPlayerParent;
        [SerializeField] GameObject UIPlayerPrefab;
        [SerializeField] Text matchIDText;
        [SerializeField] GameObject beginGameButton;

        GameObject playerLobbyUI;

        bool searching = false;

        void Start()
        {
            instance = this;
        }
        public void HostPrivate()
        {
            JoinMatchInput.interactable = false;
            lobbySelectables.ForEach(x => x.interactable = false);

            Player.localPlayer.HostGame(false);
        }

        public void HostPublic()
        {
            JoinMatchInput.interactable = false;
            lobbySelectables.ForEach(x => x.interactable = false);

            Player.localPlayer.HostGame(true);
        }

        public void HostSuccess(bool Success, string matchID)
        {
            if (Success)
            {
                lobbyCanvas.enabled = true;

                if (playerLobbyUI != null) Destroy (playerLobbyUI);
                playerLobbyUI = SpawnPlayerUIPrefab(Player.localPlayer);
                matchIDText.text = matchID;
                beginGameButton.SetActive(true);
            }
            else
            {
                JoinMatchInput.interactable = true;
                lobbySelectables.ForEach(x => x.interactable = true);
            }

        }

        public void Join()
        {
            JoinMatchInput.interactable = false;
            lobbySelectables.ForEach(x => x.interactable = false);

            Player.localPlayer.JoinGame(JoinMatchInput.text.ToUpper());
        }

        public void JoinSuccess(bool Success, string matchID)
        {
            if (Success)
            {
                lobbyCanvas.enabled = true;
                beginGameButton.SetActive(false);

                if (playerLobbyUI != null) Destroy (playerLobbyUI);
                playerLobbyUI = SpawnPlayerUIPrefab(Player.localPlayer);
                matchIDText.text = matchID;
            }
            else
            {
                JoinMatchInput.interactable = true;
                lobbySelectables.ForEach(x => x.interactable = true);
            }
        }

        public GameObject SpawnPlayerUIPrefab(Player player)
        {
            GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
            newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
            newUIPlayer.transform.SetSiblingIndex(player.playerIndex - 1);
            return newUIPlayer;
        }

        public void BeginGame()
        {
            Player.localPlayer.BeginGame();
        }

        public void SearchGame()
        {
            Debug.Log($"Searching for game");
            searchCanvas.enabled = true;
            StartCoroutine(SearchingForGame());
        }

        IEnumerator SearchingForGame()
        {
            searching = true;

            float currentTime = 1;
            while (searching)
            {
                if (currentTime > 0)
                {
                    currentTime -= Time.deltaTime;
                }
                else
                {
                    currentTime = 1;
                    Player.localPlayer.SearchGame();
                }
                yield return null;
            }
        }

        public void SearchSuccess(bool Success, string matchID)
        {
            if (Success)
            {
                searchCanvas.enabled = false;
                JoinSuccess(Success, matchID);
                searching = false;
            }
        }

        public void SearchCancel()
        {
            searchCanvas.enabled = false;
            searching = false;
            lobbySelectables.ForEach(x => x.interactable = true);
        }

        public void DisconnectLobby()
        {
            if (playerLobbyUI != null) Destroy (playerLobbyUI);
            Player.localPlayer.DisconnectGame();

            lobbyCanvas.enabled = false;
            lobbySelectables.ForEach(x => x.interactable = true);
            beginGameButton.SetActive(false);
        }
    }
}