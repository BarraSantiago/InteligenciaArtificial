using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using Pathfinder.Graph;
using StateMachine.States.SimStates;
using UnityEngine;

namespace StateMachine.Agents.Simulation
{
    public class Scavenger : SimAgent
    {
        public float Speed;
        public float RotSpeed = 20.0f;
        private int turnLeftCount;
        private int turnRightCount;
        public override void Init()
        {
            base.Init();
            agentType = SimAgentTypes.Scavenger;
            foodTarget = SimNodeType.Carrion;
            FoodLimit = 20;
            movement = 5;
            Speed = movement * Graph<SimNode<Vector2>, NodeVoronoi, Vector2>.CellSize;
            brainTypes = new[] {BrainType.Movement, BrainType.Eat};

        }
        
        protected override void MovementInputs()
        {
            int brain = (int)BrainType.ScavengerMovement;
            
            input[brain][0] = CurrentNode.GetCoordinate().x;
            input[brain][1] = CurrentNode.GetCoordinate().y;
            
            SimAgent target = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, CurrentNode);
            input[brain][2] = target.CurrentNode.GetCoordinate().x;
            input[brain][3] = target.CurrentNode.GetCoordinate().y;
            
            SimNode<Vector2> nodeTarget = GetTarget(foodTarget);
            input[brain][4] = nodeTarget.GetCoordinate().x;
            input[brain][5] = nodeTarget.GetCoordinate().y;
            
            input[brain][6] = Food;

        }

        // TODO check for flocking inputs
        protected override void ExtraInputs()
        {
            int brain = (int)BrainType.Flocking;
            input[brain][0] = CurrentNode.GetCoordinate().x;
            input[brain][1] = CurrentNode.GetCoordinate().y;
        }

        protected override void ExtraBehaviours()
        {
            Fsm.AddBehaviour<SimEatState>(Behaviours.Eat, EatTickParameters);
        }

        protected override void Move()
        {
            float leftForce = output[(int)BrainType.ScavengerMovement][0];
            float rightForce = output[(int)BrainType.ScavengerMovement][1];
            
            var pos = transform.position;
            var rotFactor = Mathf.Clamp(rightForce - leftForce, -1.0f, 1.0f);
            transform.rotation *= Quaternion.AngleAxis(rotFactor * RotSpeed * dt, Vector3.up);
            pos += transform.forward * (Mathf.Abs(rightForce + leftForce) * 0.5f * Speed * dt);
            transform.position = pos;

            if (rightForce > leftForce)
            {
                turnRightCount++;
                turnLeftCount = 0;
            }
            else
            {
                turnLeftCount++;
                turnRightCount = 0;
            }
        }
        

        protected override object[] WalkTickParameters()
        {
            object[] objects = { CurrentNode, TargetNode, transform, foodTarget, OnMove, output[(int)BrainType.ScavengerMovement] };

            return objects;
        }
    }
}