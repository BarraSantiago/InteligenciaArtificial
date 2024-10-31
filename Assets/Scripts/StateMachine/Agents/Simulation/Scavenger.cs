using Pathfinder;

namespace StateMachine.Agents.Simulation
{
    public class Scavenger : SimAgent
    {
        public override void Init()
        {
            base.Init();
            SimAgentType = SimAgentTypes.Scavenger;
            foodTarget = SimNodeType.Carrion;
            FoodLimit = 20;
            movement = 5;
        }
    }
}