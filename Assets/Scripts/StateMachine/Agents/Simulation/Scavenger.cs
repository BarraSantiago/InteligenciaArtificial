using Flocking;
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
        public Boid boid;
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
            brainTypes = new[] { BrainType.Movement, BrainType.Eat };
            boid = GetComponent<Boid>();
        }

        protected override void FsmBehaviours()
        {
            Fsm.AddBehaviour<SimWalkScavState>(Behaviours.Walk, WalkTickParameters);
            ExtraBehaviours();
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

        protected override void ExtraInputs()
        {
            int brain = (int)BrainType.Flocking;

            input[brain][0] = CurrentNode.GetCoordinate().x;
            input[brain][1] = CurrentNode.GetCoordinate().y;

            // Current direction of the boid
            input[brain][2] = transform.forward.x;
            input[brain][3] = transform.forward.y;

            // Average position of neighboring boids
            Vector2 avgNeighborPosition = GetAverageNeighborPosition();
            input[brain][4] = avgNeighborPosition.x;
            input[brain][5] = avgNeighborPosition.y;

            // Average direction of neighboring boids
            Vector2 avgNeighborVelocity = GetAverageNeighborDirection();
            input[brain][6] = avgNeighborVelocity.x;
            input[brain][7] = avgNeighborVelocity.y;

            // Separation vector
            Vector2 separationVector = GetSeparationVector();
            input[brain][8] = separationVector.x;
            input[brain][9] = separationVector.y;

            // Alignment vector
            Vector2 alignmentVector = GetAlignmentVector();
            input[brain][10] = alignmentVector.x;
            input[brain][11] = alignmentVector.y;

            // Cohesion vector
            Vector2 cohesionVector = GetCohesionVector();
            input[brain][12] = cohesionVector.x;
            input[brain][13] = cohesionVector.y;

            // Distance to target
            Vector2 targetPosition = GetTargetPosition();
            input[brain][14] = targetPosition.x;
            input[brain][15] = targetPosition.y;
            boid.target.position = targetPosition;
        }

        protected override void ExtraBehaviours()
        {
            Fsm.AddBehaviour<SimEatState>(Behaviours.Eat, EatTickParameters);
        }

        private Vector2 GetAverageNeighborPosition()
        {
            var nearBoids = EcsPopulationManager.GetBoidsInsideRadius(boid);

            Vector2 avg = Vector2.zero;
            foreach (var boid in nearBoids)
            {
                avg += (Vector2)boid.transform.position;
            }

            avg /= nearBoids.Count;
            return avg;
        }

        private Vector2 GetAverageNeighborDirection()
        {
            var nearBoids = EcsPopulationManager.GetBoidsInsideRadius(boid);

            Vector2 avg = Vector2.zero;
            foreach (var boid in nearBoids)
            {
                avg += (Vector2)boid.transform.forward;
            }

            avg /= nearBoids.Count;
            return avg;
        }

        private Vector2 GetSeparationVector()
        {
            return boid.GetSeparation();
        }

        private Vector2 GetAlignmentVector()
        {
            return boid.GetAlignment();
        }

        private Vector2 GetCohesionVector()
        {
            return boid.GetCohesion();
        }

        private Vector2 GetTargetPosition()
        {
            return GetTarget(foodTarget).GetCoordinate();
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


        protected override SimNode<Vector2> GetTarget(SimNodeType nodeType = SimNodeType.Empty)
        {
            SimNode<Vector2> target = null;
            if (nodeType == SimNodeType.Carrion)
            {
                target = EcsPopulationManager.GetNearestNode(SimNodeType.Carrion, CurrentNode);
            }

            target ??= EcsPopulationManager.GetNearestNode(SimNodeType.Corpse, CurrentNode);

            target ??= EcsPopulationManager.CoordinateToNode(EcsPopulationManager
                .GetNearestEntity(SimAgentTypes.Carnivorous, CurrentNode).CurrentNode);

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