using System;
using StateMachine;
using States.Archer;
using States.Creeper;
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
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Transform wayPoint1;
        [SerializeField] private Transform wayPoint2;
        [SerializeField] private float speed;
        [SerializeField] private float chaseDistance;
        [SerializeField] private float explodeDistance;
        [SerializeField] private float lostDistance;
        
        private FSM _fsm;
        private float lastAttack = 0;
        private void Start()
        {
            _fsm = new FSM(Enum.GetValues(typeof(Directions)).Length, Enum.GetValues(typeof(Flags)).Length);


            _fsm.AddBehaviour<PatrolState>((int)Directions.Patrol, PatrolTickParameters);
            _fsm.AddBehaviour<ChaseState>((int)Directions.Chase, ChaseTickParameters);
            //_fsm.AddBehaviour<ExplodeState>((int)Directions.Explode, ExplodeTickParameters);
            _fsm.AddBehaviour<Shoot>((int)Directions.Shoot, ShootTickParameters);


            _fsm.SetTransition((int)Directions.Patrol, (int)Flags.OnTargetNear, (int)Directions.Chase);
            _fsm.SetTransition((int)Directions.Chase, (int)Flags.OnTargetReach, (int)Directions.Shoot);
            _fsm.SetTransition((int)Directions.Chase, (int)Flags.OnTargetLost, (int)Directions.Patrol);
            _fsm.SetTransition((int)Directions.Shoot, (int)Flags.OnTargetLost, (int)Directions.Chase);
        }

        private object[] ChaseTickParameters()
        {
            object[] objects = { transform, targetTransform, speed, this.explodeDistance, this.lostDistance };
            return objects;
        }

        private object[] PatrolTickParameters()
        {
            object[] objects = { transform, wayPoint1, wayPoint2, this.targetTransform, this.speed, this.chaseDistance };
            return objects;
        }

        private object[] ExplodeTickParameters()
        {
            object[] objects = { this.gameObject };
            return objects;
        }
        
        private object[] ShootTickParameters()
        {
            object[] objects = { arrowPrefab, transform, this.targetTransform, 10f, lastAttack};
            return objects;
        }

        private void Update()
        {
            _fsm.Tick();
        }
    }
}