using System;
using UnityEngine;

namespace DefaultNamespace
{
    public enum FLags
    {
        OnTargetReach,
        OnTargetLost,
        OnTargetNear
    }

    public enum Directions
    {
        Chase,
        Patrol,
        Explode
    }

    public class Agent : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Transform wayPoint1;
        [SerializeField] private Transform wayPoint2;
        [SerializeField] private float speed;
        [SerializeField] private float chaseDistance;
        [SerializeField] private float explodeDistance;
        [SerializeField] private float lostDistance;
        private FSM _fsm;
        
        private void Start()
        {
            _fsm = new FSM(Enum.GetValues(typeof(Directions)).Length, Enum.GetValues(typeof(FLags)).Length);
            
            
            
            _fsm.AddBehaviour<PatrolState>((int)Directions.Patrol, PatrolTickParameters);
            _fsm.AddBehaviour<ChaseState>((int)Directions.Chase, ChaseTickParameters);
            _fsm.AddBehaviour<ExplodeState>((int)Directions.Explode, ExplodeTickParameters);
            
            

            _fsm.SetTransition((int)Directions.Patrol, (int)FLags.OnTargetNear, (int)Directions.Chase);
            _fsm.SetTransition((int)Directions.Chase, (int)FLags.OnTargetReach, (int)Directions.Explode);
            _fsm.SetTransition((int)Directions.Chase, (int)FLags.OnTargetLost, (int)Directions.Patrol);
        }

        private object[] ChaseTickParameters()
        {
            object[] objects = {transform, targetTransform, speed, this.explodeDistance, this.lostDistance};
            return objects;
        }
            
        private object[] PatrolTickParameters()
        {
            object[] objects = {transform, wayPoint1, wayPoint2, this.targetTransform, this.speed, this.chaseDistance};
            return objects;
        }
        
        private object[] ExplodeTickParameters()
        {
            object[] objects = {this.gameObject};
            return objects;
        }
        private void Update()
        {
            _fsm.Tick();
        }
    }
}