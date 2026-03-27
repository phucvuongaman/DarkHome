using UnityEngine;

namespace DarkHome
{
    public class PersistentManagers : MonoBehaviour
    {
        private static PersistentManagers _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

}