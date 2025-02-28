using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable; // Tableau des composants à désactiver

    private Camera sceneCamera; // Référence à la caméra principale

    // Start est appelé une fois avant la première mise à jour
    void Start()
    {
        Debug.Log($"PlayerSetup Start() appelé pour {gameObject.name}");

        // Désactiver les composants pour les autres joueurs
        if (!isLocalPlayer)
        {
            Debug.Log($"{gameObject.name} n'est pas le joueur local, désactivation des composants...");

            if (componentsToDisable != null)
            {
                foreach (var component in componentsToDisable)
                {
                    if (component != null && component.enabled) // Vérifie si le composant est actif
                    {
                        component.enabled = false;
                    }
                    else
                    {
                        Debug.LogWarning($"Un composant dans componentsToDisable est null ou déjà désactivé sur {gameObject.name}");
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

            // Désactiver la caméra principale
            DisableSceneCamera();
        }
    }

    // Désactive la caméra principale
    private void DisableSceneCamera()
    {
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

    // Réactive la caméra principale lorsque l'objet est détruit
    private void OnDestroy()
    {
        Debug.Log($"{gameObject.name} est détruit, réactivation de la caméra de la scène...");

        if (sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }
    }
}