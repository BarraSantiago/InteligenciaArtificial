using System;
using StateMachine;
using States.Generic;
using UnityEngine;

namespace Units
{
    public enum Flags
    {
        OnTargetReach,
        OnTargetLost,
        OnTargetNear
    }

    public enum Behaviours
    {
        Chase,
        Patrol,
        Explode,
        Shoot
    }

    public class Agent : MonoBehaviour
    {
        
        [SerializeField] protected Transform targetTransform;
        [SerializeField] private Transform wayPoint1;
        [SerializeField] private Transform wayPoint2;
        [SerializeField] private float speed;
        [SerializeField] private float chaseDistance;
        [SerializeField] private float reachDistance = 3;
        [SerializeField] private float lostDistance;
        
        protected FSM<Behaviours,Flags> _fsm;
        
        private void Start()
        {
            Init();
        }

        protected virtual void Init()
        {
            _fsm = new FSM<Behaviours,Flags>();
            
            _fsm.AddBehaviour<PatrolState>(Behaviours.Patrol,
                () =>
                {
                    return new object[] { transform, wayPoint1, wayPoint2, this.targetTransform, this.speed, this.chaseDistance };
                });
            _fsm.AddBehaviour<ChaseState>(Behaviours.Chase, ChaseTickParameters);
            
            _fsm.SetTransition(Behaviours.Patrol, Flags.OnTargetNear, Behaviours.Chase, () => Debug.Log("Chase"));
            _fsm.SetTransition(Behaviours.Chase, Flags.OnTargetLost, Behaviours.Patrol, () => Debug.Log("Patrol"));
            
            _fsm.ForceTransition(Behaviours.Patrol);

        }
        
        protected object[] ChaseTickParameters()
        {
            object[] objects = { transform, targetTransform, speed, this.reachDistance, this.lostDistance };
            return objects;
        }

        protected object[] PatrolTickParameters()
        {
            object[] objects = { transform, wayPoint1, wayPoint2, this.targetTransform, this.speed, this.chaseDistance };
            return objects;
        }

        
        private void Update()
        {
            _fsm.Tick();
        }
    }
}