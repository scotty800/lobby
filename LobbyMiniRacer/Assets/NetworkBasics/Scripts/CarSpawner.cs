using UnityEngine;
using Mirror;

public class CarSpawner : NetworkBehaviour
{
    public GameObject[] carPrefabs; // Liste des prefabs de voitures
    [SyncVar(hook = nameof(OnCarSelected))]
    private string selectedCar; // Voiture sélectionnée (synchronisée)

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Récupérer la voiture sélectionnée depuis PlayerPrefs (ou depuis le serveur)
        selectedCar = PlayerPrefs.GetString("SelectedCar", "");

        // Faire apparaître la voiture sélectionnée
        SpawnSelectedCar();
    }

    [Server]
    private void SpawnSelectedCar()
    {
        // Trouver le point de spawn
        Transform spawnPoint = GameObject.FindWithTag("SpawnPoint").transform;

        // Parcourir la liste des prefabs de voitures
        foreach (GameObject carPrefab in carPrefabs)
        {
            if (carPrefab.name == selectedCar)
            {
                // Instancier la voiture sur le serveur
                GameObject car = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);

                // Faire apparaître la voiture sur tous les clients
                NetworkServer.Spawn(car);

                break;
            }
        }
    }

    // Hook pour la synchronisation de selectedCar
    private void OnCarSelected(string oldCar, string newCar)
    {
        // Mettre à jour la voiture sélectionnée
        selectedCar = newCar;
    }
}