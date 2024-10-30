using UnityEngine;
using System.Collections.Generic;
using Pathfinder;
using StateMachine.Agents.Simulation;

namespace NeuralNetworkDirectory.ECS
{
    public class EcsPopulationManager : MonoBehaviour
    {
        public int entityCount = 100;
        public GameObject prefab;

        private Dictionary<uint, GameObject> entities;
        private static Dictionary<uint, SimAgent> agents;

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
                for (int i = 0; i < agents[entity.Key].input.Length; i++)
                {
                    ECSManager.GetComponent<InputComponent>(entity.Key).inputs = agents[entity.Key].input[i];
                }
                
                var output = ECSManager.GetComponent<OutputComponent>(entity.Key).outputs;
                
                for (int i = 0; i < output.Length; i++)
                {
                    agents[entity.Key].output[i] = output[i];
                }
                
                agents[entity.Key].Tick();
            }
        }

        public static SimNode<Vector2> GetEntity(SimAgent.SimAgentTypes entityType, SimNode<Vector2> position)
        {
            SimNode<Vector2> nearestAgent = null;
            float minDistance = float.MaxValue;

            foreach (var agent in agents.Values)
            {
                if (agent.SimAgentType != entityType) continue;
                
                float distance = Vector2.Distance(position.GetCoordinate(), agent.CurrentNode.GetCoordinate());
                
                if (!(distance < minDistance)) continue;
                
                minDistance = distance;
                nearestAgent = agent.CurrentNode;
            }

            return nearestAgent;
        }
    }
}