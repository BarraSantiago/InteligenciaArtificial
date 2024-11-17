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
    public class FitnessManager<TVector, TTransform>
        where TTransform : ITransform<IVector>, new()
        where TVector : IVector, IEquatable<TVector>
    {
        private static Dictionary<uint, SimAgent<TVector, TTransform>> _agents;

        public FitnessManager(Dictionary<uint, SimAgent<TVector, TTransform>> agents)
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
                    CarnivoreFitnessCalculator(agentId);
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
                        HerbivoreMovementFC(agentId);
                        break;
                    case BrainType.Eat:
                        EatFitnessCalculator(agentId);
                        break;
                    case BrainType.Escape:
                        HerbivoreEscapeFC(agentId);
                        break;
                    default:
                        throw new ArgumentException("Herbivore doesn't have a brain type: ", nameof(brainType));
                }
            }
        }

        private void HerbivoreEscapeFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            var agent = _agents[agentId] as Herbivore<IVector, ITransform<IVector>>;
            var nearestPredatorNode =
                EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, agent?.CurrentNode);

            IVector targetPosition;

            if (nearestPredatorNode?.CurrentNode?.GetCoordinate() == null) return;
            targetPosition = nearestPredatorNode.CurrentNode.GetCoordinate();

            if (!IsMovingTowardsTarget(agentId, targetPosition))
            {
                float rewardMod = (float)agent?.Hp * 2 / 2;
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward * rewardMod, BrainType.Escape);
            }

            if (agent?.Hp < 2)
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Punish(punishment, BrainType.Escape);
            }
        }

        private void HerbivoreMovementFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            var agent = _agents[agentId];
            var nearestPredatorNode =
                EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, agent.CurrentNode);

            IVector targetPosition;

            if (nearestPredatorNode?.CurrentNode?.GetCoordinate() == null) return;
            targetPosition = nearestPredatorNode.CurrentNode.GetCoordinate();

            if (IsMovingTowardsTarget(agentId, targetPosition))
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward, BrainType.Movement);
            }
            else
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Punish(punishment, BrainType.Movement);
            }
        }

        private void CarnivoreFitnessCalculator(uint agentId)
        {
            foreach (var brainType in _agents[agentId].brainTypes)
            {
                switch (brainType.Value)
                {
                    case BrainType.Attack:
                        CarnivoreAttackFC(agentId);
                        break;
                    case BrainType.Eat:
                        EatFitnessCalculator(agentId);
                        break;
                    case BrainType.Movement:
                        CarnivoreMovementFC(agentId);
                        break;
                    default:
                        throw new ArgumentException("Carnivore doesn't have a brain type: ", nameof(brainType));
                }
            }
        }


        private void CarnivoreAttackFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            var agent = _agents[agentId] as Carnivore<IVector, ITransform<IVector>>;
            var nearestHerbivoreNode =
                EcsPopulationManager.GetNearestEntity(SimAgentTypes.Herbivore, agent.CurrentNode);

            IVector targetPosition;

            if (nearestHerbivoreNode?.CurrentNode?.GetCoordinate() == null) return;
            targetPosition = nearestHerbivoreNode.CurrentNode.GetCoordinate();

            if (IsMovingTowardsTarget(agentId, targetPosition))
            {
                float killRewardMod = agent.HasKilled ? 2 : 1;
                float attackedRewardMod = agent.HasAttacked ? 1.5f : 0;
                float damageRewardMod = (float)agent.DamageDealt * 2 / 5;
                float rewardMod = killRewardMod * attackedRewardMod * damageRewardMod;

                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward * rewardMod, BrainType.Attack);
            }
            else
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Punish(punishment, BrainType.Attack);
            }
        }

        private void CarnivoreMovementFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            var agent = _agents[agentId];
            var nearestHerbivoreNode =
                EcsPopulationManager.GetNearestEntity(SimAgentTypes.Herbivore, agent.CurrentNode);
            var nearestCorpseNode = EcsPopulationManager.GetNearestNode(SimNodeType.Corpse, agent.CurrentNode);

            if (nearestHerbivoreNode?.CurrentNode?.GetCoordinate() == null) return;

            var herbPosition = nearestHerbivoreNode.CurrentNode.GetCoordinate();
            var corpsePosition = nearestCorpseNode?.GetCoordinate();

            if (IsMovingTowardsTarget(agentId, herbPosition) ||
                corpsePosition != null && IsMovingTowardsTarget(agentId, corpsePosition))
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward, BrainType.Movement);
            }
            else
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Punish(punishment, BrainType.Movement);
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
                        EatFitnessCalculator(agentId);
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
            const float punishment = 0.90f;
            const float safeDistance = 1f;

            var agent = (Scavenger<TVector, TTransform>)_agents[agentId];
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

                if (distance < safeDistance)
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
                var agentDirection = agent.Transform.forward.Normalized();
                var alignmentDotProduct = IVector.Dot(agentDirection, averageDirection.Normalized());

                if (alignmentDotProduct < 0.9f)
                {
                    isAligningWithFlock = false;
                }
            }

            if (isMaintainingDistance && isAligningWithFlock && IsMovingTowardsTarget(agentId, targetPosition))
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward, BrainType.Flocking);
            }
            else if (isColliding || !IsMovingTowardsTarget(agentId, targetPosition))
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Punish(punishment, BrainType.Flocking);
            }
        }

        private void ScavengerMovementFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

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
                if (nearestCarNode?.CurrentNode?.GetCoordinate() == null) return;
                targetPosition = nearestCarNode.CurrentNode.GetCoordinate();
            }

            if (targetPosition == null) return;

            if (IsMovingTowardsTarget(agentId, targetPosition))
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward, BrainType.ScavengerMovement);
            }
            else
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Punish(punishment, BrainType.ScavengerMovement);
            }
        }

        private void EatFitnessCalculator(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            var agent = _agents[agentId];

            if (agent.Food <= 0) return;

            float rewardMod = (float)agent.Food * 2 / agent.FoodLimit;
            ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward * rewardMod, BrainType.Eat);
        }

        private bool IsMovingTowardsTarget(uint agentId, IVector targetPosition)
        {
            var agent = _agents[agentId];
            var currentPosition = agent.CurrentNode.GetCoordinate();
            IVector agentDirection = agent.Transform.forward?.Normalized();

            var directionToTarget = (targetPosition - currentPosition).Normalized();
            if(directionToTarget == null || agentDirection == null) return false;
            var dotProduct = IVector.Dot(directionToTarget, agentDirection);

            return dotProduct > 0.9f;
        }
    }
}