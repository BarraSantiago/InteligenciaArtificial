using Pathfinder;

namespace StateMachine.Agents.Simulation
{
    public class Scavenger : SimAgent
    {

        public override void Init()
        {
            base.Init();
            FoodLimit = 20;
            foodTarget = SimNodeType.Carrion;
            movement = 5;
            SimAgentType = SimAgentTypes.Scavenger;
        }

    }
}