using UnityEngine;
using Mirror;
using System.Linq;
using System.Collections.Generic;

public class RaceManager : NetworkBehaviour
{
    public GameObject Cp;
    public GameObject CheckpointHolder;
    public GameObject[] Cars;
    public Transform[] CheckpointPositions;
    public GameObject[] checkpointForEachCar;
    private int totalCars;
    private int totalCheckpoints;
    public int nextCarNumber = 0; // Compteur pour attribuer un numéro unique aux voitures
    private Dictionary<GameObject, int> carNumbers = new Dictionary<GameObject, int>();

    // Déclaration d'une liste pour stocker le nombre de CP franchis pour chaque joueur
    private int[] cpCrossedArray;

    [SyncVar(hook = nameof(OnCheckpointPositionsChanged))]
    private Vector3[] checkpointPositions;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        totalCheckpoints = CheckpointHolder.transform.childCount;
        UpdateCarList();
        setCarPosition();
        setCheckpoints();

        if (isServer)
        {
            InitializeCheckpoints();
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCarList();
    }

    void UpdateCarList()
    {
        // Récupère **seulement** les objets avec le tag "Car"
        GameObject[] foundCars = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(obj => obj.CompareTag("Car"))
            .ToArray();

        // Mise à jour du tableau `Cars` uniquement si de nouvelles voitures apparaissent
        if (foundCars.Length != Cars.Length)
        {
            Cars = foundCars;
            totalCars = Cars.Length;
            AssignCarLayers();
            setCheckpoints();
        }
        // DEBUG : Voir quelles voitures sont détectées
        for (int i = 0; i < Cars.Length; i++)
        {
            Debug.Log($"Voiture {i}: {Cars[i].name} | Checkpoint Assigné: {checkpointForEachCar[i]?.name}");
        }
    }

    void AssignCarLayers()
    {
        for (int i = 0; i < Cars.Length; i++)
        {
            string layerName = "Car" + (i + 1); // Création du nom du layer (car1, car2, etc.)
            int layerIndex = LayerMask.NameToLayer(layerName); // Récupère l'index du layer dans Unity

            if (layerIndex == -1)
            {
                Debug.LogWarning($"Layer {layerName} n'existe pas dans les paramètres de Unity !");
            }
            else
            {
                if (!carNumbers.ContainsKey(Cars[i]))
                {
                    carNumbers[Cars[i]] = nextCarNumber;
                    CarCpManager carCpManager = Cars[i].GetComponent<CarCpManager>();
                    if (carCpManager != null)
                    {
                        carCpManager.CarNumber = nextCarNumber;
                        Debug.Log($" {Cars[i].name} reçoit CarNumber : {nextCarNumber}");
                    }
                    nextCarNumber++; //Incrémente seulement si une nouvelle voiture est détectée
                }
            }
        }
    }

    void setCheckpoints()
    {
        totalCars = Cars.Length;

        // Vérifier et récupérer les positions des checkpoints
        CheckpointPositions = new Transform[totalCheckpoints];
        for (int index = 0; index < totalCheckpoints; index++)
        {
            CheckpointPositions[index] = CheckpointHolder.transform.GetChild(index).transform;
        }

        // Trier les voitures par CarNumber pour garantir un bon ordre
        Cars = Cars.OrderBy(car => car.GetComponent<CarCpManager>().CarNumber).ToArray();

        // Réinitialiser les checkpoints
        if (checkpointForEachCar == null || checkpointForEachCar.Length != totalCars)
        {
            checkpointForEachCar = new GameObject[totalCars];
        }

        for (int i = 0; i < totalCars; i++)
        {
            CarCpManager carCpManager = Cars[i].GetComponent<CarCpManager>();
            if (carCpManager == null)
            {
                Debug.LogError($"Pas de CarCpManager trouvé sur {Cars[i].name}");
                continue;
            }

            int carNumber = carCpManager.CarNumber; // Récupérer le numéro unique de la voiture

            // Vérifier si un checkpoint existe déjà pour cette voiture et le supprimer
            if (checkpointForEachCar[carNumber] != null)
            {
                Destroy(checkpointForEachCar[carNumber]);
                checkpointForEachCar[carNumber] = null;
                Debug.Log($"Ancien checkpoint supprimé pour {Cars[i].name}");
            }

            // Vérifier qu'on a bien assez de checkpoints
            if (carNumber < CheckpointPositions.Length)
            {
                checkpointForEachCar[carNumber] = Instantiate(Cp, CheckpointPositions[carNumber].position, CheckpointPositions[carNumber].rotation);
                checkpointForEachCar[carNumber].name = "CP " + carNumber;
                checkpointForEachCar[carNumber].layer = LayerMask.NameToLayer("Car" + (carNumber + 1));

                Debug.Log($"Checkpoint {checkpointForEachCar[carNumber].name} assigné à {Cars[i].name} (Layer: Car{carNumber + 1})");
            }
            else
            {
                Debug.LogError($"Aucun checkpoint disponible pour la voiture {carNumber} !");
            }
        }
    }

    void setCarPosition()
    {
        for (int i = 0; i < totalCars; i++)
        {
            Cars[i].GetComponent<CarCpManager>().CarPosition = i + 1;
            Cars[i].GetComponent<CarCpManager>().CarNumber = i;
        }
    }

    void InitializeCheckpoints()
    {
        checkpointPositions = new Vector3[totalCheckpoints];
        for (int i = 0; i < totalCheckpoints; i++)
        {
            checkpointPositions[i] = CheckpointHolder.transform.GetChild(i).position;
        }
    }

    private void OnCheckpointPositionsChanged(Vector3[] oldPositions, Vector3[] newPositions)
    {
        for (int i = 0; i < newPositions.Length; i++)
        {
            checkpointForEachCar[i].transform.position = newPositions[i];
        }
    }

    [ClientRpc]
    public void RpcUpdateCheckpointPosition(int carNumber, int cpNumber, Vector3 position)
    {
        checkpointForEachCar[carNumber].transform.position = position;
    }

    public void CarCollectedCp(int carNumber, int cpNumber)
    {
        if (carNumber >= checkpointForEachCar.Length || cpNumber >= CheckpointPositions.Length)
        {
            Debug.LogError($"CarCollectedCp: Indice hors limites (carNumber={carNumber}, cpNumber={cpNumber})");
            return;
        }

        checkpointForEachCar[carNumber].transform.position = CheckpointPositions[cpNumber].transform.position;
        checkpointForEachCar[carNumber].transform.rotation = CheckpointPositions[cpNumber].transform.rotation;

        RpcUpdateCheckpointPosition(carNumber, cpNumber, CheckpointPositions[cpNumber].transform.position);

        Debug.Log($"Checkpoint de la voiture {carNumber} déplacé au CP {cpNumber}");

        comparePositions(carNumber);
    }

    void comparePositions(int carNumber)
    {
        if (Cars[carNumber].GetComponent<CarCpManager>().CarPosition > 1)
        {
            GameObject currentCar = Cars[carNumber];
            int currentCarPos = currentCar.GetComponent<CarCpManager>().CarPosition;
            int currentCarCp = currentCar.GetComponent<CarCpManager>().cpCrossed;

            GameObject carInFront = null;
            int carInfrontPos = 0;
            int carInFrontCp = 0;

            for (int i = 0; i < totalCars; i++)
            {
                if (Cars[i].GetComponent<CarCpManager>().CarPosition == currentCarPos - 1) // car in front
                {
                    carInFront = Cars[i];
                    carInFrontCp = carInFront.GetComponent<CarCpManager>().cpCrossed;
                    carInfrontPos = carInFront.GetComponent<CarCpManager>().CarPosition;
                    break;
                }
            }

            //this car has crossed the car in front
            if (currentCarCp > carInFrontCp)
            {
                int newPosition = currentCarPos - 1;
                currentCar.GetComponent<CarCpManager>().CarPosition = currentCarPos - 1;
                carInFront.GetComponent<CarCpManager>().CarPosition = carInfrontPos + 1;

                // Appeler la commande pour synchroniser la nouvelle position
                currentCar.GetComponent<CarCpManager>().CmdUpdateCarPosition(newPosition);
                carInFront.GetComponent<CarCpManager>().CmdUpdateCarPosition(carInfrontPos + 1);

                Debug.Log("Car " + carNumber + "has over taken" + carInFront.GetComponent<CarCpManager>().CarNumber);
            }
        }
    }
}
