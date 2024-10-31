using UnityEngine;
using System.Collections.Generic;
using ECS.Patron;
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
            // TODO deberia ser pararell for each?
            foreach (var entity in entities)
            {
                ECSManager.GetComponent<InputComponent>(entity.Key).inputs = agents[entity.Key].input;

                agents[entity.Key].output = ECSManager.GetComponent<OutputComponent>(entity.Key).outputs;
                
                agents[entity.Key].Tick();
            }
        }

        public static SimAgent GetNearestEntity(SimAgent.SimAgentTypes entityType, SimNode<Vector2> position)
        {
            SimAgent nearestAgent = null;
            float minDistance = float.MaxValue;

            foreach (var agent in agents.Values)
            {
                if (agent.SimAgentType != entityType) continue;

                float distance = Vector2.Distance(position.GetCoordinate(), agent.CurrentNode.GetCoordinate());

                if (minDistance > distance) continue;

                minDistance = distance;
                nearestAgent = agent;
            }

            return nearestAgent;
        }
        
        public static SimAgent GetEntity(SimAgent.SimAgentTypes entityType, SimNode<Vector2> position)
        {
            SimAgent target = null;

            foreach (var agent in agents.Values)
            {
                if (agent.SimAgentType != entityType) continue;
                
                if (!position.GetCoordinate().Equals(agent.CurrentNode.GetCoordinate())) continue;

                target = agent;
                break;
            }

            return target;
        }
    }
}