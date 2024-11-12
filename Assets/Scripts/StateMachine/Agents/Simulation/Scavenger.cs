using System;
using Flocking;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using StateMachine.States.SimStates;
using Utils;

namespace StateMachine.Agents.Simulation
{
    public class Scavenger<TVector, TTransform> : SimAgent<TVector, TTransform>
        where TTransform : ITransform<IVector>, new()
        where TVector : IVector, IEquatable<TVector>
    {
        public Boid<IVector, ITransform<IVector>> boid = new Boid<IVector, ITransform<IVector>>();
        public float cellSize;
        public float Speed;
        public float RotSpeed = 20.0f;
        private IVector targetPosition = new MyVector();
        public override INode<IVector> CurrentNode {
            get => currentNode;
            set
            {
                currentNode = value;
                transform.position = value.GetCoordinate();
                boid.transform.position = value.GetCoordinate();
                boid.transform.up = (targetPosition - CurrentNode.GetCoordinate()).Normalized();
            }
            
        }

        public override void Init()
        {
            targetPosition = GetTargetPosition();
            boid = new Boid<IVector, ITransform<IVector>>
            {
                transform = transform,
                target = targetPosition,
            };
            base.Init();
            agentType = SimAgentTypes.Scavenger;
            foodTarget = SimNodeType.Carrion;
            FoodLimit = 20;
            movement = 5;
            Speed = movement * cellSize;
            brainTypes = new[] { BrainType.Movement, BrainType.Eat };
        }

        protected override void FsmBehaviours()
        {
            Fsm.AddBehaviour<SimWalkScavState>(Behaviours.Walk, WalkTickParameters);
            ExtraBehaviours();
        }

        protected override void MovementInputs()
        {
            int brain = (int)BrainType.ScavengerMovement;

            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;

            var target = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, CurrentNode);
            if (target == null || target.CurrentNode == null)
            {
                input[brain][2] = NoTarget;
                input[brain][3] = NoTarget;
            }
            else
            {
                input[brain][2] = target.CurrentNode.GetCoordinate().X;
                input[brain][3] = target.CurrentNode.GetCoordinate().Y;
            }

            INode<IVector> nodeTarget = GetTarget(foodTarget);
            if (nodeTarget == null)
            {
                input[brain][4] = NoTarget;
                input[brain][5] = NoTarget;
            }
            else
            {
                input[brain][4] = nodeTarget.GetCoordinate().X;
                input[brain][5] = nodeTarget.GetCoordinate().Y;
            }

            input[brain][6] = Food;
        }

        protected override void ExtraInputs()
        {
            int brain = (int)BrainType.Flocking;
            targetPosition = GetTargetPosition();

            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;

            // Current direction of the boid
            if (targetPosition != null)
            {
                IVector direction = (targetPosition - CurrentNode.GetCoordinate()).Normalized();
                input[brain][2] = direction.X;
                input[brain][3] = direction.Y;
            }
            else
            {
                input[brain][2] = NoTarget;
                input[brain][3] = NoTarget;
            }

            // Average position of neighboring boids
            IVector avgNeighborPosition = GetAverageNeighborPosition();
            input[brain][4] = avgNeighborPosition.X;
            input[brain][5] = avgNeighborPosition.Y;

            // Average direction of neighboring boids
            IVector avgNeighborVelocity = GetAverageNeighborDirection();
            input[brain][6] = avgNeighborVelocity.X;
            input[brain][7] = avgNeighborVelocity.Y;

            // Separation vector
            IVector separationVector = GetSeparationVector();
            input[brain][8] = separationVector.X;
            input[brain][9] = separationVector.Y;

            // Alignment vector
            IVector alignmentVector = GetAlignmentVector();
            input[brain][10] = alignmentVector.X;
            input[brain][11] = alignmentVector.Y;

            // Cohesion vector
            IVector cohesionVector = GetCohesionVector();
            input[brain][12] = cohesionVector.X;
            input[brain][13] = cohesionVector.Y;

            // Distance to target
            if (targetPosition == null)
            {
                input[brain][14] = NoTarget;
                input[brain][15] = NoTarget;
                return;
            }

            input[brain][14] = targetPosition.X;
            input[brain][15] = targetPosition.Y;
            boid.target = targetPosition;
        }

        protected override void ExtraBehaviours()
        {
            Fsm.AddBehaviour<SimEatState>(Behaviours.Eat, EatTickParameters);
        }

        private IVector GetAverageNeighborPosition()
        {
            var nearBoids = EcsPopulationManager.GetBoidsInsideRadius(boid);

            var avg = MyVector.zero();
            foreach (var boid in nearBoids)
            {
                avg += (MyVector)boid.transform.position;
            }

            avg /= nearBoids.Count;
            return avg;
        }

        private IVector GetAverageNeighborDirection()
        {
            var nearBoids = EcsPopulationManager.GetBoidsInsideRadius(boid);

            var avg = MyVector.zero();
            foreach (var boid1 in nearBoids)
            {
                avg += boid1.GetDirection().Normalized() - boid1.transform.position.Normalized();
            }

            avg /= nearBoids.Count;
            return avg;
        }

        private IVector GetSeparationVector()
        {
            return boid.GetSeparation();
        }

        private IVector GetAlignmentVector()
        {
            return boid.GetAlignment();
        }

        private IVector GetCohesionVector()
        {
            return boid.GetCohesion();
        }

        private IVector GetTargetPosition()
        {
            var targetNode = GetTarget(foodTarget);
            if (targetNode == null)
            {
                return MyVector.NoTarget(); // or any default value
            }

            return targetNode.GetCoordinate();
        }

        protected override void Move()
        {
            float leftForce = output[(int)BrainType.ScavengerMovement][0];
            float rightForce = output[(int)BrainType.ScavengerMovement][1];

            var pos = transform.position;
            var rotFactor = Math.Clamp(rightForce - leftForce, -1.0f, 1.0f);
            //transform.rotation *= Quaternion.AngleAxis(rotFactor * RotSpeed * dt, Vector3.up);
            //pos += transform.forward * (Math.Abs(rightForce + leftForce) * 0.5f * Speed * dt);
            //transform.position = pos;

            if (rightForce > leftForce)
            {
            }
            else
            {
            }
        }


        protected override INode<IVector> GetTarget(SimNodeType nodeType = SimNodeType.Empty)
        {
            INode<IVector> target = EcsPopulationManager.GetNearestNode(nodeType, CurrentNode);
            

            if (target == null)
            {
                target = EcsPopulationManager.GetNearestNode(SimNodeType.Corpse, CurrentNode);
            }

            if (target == null)
            {
                var nearestEntity = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, CurrentNode);
                if (nearestEntity != null)
                {
                    target = nearestEntity.CurrentNode;
                }
            }

            return target;
        }

        protected override object[] WalkTickParameters()
        {
            object[] objects =
                { CurrentNode, transform, foodTarget, OnMove, output[(int)BrainType.ScavengerMovement] };

            return objects;
        }
    }
}