using System.Collections.Generic;
using ECS.Implementation;
using UnityEngine;

public class ECSExample_ECSWithGOs : MonoBehaviour
{
    public int entityCount = 100;
    public float velocity = 10.0f;
    public GameObject prefab;

    private Dictionary<uint, GameObject> entities;

    void Start()
    {
        ECSManager.Init();
        entities = new Dictionary<uint, GameObject>();
        for (int i = 0; i < entityCount; i++)
        {
            uint entityID = ECSManager.CreateEntity();
            ECSManager.AddComponent(entityID, new PositionComponent<Vector3>(new Vector3( 0, -i, 0)));
            ECSManager.AddComponent(entityID, new VelocityComponent<Vector3>(velocity, Vector3.right));
            ECSManager.AddComponent(entityID, new RotationComponent(0, 0, 0));
            ECSManager.AddComponent(entityID, new VelRotationComponent(10, Vector3.up.x, Vector3.up.y, Vector3.up.z));
            entities.Add(entityID, Instantiate(prefab, new Vector3(0, -i, 0), Quaternion.identity));
        }
    }

    void Update()
    {
        ECSManager.Tick(Time.deltaTime);
    }

    void LateUpdate()
    {
        foreach (KeyValuePair<uint, GameObject> entity in entities)
        {
            PositionComponent<Vector3> position = ECSManager.GetComponent<PositionComponent<Vector3>>(entity.Key);
            entity.Value.transform.SetPositionAndRotation(new Vector3(position.Position.x, position.Position.y, position.Position.z), Quaternion.identity);
            RotationComponent rotationComponent = ECSManager.GetComponent<RotationComponent>(entity.Key);
            entity.Value.transform.rotation = Quaternion.Euler(rotationComponent.X, rotationComponent.Y, rotationComponent.Z);
        }
    }
}
