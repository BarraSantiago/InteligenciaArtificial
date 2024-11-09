using System;
using Flocking;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using StateMachine.States.SimStates;
using Utils;

namespace StateMachine.Agents.Simulation
{
    public class Scavenger<TVector, TTransform> : SimAgent<TVector,TTransform> 
        where TTransform : ITransform<TVector>
        where TVector : IVector, IEquatable<TVector>
    {
        public Boid<IVector, ITransform<IVector>> boid;
        public float cellSize;
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
            Speed = movement * cellSize;
            brainTypes = new[] { BrainType.Movement, BrainType.Eat };
            boid = new Boid<IVector, ITransform<IVector>>();
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
            input[brain][2] = target.CurrentNode.GetCoordinate().X;
            input[brain][3] = target.CurrentNode.GetCoordinate().Y;

            INode<IVector> nodeTarget = GetTarget(foodTarget);
            input[brain][4] = nodeTarget.GetCoordinate().X;
            input[brain][5] = nodeTarget.GetCoordinate().Y;

            input[brain][6] = Food;
        }

        protected override void ExtraInputs()
        {
            int brain = (int)BrainType.Flocking;

            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;

            // Current direction of the boid
            input[brain][2] = transform.forward.X;
            input[brain][3] = transform.forward.Y;

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
            IVector targetPosition = GetTargetPosition();
            input[brain][14] = targetPosition.X;
            input[brain][15] = targetPosition.Y;
            boid.target.position = targetPosition;
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
            foreach (var boid in nearBoids)
            {
                avg += (MyVector)boid.transform.forward;
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
            return GetTarget(foodTarget).GetCoordinate();
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
                turnRightCount++;
                turnLeftCount = 0;
            }
            else
            {
                turnLeftCount++;
                turnRightCount = 0;
            }
        }


        protected override INode<IVector> GetTarget(SimNodeType nodeType = SimNodeType.Empty)
        {
            INode<IVector> target = null;
            if (nodeType == SimNodeType.Carrion)
            {
                target = EcsPopulationManager.GetNearestNode(SimNodeType.Carrion, CurrentNode);
            }

            target ??= EcsPopulationManager.GetNearestNode(SimNodeType.Corpse, CurrentNode);

            target ??= EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivorous, CurrentNode).CurrentNode;

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