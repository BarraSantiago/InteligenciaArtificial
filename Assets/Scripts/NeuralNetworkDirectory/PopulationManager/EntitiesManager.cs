using System.Collections.Generic;
using Flocking;
using Pathfinder;
using StateMachine.Agents.Simulation;
using Utils;

namespace NeuralNetworkDirectory.PopulationManager
{
    using SimAgentType = SimAgent<IVector, ITransform<IVector>>;
    using SimBoid = Boid<IVector, ITransform<IVector>>;

    public class EntitiesManager
    {
        // ENTITIES
        public static SimAgentType GetNearestEntity(SimAgentTypes entityType, IVector position)
        {
            SimAgentType nearestAgent = null;
            float minDistance = float.MaxValue;

            foreach (var agent in DataContainer._agents.Values)
            {
                if (agent.agentType != entityType) continue;

                float distance = IVector.Distance(position, agent.CurrentNode.GetCoordinate());

                if (minDistance < distance) continue;

                minDistance = distance;
                nearestAgent = agent;
            }

            return nearestAgent;
        }

        // ENTITIES
        public static SimAgentType GetEntity(SimAgentTypes entityType, INode<IVector> position)
        {
            SimAgentType target = null;

            foreach (var agent in DataContainer._agents.Values)
            {
                if (agent.agentType != entityType) continue;

                if (!position.GetCoordinate().Equals(agent.CurrentNode.GetCoordinate())) continue;

                target = agent;
                break;
            }

            return target;
        }
        
        public static List<SimBoid> GetBoidsInsideRadius(SimBoid boid)
        {
            List<SimBoid> insideRadiusBoids = new List<SimBoid>();

            foreach (var scavenger in DataContainer._scavengers.Values)
            {
                if (scavenger?.Transform.position == null)
                {
                    continue;
                }

                if (IVector.Distance(boid.transform.position, scavenger.Transform.position) >
                    boid.detectionRadious) continue;
                if (boid == scavenger.boid) continue;
                insideRadiusBoids.Add(scavenger.boid);
            }

            return insideRadiusBoids;
        }

        public static INode<IVector> GetNearestNode(SimNodeType carrion, IVector position)
        {
            INode<IVector> nearestNode = null;
            float minDistance = float.MaxValue;

            foreach (var node in DataContainer.graph.NodesType)
            {
                if (node.NodeType != carrion) continue;

                float distance = IVector.Distance(position, node.GetCoordinate());

                if (minDistance > distance) continue;

                minDistance = distance;

                nearestNode = node;
            }

            return nearestNode;
        }

    }
}