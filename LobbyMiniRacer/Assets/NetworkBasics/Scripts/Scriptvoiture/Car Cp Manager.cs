using Mirror;
using UnityEngine;
using System.Collections;

public class CarCpManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCpCrossedChanged))]
    public int cpCrossed = 0;

    [SyncVar]
    public int CarNumber;

    [SyncVar(hook = nameof(OnCarPositionChanged))]
    public int CarPosition;
    public RaceManager raceManager;

    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(AssignRaceManager());
    }

    private IEnumerator AssignRaceManager()
    {
        while (raceManager == null)
        {
            raceManager = Object.FindFirstObjectByType<RaceManager>();

            if (raceManager == null)
            {
                Debug.LogWarning("RaceManager non encore trouvé, nouvelle tentative...");
                yield return new WaitForSeconds(0.5f);
            }
        }

        Debug.Log("RaceManager assigné avec succès !");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return; // S'assure que seul le joueur local gère cette logique

        if (other.gameObject.CompareTag("CP"))
        {
            CmdIncreaseCpCrossed();
        }
    }

    [Command]
    private void CmdIncreaseCpCrossed()
    {
        cpCrossed += 1;
        if (raceManager != null)
        {
            Debug.Log($"Car {CarNumber} a atteint le checkpoint {cpCrossed}");
            raceManager.CarCollectedCp(CarNumber, cpCrossed);
        }
        else
        {
            Debug.LogError("RaceManager est NULL, impossible d'appeler CarCollectedCp !");
        }
    }

    private void OnCarPositionChanged(int oldValue, int newValue)
    {
        CarPosition = newValue;
        Debug.Log($"Car {CarNumber} position updated to {newValue}");
    }

    [Command]
    public void CmdUpdateCarPosition(int newPosition)
    {
        CarPosition = newPosition;
    }

    private void OnCpCrossedChanged(int oldValue, int newValue)
    {
        cpCrossed = newValue;
        Debug.Log($"Checkpoint count updated to {newValue} for Car {CarNumber}");
    }
}