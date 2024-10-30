using NeuralNetworkDirectory.ECS;
using Pathfinder;
using UnityEngine;

namespace StateMachine.Agents.Simulation
{
    public class Herbivore : SimAgent
    {
        private int hp;

        public int Hp
        {
            get => hp;
            set
            {
                hp = value;
                if (hp <= 0) Die();
            }
        }

        private const int FoodDropped = 1;
        private const int InitialHp = 2;

        public override void Init()
        {
            base.Init();
            hp = InitialHp;
            foodTarget = SimNodeType.Bush;
            SimAgentType = SimAgentTypes.Herbivore;
        }

        protected override void EscapeInputs()
        {
            input[1][0] = CurrentNode.GetCoordinate().x;
            input[1][1] = CurrentNode.GetCoordinate().y;
            SimNode<Vector2> target = EcsPopulationManager.GetEntity(SimAgentTypes.Carnivorous, CurrentNode);
            input[1][2] = target.GetCoordinate().x;
            input[1][3] = target.GetCoordinate().y;
        }

        private void Die()
        {
            CurrentNode.NodeType = SimNodeType.Corpse;
            CurrentNode.food = FoodDropped;
        }
    }
}