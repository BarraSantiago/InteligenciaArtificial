using System.Collections.Generic;
using UnityEngine;

public class ECSExample_NoECS : MonoBehaviour
{
    public int entityCount = 100;
    public float velocity = 10.0f;
    public GameObject prefab;

    private List<GameObject> entities;

    private void Start()
    {
        entities = new List<GameObject>();
        for (var i = 0; i < entityCount; i++)
        {
            var go = Instantiate(prefab, new Vector3(0, -i, 0), Quaternion.identity);
            go.AddComponent<MovementMonoBehaviour>().velocity = velocity;
            entities.Add(go);
        }
    }
}