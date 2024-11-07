using System;
using System.Collections.Generic;
using ECS.Patron;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using StateMachine.Agents.Simulation;
using UnityEngine;

namespace NeuralNetworkDirectory.AI
{
    public class FitnessManager
    {
        private static Dictionary<uint, SimAgent> _agents;
        
        public FitnessManager(Dictionary<uint, SimAgent> agents)
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

        private void ScavengerFitnessCalculator(uint agentId)
        {
            foreach (var brainType in _agents[agentId].brainTypes)
            {
                switch (brainType)
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

            var agent = (Scavenger)_agents[agentId];
            var neighbors = EcsPopulationManager.GetBoidsInsideRadius(agent.boid);
            var targetPosition = agent.CurrentNode.GetCoordinate();

            bool isMaintainingDistance = true;
            bool isAligningWithFlock = true;
            bool isColliding = false;

            Vector2 averageDirection = Vector2.zero;
            int neighborCount = 0;

            foreach (var neighbor in neighbors)
            {
                if (neighbor == agent) continue;

                Vector2 neighborPosition = neighbor.transform.position;
                float distance = Vector2.Distance(agent.CurrentNode.GetCoordinate(), neighborPosition);

                if (distance < safeDistance) // Assuming 1.0f is the minimum safe distance
                {
                    isColliding = true;
                    isMaintainingDistance = false;
                }

                averageDirection += (Vector2)neighbor.transform.forward;
                neighborCount++;
            }

            if (neighborCount > 0)
            {
                averageDirection /= neighborCount;
                var agentDirection = agent.transform.forward.normalized;
                var alignmentDotProduct = Vector2.Dot(agentDirection, averageDirection.normalized);

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

            Vector2 targetPosition;

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

        private void HerbivoreFitnessCalculator(uint agentId)
        {
            throw new NotImplementedException();
        }

        private void CarnivorousFitnessCalculator(uint agentId)
        {
            throw new NotImplementedException();
        }

        private bool IsMovingTowardsTarget(uint agentId, Vector2 targetPosition)
        {
            var agent = _agents[agentId];
            var currentPosition = agent.CurrentNode.GetCoordinate();
            Vector2 currentVelocity = agent.transform.forward;

            // Calculate the direction to the target
            var directionToTarget = (targetPosition - currentPosition).normalized;

            // Normalize the current velocity
            var normalizedVelocity = currentVelocity.normalized;

            // Calculate the dot product between the direction to the target and the agent's velocity
            var dotProduct = Vector2.Dot(directionToTarget, normalizedVelocity);

            // If the dot product is close to 1, the agent is moving towards the target
            return dotProduct > 0.9f;
        }

        
    }
}