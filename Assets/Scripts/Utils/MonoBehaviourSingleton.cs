using UnityEngine;

namespace Toolbox
{
    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance;
        public static T Get()
        {
            return Instance;
        }

        public virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this as T;
            DontDestroyOnLoad(this);
        }
    }
}