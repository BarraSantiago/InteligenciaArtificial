using UnityEngine;

namespace Utils
{
    public class Helper : MonoBehaviour
    {
        public static GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Instantiate(prefab, position, rotation);
        } 
    }
}