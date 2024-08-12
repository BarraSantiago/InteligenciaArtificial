using States.Archer;
using UnityEngine;

namespace Units.Archer
{
    public class Archer : Agent
    {
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private float shootDistance = 3;

        protected override void Init()
        {
            base.Init();
            _fsm.AddBehaviour<Shoot>((int)Directions.Shoot, ShootTickParameters);
            
            _fsm.SetTransition((int)Directions.Chase, (int)Flags.OnTargetReach, (int)Directions.Shoot);
            _fsm.SetTransition((int)Directions.Shoot, (int)Flags.OnTargetLost, (int)Directions.Patrol);
        }
        
        private object[] ShootTickParameters()
        {
            object[] objects = { arrowPrefab, transform, this.targetTransform, 10f, shootDistance};
            return objects;
        }

    }
}