using System;
using System.Linq;
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

    public enum Flags
    {
        OnTargetLost,
        OnEscape,
        OnEat,
        OnSearchFood,
        OnAttack
    }

    public class SimAgent<TVector, TTransform>
        where TVector : IVector, IEquatable<TVector>
        where TTransform : ITransform<IVector>, new()
    {
        public enum Behaviours
        {
            Walk,
            Escape,
            Eat,
            Attack
        }

        public TTransform transform = new();

        public virtual INode<IVector> CurrentNode
        {
            get => currentNode;
            set
            {
                currentNode = value;
                transform.position = value.GetCoordinate();
            }
        }

        protected INode<IVector> currentNode = new SimNode<IVector>();
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
        protected const int NoTarget = -99999;
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
            Fsm = new FSM<Behaviours, Flags>();

            OnMove += Move;
            OnEat += Eat;

            FsmBehaviours();

            FsmTransitions();

            //UpdateInputs();
        }

        protected void CalculateInputs()
        {
            int brainTypesCount = Enum.GetValues(typeof(BrainType)).Length;
            input = new float[brainTypesCount][];
            output = new float[brainTypesCount][];
            brainTypes = new BrainType[brainTypesCount];

            const int MaxOutputs = 4;
            for (int i = 0; i < brainTypesCount; i++)
            {
                var inputCount = EcsPopulationManager.inputCounts
                    .FirstOrDefault(input => input.agentType == agentType && input.brainType == (BrainType)i).inputCount;
                input[i] = new float[inputCount]; 
                output[i] = new float[MaxOutputs];
            }
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
            var inputCount = EcsPopulationManager.inputCounts
                .FirstOrDefault(input => input.agentType == agentType && input.brainType == (BrainType)brain).inputCount;
            input[brain] = new float[inputCount]; 
            input[brain][0] = CurrentNode.GetCoordinate().X;
            input[brain][1] = CurrentNode.GetCoordinate().Y;
            INode<IVector> target = GetTarget(foodTarget);
            
            if (target == null)
            {
                input[brain][2] = NoTarget;
                input[brain][3] = NoTarget;
                return;
            }

            input[brain][2] = target.GetCoordinate().X;
            input[brain][3] = target.GetCoordinate().Y;
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

            if (!targetPos.Equals(null)) CurrentNode = EcsPopulationManager.CoordinateToNode(targetPos);
        }

        private float CalculateSpeed(float rawSpeed)
        {
            if (rawSpeed < 1) return movement;
            if (rawSpeed < 0) return movement - 1;
            if (rawSpeed < -0.6) return movement - 2;
            return rawSpeed;
        }

        private IVector CalculateNewPosition(IVector targetPos, float[] brainOutput, float speed)
        {
            if (brainOutput[0] > 0)
            {
                if (brainOutput[1] > 0.1) // Right
                {
                    targetPos.X += speed;
                }
                else if (brainOutput[1] < -0.1) // Left
                {
                    targetPos.X -= speed;
                }
            }
            else
            {
                if (brainOutput[1] > 0.1) // Up
                {
                    targetPos.Y += speed;
                }
                else if (brainOutput[1] < -0.1) // Down
                {
                    targetPos.Y -= speed;
                }
            }

            return targetPos;
        }

        protected virtual INode<IVector> GetTarget(SimNodeType nodeType = SimNodeType.Empty)
        {
            return EcsPopulationManager.GetNearestNode(nodeType, CurrentNode);
        }
    }
}