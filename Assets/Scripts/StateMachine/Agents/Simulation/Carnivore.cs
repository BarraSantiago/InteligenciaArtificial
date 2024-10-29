using Pathfinder;

namespace StateMachine.Agents.Simulation
{
    public class Carnivore : SimAgent
    {
        public override void Init()
        {
            base.Init();
            FoodLimit = 1;
            foodTarget = SimNodeType.Corpse;
        }
    }
}