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

        void AgentIdentifier(SimAgentTypes agentType, uint agentId)
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
                    case BrainType.Movement:
                        break;
                    case BrainType.ScavengerMovement:
                        ScavengerMovementFitnessCalculator(agentId);
                        break;
                    case BrainType.Eat:
                        break;
                    case BrainType.Attack:
                        break;
                    case BrainType.Escape:
                        break;
                    case BrainType.Flocking:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ScavengerMovementFitnessCalculator(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.97f;

            int brainId = (int)BrainType.ScavengerMovement;
            var agent = _agents[agentId];
            var currentPosition = agent.CurrentNode.GetCoordinate();
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