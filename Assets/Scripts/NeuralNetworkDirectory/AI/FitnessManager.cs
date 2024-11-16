using System;
using System.Collections.Generic;
using ECS.Patron;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using StateMachine.Agents.Simulation;
using Utils;

namespace NeuralNetworkDirectory.AI
{
    public class FitnessManager <TVector, TTransform>
        where TTransform : ITransform<IVector>, new()
        where TVector : IVector, IEquatable<TVector>
    {
        private static Dictionary<uint, SimAgent<TVector,TTransform>> _agents;

        public FitnessManager(Dictionary<uint, SimAgent<TVector,TTransform>> agents)
        {
            _agents = agents;
        }

        public void Tick()
        {
            foreach (var agent in _agents)
            {
                CalculateFitness(agent.Value.agentType, agent.Key);
            }
        }

        public void CalculateFitness(SimAgentTypes agentType, uint agentId)
        {
            switch (agentType)
            {
                case SimAgentTypes.Carnivorous:
                    CarnivorousFitnessCalculator(agentId);
                    break;
                case SimAgentTypes.Herbivore:
                    HerbivoreFitnessCalculator(agentId);
                    break;
                case SimAgentTypes.Scavenger:
                    ScavengerFitnessCalculator(agentId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(agentType), agentType, null);
            }
        }
        
        private void HerbivoreFitnessCalculator(uint agentId)
        {
            foreach (var brainType in _agents[agentId].brainTypes)
            {
                switch (brainType.Value)
                {
                    case BrainType.Movement:
                        break;
                    case BrainType.Eat:
                        break;
                    case BrainType.Escape:
                        break;
                    default:
                        throw new ArgumentException("Herbivore doesn't have a brain type: ", nameof(brainType));
                }
            }
        }

        private void CarnivorousFitnessCalculator(uint agentId)
        {
            foreach (var brainType in _agents[agentId].brainTypes)
            {
                switch (brainType.Value)
                {
                    case BrainType.Attack:
                        break;
                    case BrainType.Eat:
                        break;
                    case BrainType.Movement:
                        break;
                    default:
                        throw new ArgumentException("Carnivore doesn't have a brain type: ", nameof(brainType));
                }
            }
        }

        private void ScavengerFitnessCalculator(uint agentId)
        {
            foreach (var brainType in _agents[agentId].brainTypes)
            {
                switch (brainType.Value)
                {
                    case BrainType.ScavengerMovement:
                        ScavengerMovementFC(agentId);
                        break;
                    case BrainType.Eat:
                        ScavengerEatFC(agentId);
                        break;
                    case BrainType.Flocking:
                        ScavengerFlockingFC(agentId);
                        break;
                    default:
                        throw new ArgumentException("Scavenger doesn't have a brain type: ", nameof(brainType));
                }
            }
        }

        private void ScavengerFlockingFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.97f;
            const float safeDistance = 1f;

            var agent = (Scavenger<TVector,TTransform>)_agents[agentId];
            var neighbors = EcsPopulationManager.GetBoidsInsideRadius(agent.boid);
            var targetPosition = agent.CurrentNode.GetCoordinate();

            bool isMaintainingDistance = true;
            bool isAligningWithFlock = true;
            bool isColliding = false;

            IVector averageDirection = null;
            int neighborCount = 0;

            foreach (var neighbor in neighbors)
            {
                if (Equals(neighbor, agent)) continue;

                var neighborPosition = neighbor.transform.position;
                float distance = agent.CurrentNode.GetCoordinate().Distance(neighborPosition);

                if (distance < safeDistance) // Assuming 1.0f is the minimum safe distance
                {
                    isColliding = true;
                    isMaintainingDistance = false;
                }

                averageDirection += neighbor.transform.forward;
                neighborCount++;
            }

            if (neighborCount > 0)
            {
                averageDirection /= neighborCount;
                var agentDirection = agent.transform.forward.Normalized();
                var alignmentDotProduct = IVector.Dot(agentDirection, averageDirection.Normalized());

                if (alignmentDotProduct < 0.9f) // Assuming 0.9f as the threshold for alignment
                {
                    isAligningWithFlock = false;
                }
            }

            if (isMaintainingDistance && isAligningWithFlock && IsMovingTowardsTarget(agentId, targetPosition))
            {
                // Reward the agent
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward, BrainType.Flocking);
            }
            else if (isColliding || !IsMovingTowardsTarget(agentId, targetPosition))
            {
                // Penalize the agent
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Punish(punishment, BrainType.Flocking);
            }
        }

        private void ScavengerEatFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.97f;

            int brainId = (int)BrainType.ScavengerMovement;

            // Reward the agent
            ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward, BrainType.ScavengerMovement);
        }

        private void ScavengerMovementFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.97f;

            int brainId = (int)BrainType.ScavengerMovement;
            var agent = _agents[agentId];
            var nearestCarrionNode = EcsPopulationManager.GetNearestNode(SimNodeType.Carrion, agent.CurrentNode);
            var nearestCorpseNode = EcsPopulationManager.GetNearestNode(SimNodeType.Corpse, agent.CurrentNode);
            var nearestCarNode = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, agent.CurrentNode);

            IVector targetPosition;

            if (nearestCarrionNode != null)
            {
                targetPosition = nearestCarrionNode.GetCoordinate();
            }
            else if (nearestCorpseNode != null)
            {
                targetPosition = nearestCorpseNode.GetCoordinate();
            }
            else
            {
                targetPosition = nearestCarNode.CurrentNode.GetCoordinate();
            }

            if (IsMovingTowardsTarget(agentId, targetPosition))
            {
                // Reward the agent
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward, BrainType.ScavengerMovement);
            }
            else
            {
                // Penalize the agent
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Punish(punishment, BrainType.ScavengerMovement);
            }
        }

        private bool IsMovingTowardsTarget(uint agentId, IVector targetPosition)
        {
            var agent = _agents[agentId];
            var currentPosition = agent.CurrentNode.GetCoordinate();
            IVector currentVelocity = agent.transform.forward;

            // Calculate the direction to the target
            var directionToTarget = (targetPosition - currentPosition).Normalized();

            // Normalize the current velocity
            var normalizedVelocity = currentVelocity.Normalized();

            // Calculate the dot product between the direction to the target and the agent's velocity
            var dotProduct = IVector.Dot(directionToTarget, normalizedVelocity);

            // If the dot product is close to 1, the agent is moving towards the target
            return dotProduct > 0.9f;
        }
    }
}