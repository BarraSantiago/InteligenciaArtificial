using Pathfinder;

namespace StateMachine.Agents.RTS
{
    public class Miner : RTSAgent
    {
        private NodeType nodeObjective = NodeType.Mine;
        //protected override void Init()
        //{
        //    base.Init();
        //    _fsm.AddBehaviour<Mine>(Behaviours.Mine, MineTickParameters);
        //    _fsm.SetTransition(Behaviours.Patrol, Flags.OnTargetReach, Behaviours.Mine);
        //    _fsm.SetTransition(Behaviours.Mine, Flags.OnTargetLost, Behaviours.Patrol);
        //}
        
    }
}