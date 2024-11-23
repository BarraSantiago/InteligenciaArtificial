using System;
using System.Collections.Generic;
using ECS.Patron;
using Flocking;
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
            foreach (KeyValuePair<uint, SimAgent<TVector, TTransform>> agent in _agents)
            {
                CalculateFitness(agent.Value.agentType, agent.Key);
            }
        }

        public void CalculateFitness(SimAgentTypes agentType, uint agentId)
        {
            switch (agentType)
            {
                case SimAgentTypes.Carnivore:
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
            foreach (KeyValuePair<int, BrainType> brainType in _agents[agentId].brainTypes)
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
                    case BrainType.ScavengerMovement:
                    case BrainType.Attack:
                    case BrainType.Flocking:
                    default:
                        throw new ArgumentException("Herbivore doesn't have a brain type: ", nameof(brainType));
                }
            }
        }

        private void HerbivoreEscapeFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            Herbivore<IVector, ITransform<IVector>> agent = _agents[agentId] as Herbivore<IVector, ITransform<IVector>>;
            SimAgent<IVector, ITransform<IVector>> nearestPredatorNode =
                EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivore, agent?.Transform.position);

            IVector targetPosition;

            if (nearestPredatorNode?.CurrentNode?.GetCoordinate() == null) return;
            targetPosition = nearestPredatorNode.CurrentNode.GetCoordinate();

            if (!IsMovingTowardsTarget(agentId, targetPosition))
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward, BrainType.Escape);
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

            SimAgent<TVector, TTransform> agent = _agents[agentId];
            SimAgent<IVector, ITransform<IVector>> nearestPredatorNode =
                EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivore, agent.Transform.position);

            if (nearestPredatorNode?.CurrentNode?.GetCoordinate() == null) return;
            IVector targetPosition = nearestPredatorNode.CurrentNode.GetCoordinate();

            if (!IsMovingTowardsTarget(agentId, targetPosition))
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
            foreach (KeyValuePair<int, BrainType> brainType in _agents[agentId].brainTypes)
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
                    case BrainType.ScavengerMovement:
                    case BrainType.Escape:
                    case BrainType.Flocking:
                    default:
                        throw new ArgumentException("Carnivore doesn't have a brain type: ", nameof(brainType));
                }
            }
        }


        private void CarnivoreAttackFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            if(_agents[agentId] is not Carnivore<IVector, ITransform<IVector>> agent) return;
            SimAgent<IVector, ITransform<IVector>> nearestHerbivoreNode =
                EcsPopulationManager.GetNearestEntity(SimAgentTypes.Herbivore, agent.Transform.position);

            if (nearestHerbivoreNode?.CurrentNode?.GetCoordinate() == null) return;
            IVector targetPosition = nearestHerbivoreNode.CurrentNode.GetCoordinate();

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

            SimAgent<TVector, TTransform> agent = _agents[agentId];
            SimAgent<IVector, ITransform<IVector>> nearestHerbivoreNode =
                EcsPopulationManager.GetNearestEntity(SimAgentTypes.Herbivore, agent.Transform.position);
            INode<IVector> nearestCorpseNode = EcsPopulationManager.GetNearestNode(SimNodeType.Corpse, agent.Transform.position);

            if (nearestHerbivoreNode?.CurrentNode?.GetCoordinate() == null) return;

            IVector herbPosition = nearestHerbivoreNode.CurrentNode.GetCoordinate();
            IVector corpsePosition = nearestCorpseNode?.GetCoordinate();

            bool movingToHerb = IsMovingTowardsTarget(agentId, herbPosition);
            bool movingToCorpse = corpsePosition != null && IsMovingTowardsTarget(agentId, corpsePosition);
            
            if (movingToHerb || movingToCorpse)
            {
                float rewardMod = movingToHerb ? 1.15f : 0.9f;
                
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward * rewardMod, BrainType.Movement);
            }
            else
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Punish(punishment, BrainType.Movement);
            }
        }

        private void ScavengerFitnessCalculator(uint agentId)
        {
            foreach (KeyValuePair<int, BrainType> brainType in _agents[agentId].brainTypes)
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
                    case BrainType.Movement:
                    case BrainType.Attack:
                    case BrainType.Escape:
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

            Scavenger<TVector, TTransform> agent = (Scavenger<TVector, TTransform>)_agents[agentId];
            List<Boid<IVector, ITransform<IVector>>> neighbors = EcsPopulationManager.GetBoidsInsideRadius(agent.boid);
            IVector targetPosition = agent.GetTarget(SimNodeType.Carrion).GetCoordinate();

            bool isMaintainingDistance = true;
            bool isAligningWithFlock = true;
            bool isColliding = false;

            IVector averageDirection = null;
            int neighborCount = 0;

            foreach (Boid<IVector, ITransform<IVector>> neighbor in neighbors)
            {
                if (Equals(neighbor, agent.boid)) continue;

                IVector neighborPosition = neighbor.transform.position;
                float distance = agent.Transform.position.Distance(neighborPosition);

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
                IVector agentDirection = agent.boid.transform.forward;
                float alignmentDotProduct = IVector.Dot(agentDirection, averageDirection.Normalized());

                if (alignmentDotProduct < 0.9f)
                {
                    isAligningWithFlock = false;
                }
            }

            if (isMaintainingDistance || isAligningWithFlock || IsMovingTowardsTarget(agentId, targetPosition))
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward, BrainType.Flocking);
            }
            
            if (isColliding || !IsMovingTowardsTarget(agentId, targetPosition))
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Punish(punishment, BrainType.Flocking);
            }
        }

        private void ScavengerMovementFC(uint agentId)
        {
            const float reward = 10;
            const float punishment = 0.90f;

            int brainId = (int)BrainType.ScavengerMovement;
            Scavenger<TVector, TTransform> agent = (Scavenger<TVector, TTransform>)_agents[agentId];
            int neighbors = EcsPopulationManager.GetBoidsInsideRadius(agent.boid).Count;
            INode<IVector> nearestCarrionNode = EcsPopulationManager.GetNearestNode(SimNodeType.Carrion, agent.Transform.position);
            INode<IVector> nearestCorpseNode = EcsPopulationManager.GetNearestNode(SimNodeType.Corpse, agent.Transform.position);
            SimAgent<IVector, ITransform<IVector>> nearestCarNode = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivore, agent.Transform.position);

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

            if(neighbors > 0)
            {
                ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward/5+neighbors, BrainType.ScavengerMovement);
            }
            
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

            SimAgent<TVector, TTransform> agent = _agents[agentId];

            if (agent.Food <= 0) return;

            float rewardMod = (float)agent.Food * 2 / agent.FoodLimit;
            ECSManager.GetComponent<NeuralNetComponent>(agentId).Reward(reward * rewardMod, BrainType.Eat);
        }

        private bool IsMovingTowardsTarget(uint agentId, IVector targetPosition)
        {
            SimAgent<TVector, TTransform> agent = _agents[agentId];
            IVector currentPosition = agent.Transform.position;
            IVector agentDirection = agent.Transform.forward;

            IVector directionToTarget = (targetPosition - currentPosition).Normalized();
            if(directionToTarget == null || agentDirection == null) return false;
            float dotProduct = IVector.Dot(directionToTarget, agentDirection);

            return dotProduct > 0.9f;
        }
    }
}