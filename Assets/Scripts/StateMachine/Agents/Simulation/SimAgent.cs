using System;
using FlappyIa.GeneticAlg;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using StateMachine.States.SimStates;
using Utils;

namespace StateMachine.Agents.Simulation
{
    public enum SimAgentTypes
    {
        Carnivorous,
        Herbivore,
        Scavenger
    }

    public class SimAgent<TVector, TTransform> 
        where TVector : IVector, IEquatable<TVector>
        where TTransform : ITransform<TVector>
    {
        public enum Behaviours
        {
            Walk,
            Escape,
            Eat,
            Attack
        }

        public enum Flags
        {
            OnTargetLost,
            OnEscape,
            OnEat,
            OnSearchFood,
            OnAttack
        }

        public TTransform transform;
        public ICoordinate<TVector> CurrentNode;
        public bool CanReproduce() => Food >= FoodLimit;
        public SimAgentTypes agentType { get; protected set; }
        public FSM<Behaviours, Flags> Fsm;

        protected int movement = 3;
        protected SimNodeType foodTarget;
        protected int FoodLimit = 5;
        protected int Food = 0;
        protected Action OnMove;
        protected Action OnEat;
        protected float dt;

        protected SimNode<TVector> TargetNode
        {
            get => targetNode;
            set => targetNode = value;
        }

        private SimNode<TVector> targetNode;
        Genome[] genomes;
        public float[][] output;
        public float[][] input;
        public BrainType[] brainTypes;

        public SimAgent()
        {
        }

        public SimAgent(SimAgentTypes agentType)
        {
            this.agentType = agentType;
        }

        public virtual void Init()
        {
            int brainTypesCount = Enum.GetValues(typeof(BrainType)).Length;
            input = new float[brainTypesCount][];
            output = new float[brainTypesCount][];
            brainTypes = new BrainType[brainTypesCount];

            const int MaxInputs = 8;
            for (int i = 0; i < brainTypesCount; i++)
            {
                input[i] = new float[MaxInputs]; // Assuming each brain type requires 4 inputs
                output[i] = new float[MaxInputs]; // Assuming each brain type produces 4 outputs
            }           
            
            Fsm = new FSM<Behaviours, Flags>();

            OnMove += Move;
            OnEat += Eat;

            FsmBehaviours();

            FsmTransitions();

            //UpdateInputs();
        }

        public virtual void Uninit()
        {
            OnMove -= Move;
            OnEat -= Eat;
        }

        public void Tick(float deltaTime)
        {
            dt = deltaTime;
            Fsm.Tick();
        }

        public virtual void UpdateInputs()
        {
            FindFoodInputs();
            MovementInputs();
            ExtraInputs();
        }


        private void FindFoodInputs()
        {
            int brain = (int)BrainType.Eat;
            input[brain][0] = CurrentNode.GetCoordinate().x;
            input[brain][1] = CurrentNode.GetCoordinate().y;
            SimNode<TVector> target = GetTarget(foodTarget);
            input[brain][2] = target.GetCoordinate().x;
            input[brain][3] = target.GetCoordinate().y;
        }

        protected virtual void MovementInputs()
        {
        }

        protected virtual void ExtraInputs()
        {
        }

        protected virtual void FsmTransitions()
        {
            WalkTransitions();
            EatTransitions();
            ExtraTransitions();
        }

        protected virtual void WalkTransitions()
        {
        }

        protected virtual void EatTransitions()
        {
        }

        protected virtual void ExtraTransitions()
        {
        }

        protected virtual void FsmBehaviours()
        {
            Fsm.AddBehaviour<SimWalkState>(Behaviours.Walk, WalkTickParameters);
            ExtraBehaviours();
        }

        protected virtual void ExtraBehaviours()
        {
        }

        protected virtual object[] WalkTickParameters()
        {
            int extraBrain = agentType == SimAgentTypes.Carnivorous ? (int)BrainType.Attack : (int)BrainType.Escape;
            object[] objects =
            {
                CurrentNode, TargetNode, transform, foodTarget, OnMove, output[(int)BrainType.Movement],
                output[extraBrain]
            };
            return objects;
        }

        protected virtual object[] WalkEnterParameters()
        {
            object[] objects = { };
            return objects;
        }


        protected virtual object[] EatTickParameters()
        {
            object[] objects = { CurrentNode, foodTarget, OnEat, output[0], output[1] };
            return objects;
        }

        private void Eat() => Food++;

        protected virtual void Move()
        {
            if (CurrentNode == null || TargetNode == null) return;

            if (CurrentNode.GetCoordinate().Equals(TargetNode.GetCoordinate())) return;

            int brain = (int)BrainType.Movement;
            var targetPos = CurrentNode.GetCoordinate();
            float speed = CalculateSpeed(output[brain][2]);

            targetPos = CalculateNewPosition(targetPos, output[brain], speed);
            if (targetPos.Equals(targetPos.zero())) CurrentNode = GetNode(targetPos);
        }

        private float CalculateSpeed(float rawSpeed)
        {
            if (rawSpeed < 1) return movement;
            if (rawSpeed < 0) return movement - 1;
            if (rawSpeed < -0.6) return movement - 2;
            return rawSpeed;
        }

        private TVector CalculateNewPosition(TVector targetPos, float[] brainOutput, float speed)
        {
            if (brainOutput[0] > 0)
            {
                if (brainOutput[1] > 0.1) // Right
                {
                    targetPos.x += speed;
                }
                else if (brainOutput[1] < -0.1) // Left
                {
                    targetPos.x -= speed;
                }
            }
            else
            {
                if (brainOutput[1] > 0.1) // Up
                {
                    targetPos.y += speed;
                }
                else if (brainOutput[1] < -0.1) // Down
                {
                    targetPos.y -= speed;
                }
            }

            return targetPos;
        }

        // TODO cosas rojas
        protected virtual SimNode<TVector> GetTarget(SimNodeType nodeType = SimNodeType.Empty)
        {
            TVector position = transform.position;
            SimNode<TVector> nearestNode = null;
            float minDistance = float.MaxValue;

            foreach (var node in EcsPopulationManager.graph.NodesType)
            {
                if (node.NodeType != nodeType) continue;
                float distance = position.Distance(node.GetCoordinate());
                if (!(distance < minDistance)) continue;

                minDistance = distance;
                nearestNode = node;
            }

            if (nodeType != SimNodeType.Corpse || nearestNode != null) return nearestNode;

            var nodeVoronoi = EcsPopulationManager.GetNearestEntity(SimAgentTypes.Herbivore, CurrentNode).CurrentNode;
            nearestNode = EcsPopulationManager.CoordinateToNode(nodeVoronoi);


            return nearestNode;
        }

        protected virtual ICoordinate<TVector> GetNode(TVector position)
        {
            return EcsPopulationManager.graph.CoordNodes[(int)position.x, (int)position.y];
        }
    }
}