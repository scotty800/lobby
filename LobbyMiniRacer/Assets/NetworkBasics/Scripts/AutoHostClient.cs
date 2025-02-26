using Mirror;
using UnityEngine;

namespace MirrorBasics
{
    public class AutoHostClient : MonoBehaviour
    {
        [SerializeField] NetworkManager networkManager;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (!Application.isBatchMode)
            {
                Debug.Log($"=== Client Build ===");
                networkManager.StartClient();
            }
            else
            {
                Debug.Log($"=== Server Build ===");
            }
        }

        public void JoinLocal()
        {
            networkManager.networkAddress = "localhost";
            networkManager.StartClient();
        }
    }
}

