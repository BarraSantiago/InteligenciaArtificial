using System;
using System.Linq;
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

        public override TTransform Transform
        {
            get => transform;
            set
            {
                transform.forward = (transform.position - value.position).Normalized();
                transform = value;
                boid.transform.position = value.position;
                boid.transform.forward = (targetPosition - value.position).Normalized();
            }
        }

        public override void Init()
        {
            targetPosition = GetTargetPosition();
            Transform.forward = (targetPosition - Transform.position).Normalized();
            boid = new Boid<IVector, ITransform<IVector>>
            {
                transform = Transform,
                target = targetPosition,
            };

            boid.transform.forward ??= MyVector.zero();

            base.Init();
            foodTarget = SimNodeType.Carrion;
            FoodLimit = 20;
            movement = 5;
            Speed = movement * 1;

            CalculateInputs();
        }

        public override void Reset()
        {
            base.Reset();
            boid = new Boid<IVector, ITransform<IVector>>
            {
                transform = Transform,
                target = targetPosition,
            };
            boid.Init(EcsPopulationManager.flockingManager.Alignment, EcsPopulationManager.flockingManager.Cohesion,
                EcsPopulationManager.flockingManager.Separation, EcsPopulationManager.flockingManager.Direction);
        }


        protected override void MovementInputs()
        {
            int brain = GetBrainTypeKeyByValue(BrainType.ScavengerMovement);
            var inputCount = GetInputCount(BrainType.ScavengerMovement);

            input[brain] = new float[inputCount];
            input[brain][0] = Transform.position.X;
            input[brain][1] = Transform.position.Y;

            var target = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivore, Transform.position);
            if (target == null || target.CurrentNode == null)
            {
                input[brain][2] = NoTarget;
                input[brain][3] = NoTarget;
            }
            else
            {
                input[brain][2] = target.Transform.position.X;
                input[brain][3] = target.Transform.position.Y;
            }

            targetPosition = GetTargetPosition();
            if (targetPosition == null)
            {
                input[brain][4] = NoTarget;
                input[brain][5] = NoTarget;
            }
            else
            {
                input[brain][4] = targetPosition.X;
                input[brain][5] = targetPosition.Y;
            }

            input[brain][6] = Food;
        }

        protected override void ExtraInputs()
        {
            int brain = GetBrainTypeKeyByValue(BrainType.Flocking);
            var inputCount = GetInputCount(BrainType.Flocking);
            input[brain] = new float[inputCount];

            targetPosition = GetTargetPosition();

            input[brain][0] = Transform.position.X;
            input[brain][1] = Transform.position.Y;

            if (targetPosition != null)
            {
                IVector direction = (targetPosition - Transform.position).Normalized();
                input[brain][2] = direction.X;
                input[brain][3] = direction.Y;
            }
            else
            {
                input[brain][2] = NoTarget;
                input[brain][3] = NoTarget;
            }

            IVector avgNeighborPosition = GetAverageNeighborPosition();
            input[brain][4] = avgNeighborPosition.X;
            input[brain][5] = avgNeighborPosition.Y;

            IVector avgNeighborVelocity = GetAverageNeighborDirection();
            input[brain][6] = avgNeighborVelocity.X;
            input[brain][7] = avgNeighborVelocity.Y;

            IVector separationVector = GetSeparationVector();
            input[brain][8] = separationVector.X;
            input[brain][9] = separationVector.Y;

            IVector alignmentVector = GetAlignmentVector();
            if (alignmentVector == null)
            {
                input[brain][10] = NoTarget;
                input[brain][11] = NoTarget;
            }
            else
            {
                input[brain][10] = alignmentVector.X;
                input[brain][11] = alignmentVector.Y;
            }

            IVector cohesionVector = GetCohesionVector();
            if (cohesionVector == null)
            {
                input[brain][12] = NoTarget;
                input[brain][13] = NoTarget;
            }
            else
            {
                input[brain][12] = cohesionVector.X;
                input[brain][13] = cohesionVector.Y;
            }

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


        private IVector GetAverageNeighborPosition()
        {
            var nearBoids = EcsPopulationManager.GetBoidsInsideRadius(boid);

            if (nearBoids.Count == 0)
            {
                return MyVector.zero();
            }

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

            if (nearBoids.Count == 0)
            {
                return MyVector.zero();
            }

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
            int index = GetBrainTypeKeyByValue(BrainType.ScavengerMovement);
            if (output[index].Length != 2) return;
            float leftForce = output[index][0];
            float rightForce = output[index][1];

            var pos = Transform.position;
            //var rotFactor = Math.Clamp(rightForce - leftForce, -1.0f, 1.0f);
            //transform.rotation *= Quaternion.AngleAxis(rotFactor * RotSpeed * dt, Vector3.up);
            //pos += transform.forward * (Math.Abs(rightForce + leftForce) * 0.5f * Speed * dt);
            //transform.position = pos;

            leftForce = (leftForce - 0.5f) * 2.0f;

            rightForce = (rightForce - 0.5f) * 2.0f;

            var currentPos = new MyVector(Transform.position.X, Transform.position.Y);
            currentPos.X += rightForce;
            currentPos.Y += leftForce;

            if (!EcsPopulationManager.graph.IsWithinGraphBorders(currentPos))
            {
                if (currentPos.X < EcsPopulationManager.graph.MinX)
                {
                    currentPos.X = EcsPopulationManager.graph.MaxX;
                }

                if (currentPos.X > EcsPopulationManager.graph.MaxX)
                {
                    currentPos.X = EcsPopulationManager.graph.MinX;
                }

                if (currentPos.Y < EcsPopulationManager.graph.MinY)
                {
                    currentPos.Y = EcsPopulationManager.graph.MaxY;
                }

                if (currentPos.Y > EcsPopulationManager.graph.MaxY)
                {
                    currentPos.Y = EcsPopulationManager.graph.MinY;
                }
            }

            SetPosition(currentPos);
        }

        public override void SetPosition(IVector position)
        {
            Transform = (TTransform)new ITransform<IVector>(position);
        }

        public override INode<IVector> GetTarget(SimNodeType nodeType = SimNodeType.Empty)
        {
            INode<IVector> target = EcsPopulationManager.GetNearestNode(nodeType, Transform.position);


            if (target == null)
            {
                target = EcsPopulationManager.GetNearestNode(SimNodeType.Corpse, Transform.position);
            }

            if (target == null)
            {
                var nearestEntity = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Carnivore, Transform.position);
                if (nearestEntity != null)
                {
                    target = nearestEntity.CurrentNode;
                }
            }

            return target;
        }

        protected override void FsmBehaviours()
        {
            Fsm.AddBehaviour<SimWalkScavState>(Behaviours.Walk, WalkTickParameters);
            ExtraBehaviours();
        }

        protected override void ExtraBehaviours()
        {
            Fsm.AddBehaviour<SimEatScavState>(Behaviours.Eat, EatTickParameters);
        }

        protected override object[] EatTickParameters()
        {
            object[] objects =
            {
                Transform.position,
                EcsPopulationManager.graph.NodesType[(int)targetPosition.X, (int)targetPosition.Y],
                OnEat,
                output[GetBrainTypeKeyByValue(BrainType.Eat)]
            };
            return objects;
        }

        protected override object[] WalkTickParameters()
        {
            object[] objects =
                { Transform.position, targetPosition, OnMove, output[GetBrainTypeKeyByValue(BrainType.Eat)] };

            return objects;
        }

        protected override void EatTransitions()
        {
            Fsm.SetTransition(Behaviours.Eat, Flags.OnEat, Behaviours.Eat);
            Fsm.SetTransition(Behaviours.Eat, Flags.OnSearchFood, Behaviours.Walk);
        }

        protected override void WalkTransitions()
        {
            Fsm.SetTransition(Behaviours.Walk, Flags.OnEat, Behaviours.Eat);
            Fsm.SetTransition(Behaviours.Walk, Flags.OnSearchFood, Behaviours.Walk);
        }
    }
}