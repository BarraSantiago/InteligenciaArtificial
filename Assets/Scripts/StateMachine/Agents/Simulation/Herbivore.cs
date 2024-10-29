using Pathfinder;

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
        }

        private void Die()
        {
            CurrentNode.NodeType = SimNodeType.Corpse;
            CurrentNode.food = FoodDropped;
        }
    }
}