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

    public enum Directions
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
        
        protected FSM _fsm;
        
        private void Start()
        {
            Init();
        }

        protected virtual void Init()
        {
            _fsm = new FSM(Enum.GetValues(typeof(Directions)).Length, Enum.GetValues(typeof(Flags)).Length);
            
            _fsm.AddBehaviour<PatrolState>((int)Directions.Patrol, PatrolTickParameters);
            _fsm.AddBehaviour<ChaseState>((int)Directions.Chase, ChaseTickParameters);
            
            _fsm.SetTransition((int)Directions.Patrol, (int)Flags.OnTargetNear, (int)Directions.Chase);
            _fsm.SetTransition((int)Directions.Chase, (int)Flags.OnTargetLost, (int)Directions.Patrol);
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