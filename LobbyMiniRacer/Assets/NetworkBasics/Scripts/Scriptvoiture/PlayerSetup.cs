using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable; // Tableau des composants à désactiver
    private Camera sceneCamera;

    // Start est appelé une fois avant la première mise à jour
    void Start()
    {
        Debug.Log("PlayerSetup Start() appelé"); // Log pour suivre l'exécution

        if (!isLocalPlayer)
        {
            Debug.Log($"{gameObject.name} n'est pas le joueur local, désactivation des composants...");
            // Vérification de la taille du tableau et des éléments avant de désactiver
            if (componentsToDisable != null)
            {
                for (int i = 0; i < componentsToDisable.Length; i++)
                {
                    if (componentsToDisable[i] != null) // Vérifie si le composant n'est pas null
                    {
                        componentsToDisable[i].enabled = false;
                    }
                    else
                    {
                        Debug.LogWarning($"componentsToDisable[{i}] est null sur {gameObject.name}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Le tableau componentsToDisable est null.");
            }
        }
        else
        {
            Debug.Log($"{gameObject.name} est le joueur local, désactivation de la caméra de la scène...");
            sceneCamera = Camera.main; // Obtient la caméra principale

            if (sceneCamera == null)
            {
                Debug.LogWarning("Aucune caméra avec le tag 'MainCamera' trouvée dans la scène.");
            }
            else
            {
                sceneCamera.gameObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        Debug.Log($"{gameObject.name} est désactivé, réactivation de la caméra de la scène...");

        if (sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }
    }
}



