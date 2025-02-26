using UnityEngine;
using Mirror; // Import pour les fonctionnalités en ligne

public class CameraFollow : MonoBehaviour
{
    public float moveSmoothness = 0.1f;
    public float rotSmoothness = 0.1f;

    public Vector3 moveOffset;
    public Vector3 rotOffset;

    public Camera sceneCamera; // Caméra par défaut (vue générale)
    public Camera playCamera;  // Caméra pour suivre le joueur

    private Transform carTarget; // Cible dynamique (assignée automatiquement)
    public static CameraFollow Instance { get; private set; }
    private bool isPlayerInMap = false;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Évite d'avoir plusieurs instances actives
            return;
        }

        if (sceneCamera != null) sceneCamera.enabled = true;
        if (playCamera != null) playCamera.enabled = false;

        FindLocalPlayer();
    }
    void FixedUpdate()
    {
        if (carTarget == null)
        {
            FindLocalPlayer(); // Continue à chercher la voiture locale si non assignée
            return;
        }

        if (isPlayerInMap)
        {
            ActivatePlayCamera();
            followTarget();
        }
        else
        {
            ActivateSceneCamera();
        }
    }

    void followTarget()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        Vector3 targetPos = carTarget.TransformPoint(moveOffset);
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);
    }

    void HandleRotation()
    {
        Vector3 direction = carTarget.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotSmoothness * Time.deltaTime);
    }

    void ActivateSceneCamera()
    {
        if (sceneCamera != null && !sceneCamera.enabled)
        {
            sceneCamera.enabled = true;
            if (playCamera != null) playCamera.enabled = false;
        }
    }

    void ActivatePlayCamera()
    {
        if (playCamera != null && !playCamera.enabled)
        {
            playCamera.enabled = true;
            if (sceneCamera != null) sceneCamera.enabled = false;
        }
    }

    void FindLocalPlayer()
    {
        // Utilise FindObjectsByType pour récupérer les objets de type NetworkIdentity
        var players = Object.FindObjectsByType<NetworkIdentity>(FindObjectsSortMode.None);

        foreach (var player in players)
        {
            if (player.isLocalPlayer)
            {
                carTarget = player.transform; // Assigne le joueur local comme cible
                isPlayerInMap = true; // Suppose qu'il est dans la map au départ
                break;
            }
        }
    }

    public void SetPlayerInMap(bool inMap)
    {
        isPlayerInMap = inMap;
    }

    public void SetTarget(Transform target)
    {
        carTarget = target;
        isPlayerInMap = true; // Active la caméra de jeu si un joueur est assigné
    }
}

