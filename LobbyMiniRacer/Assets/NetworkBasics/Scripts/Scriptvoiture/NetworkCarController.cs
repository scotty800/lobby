using Mirror;
using UnityEngine;

namespace MyCarController
{
    public class NetworkCarController : NetworkBehaviour
    {
        [SyncVar] private Vector3 syncedPosition;
        [SyncVar] private Quaternion syncedRotation;

        private Vector3 lastPosition;
        private Quaternion lastRotation;
        private float positionThreshold = 0.01f; // Très petit seuil pour détecter les changements
        private float rotationThreshold = 0.1f;  // Très petit seuil pour détecter les rotations
        private float syncRate = 0.1f;          // Fréquence plus modérée (10 fois par seconde)

        private float lastSyncTimeLocal; // Renommé pour éviter le conflit avec un champ parent

        private Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();

            if (!isLocalPlayer)
            {
                lastPosition = transform.position;
                lastRotation = transform.rotation;
            }
        }

        void Update()
        {
            if (isLocalPlayer)
            {
                // Vérifier si un changement significatif a eu lieu
                bool positionChanged = Vector3.Distance(lastPosition, transform.position) > positionThreshold;
                bool rotationChanged = Quaternion.Angle(lastRotation, transform.rotation) > rotationThreshold;

                // Vérifier la fréquence de synchronisation
                if ((positionChanged || rotationChanged) && Time.time - lastSyncTimeLocal >= syncRate)
                {
                    CmdUpdateCarTransform(transform.position, transform.rotation);
                    lastSyncTimeLocal = Time.time;
                    lastPosition = transform.position;
                    lastRotation = transform.rotation;
                }
            }
            else
            {
                // Mise à jour instantanée pour les autres clients
                InstantSync();
            }
        }

        [Command]
        void CmdUpdateCarTransform(Vector3 position, Quaternion rotation)
        {
            // Mettez à jour immédiatement les valeurs synchronisées
            syncedPosition = position;
            syncedRotation = rotation;
        }

        void InstantSync()
        {
            if (QuaternionIsValid(syncedRotation))
            {
                // Normalisation du quaternion avant de l'appliquer
                syncedRotation = Quaternion.Normalize(syncedRotation);

                // Appliquer directement la position et la rotation synchronisées
                transform.position = syncedPosition;
                transform.rotation = syncedRotation;

                // Appliquer au Rigidbody pour les collisions physiques
                if (rb != null)
                {
                    rb.MovePosition(syncedPosition);
                    rb.MoveRotation(syncedRotation);
                }
            }
        }

        bool QuaternionIsValid(Quaternion q)
        {
            return !(float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w));
        }
    }
}
