using UnityEngine;
using System.Collections.Generic;
using ECS.Implementation;
using UnityEngine;

namespace NeuralNetworkDirectory.ECS
{
    public class ECSPopulationManager : MonoBehaviour
    {
        public int entityCount = 100;
        public GameObject prefab;

        private Dictionary<uint, GameObject> entities;

        private void Start()
        {
            ECSManager.Init();
            entities = new Dictionary<uint, GameObject>();
            for (var i = 0; i < entityCount; i++)
            {
                var entityID = ECSManager.CreateEntity();
                ECSManager.AddComponent(entityID, new InputComponent());
                ECSManager.AddComponent(entityID, new NeuralNetComponent());
                ECSManager.AddComponent(entityID, new OutputComponent());
            }
        }

        private void Update()
        {
            ECSManager.Tick(Time.deltaTime);
        }

        private void LateUpdate()
        {
            foreach (var entity in entities)
            {
                var position = ECSManager.GetComponent<PositionComponent<Vector3>>(entity.Key);
                entity.Value.transform.SetPositionAndRotation(
                    new Vector3(position.Position.x, position.Position.y, position.Position.z), Quaternion.identity);
                var rotationComponent = ECSManager.GetComponent<RotationComponent>(entity.Key);
                entity.Value.transform.rotation =
                    Quaternion.Euler(rotationComponent.X, rotationComponent.Y, rotationComponent.Z);
            }
        }
    }
}