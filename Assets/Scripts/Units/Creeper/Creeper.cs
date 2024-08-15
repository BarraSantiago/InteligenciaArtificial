using States.Creeper;

namespace Units.Creeper
{
    public class Creeper : Agent
    {
        protected override void Init()
        {
            base.Init();
            
            _fsm.AddBehaviour<ExplodeState>(Behaviours.Explode, ExplodeTickParameters);

            _fsm.SetTransition(Behaviours.Chase, Flags.OnTargetReach, Behaviours.Explode);
        }

        private object[] ExplodeTickParameters()
        {
            object[] objects = { this.gameObject };
            return objects;
        }
    }
}