using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject[] carPrefabs;
    private string selectedCar;

    void Start()
    {
        selectedCar = PlayerPrefs.GetString("SelectedCar", "");

        Transform spawnPoint = GameObject.FindWithTag("SpawnPoint").transform;

        foreach (GameObject car in carPrefabs)
        {
            if (car.name == selectedCar)
            {
                Instantiate(car, spawnPoint.position, spawnPoint.rotation);
                break;
            }
        }
    }
}
