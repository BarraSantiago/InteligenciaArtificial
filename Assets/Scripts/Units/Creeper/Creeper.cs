using States.Creeper;

namespace Units.Creeper
{
    public class Creeper : Agent
    {
        protected override void Init()
        {
            base.Init();
            
            _fsm.AddBehaviour<ExplodeState>((int)Directions.Explode, ExplodeTickParameters);

            _fsm.SetTransition((int)Directions.Chase, (int)Flags.OnTargetReach, (int)Directions.Explode);
        }

        private object[] ExplodeTickParameters()
        {
            object[] objects = { this.gameObject };
            return objects;
        }
    }
}