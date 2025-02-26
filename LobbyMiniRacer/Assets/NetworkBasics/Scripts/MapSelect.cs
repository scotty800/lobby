using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections.Generic;

public class MenuManager : NetworkBehaviour
{
    public GameObject panelSelectCircuit;
    public GameObject panelSelectCategory;

    public Button buttonNext;
    public Button buttonPlay;

    public List<GameObject> carPanels; // Liste des panneaux de voiture (à assigner dans l'Inspector)

    [SyncVar(hook = nameof(OnCircuitSelected))]
    private string selectedCircuit;
    [SyncVar(hook = nameof(OnCarSelected))]
    private string selectedCar;

    [SyncVar(hook = nameof(OnCarPanelActiveChanged))]
    private int activeCarPanelIndex = -1; // Index du panneau de voiture actif

    private bool allPlayersReady = false;

    void Start()
    {
        buttonNext.interactable = false;
        buttonPlay.interactable = false;
        panelSelectCircuit.SetActive(false);
        panelSelectCategory.SetActive(false);

        // Désactiver tous les panneaux de voiture au démarrage
        foreach (var panel in carPanels)
        {
            panel.SetActive(false);
        }
    }

    // Sélection du circuit
    public void SelectCircuit(string circuitName)
    {
        if (!isServer) return;
        selectedCircuit = circuitName;
        buttonNext.interactable = true;
    }

    public void GoToCategorySelection()
    {
        if (!isServer) return;
        panelSelectCircuit.SetActive(false);
        panelSelectCategory.SetActive(true);
        RpcGoToCategorySelection();
    }

    [ClientRpc]
    void RpcGoToCategorySelection()
    {
        panelSelectCircuit.SetActive(false);
        panelSelectCategory.SetActive(true);
    }

    // Afficher le panneau de sélection de voiture
    public void ShowCarPanel(int carPanelIndex)
    {
        if (!isServer) return;

        if (carPanelIndex < 0 || carPanelIndex >= carPanels.Count)
        {
            Debug.LogError("Invalid carPanelIndex on the server.");
            return;
        }

        panelSelectCategory.SetActive(false);
        activeCarPanelIndex = carPanelIndex; // Mettre à jour l'index du panneau actif
        RpcShowCarPanel(carPanelIndex);
    }

    [ClientRpc]
    void RpcShowCarPanel(int carPanelIndex)
    {
        if (carPanelIndex < 0 || carPanelIndex >= carPanels.Count)
        {
            Debug.LogError("Invalid carPanelIndex on the client.");
            return;
        }

        panelSelectCategory.SetActive(false);
        carPanels[carPanelIndex].SetActive(true);
    }

    // Sélection de la voiture
    public void SelectCar(string carName)
    {
        selectedCar = carName;
        PlayerPrefs.SetString("SelectedCar", carName);
        CmdPlayerReady();
    }

    [Command]
    void CmdPlayerReady()
    {
        // Vérifier si tous les joueurs sont prêts
        allPlayersReady = true; // Remplacer par une logique réelle pour vérifier si tous les joueurs sont prêts
        if (allPlayersReady)
        {
            buttonPlay.interactable = true;
            RpcEnablePlayButton();
        }
    }

    [ClientRpc]
    void RpcEnablePlayButton()
    {
        buttonPlay.interactable = true;
    }

    // Lancer le jeu
    public void PlayGame()
    {
        if (!isServer) return;
        if (!string.IsNullOrEmpty(selectedCircuit) && !string.IsNullOrEmpty(selectedCar))
        {
            PlayerPrefs.SetString("SelectedCircuit", selectedCircuit);
            NetworkManager.singleton.ServerChangeScene(selectedCircuit);
            RpcLoadGameScene(selectedCircuit);
        }
    }

    [ClientRpc]
    void RpcLoadGameScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Hooks pour les SyncVars
    void OnCircuitSelected(string oldValue, string newValue)
    {
        selectedCircuit = newValue;
    }

    void OnCarSelected(string oldValue, string newValue)
    {
        selectedCar = newValue;
    }

    void OnCarPanelActiveChanged(int oldIndex, int newIndex)
    {
        if (oldIndex >= 0 && oldIndex < carPanels.Count)
            carPanels[oldIndex].SetActive(false);

        if (newIndex >= 0 && newIndex < carPanels.Count)
            carPanels[newIndex].SetActive(true);
    }

    public void OnCarPanelButtonClicked(int carPanelIndex)
    {
        ShowCarPanel(carPanelIndex);
    }
}