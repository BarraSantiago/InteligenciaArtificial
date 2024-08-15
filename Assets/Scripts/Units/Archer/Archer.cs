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
            _fsm.AddBehaviour<Shoot>(Behaviours.Shoot, ShootTickParameters);
            
            _fsm.SetTransition(Behaviours.Chase, Flags.OnTargetReach, Behaviours.Shoot);
            _fsm.SetTransition(Behaviours.Shoot, Flags.OnTargetLost, Behaviours.Patrol);
            
        }
        
        private object[] ShootTickParameters()
        {
            object[] objects = { arrowPrefab, transform, this.targetTransform, 10f, shootDistance};
            return objects;
        }

    }
}