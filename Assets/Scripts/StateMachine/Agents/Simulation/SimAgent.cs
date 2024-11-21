using System;
using System.Collections.Generic;
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
        Carnivore,
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

        public virtual TTransform Transform
        {
            get => transform;
            set
            {
                transform ??= new TTransform();
                transform.position ??= new MyVector(0, 0);

                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Transform value cannot be null");
                }

                if (transform.position == null || value.position == null)
                {
                    throw new InvalidOperationException("Transform positions cannot be null");
                }

                transform.forward = (transform.position - value.position).Normalized();
                transform = value;
            }
        }

        public virtual INode<IVector> CurrentNode =>
            EcsPopulationManager.graph.NodesType[(int)Transform.position.X, (int)Transform.position.Y];

        protected TTransform transform = new TTransform();
        public bool CanReproduce() => Food >= FoodLimit;
        public SimAgentTypes agentType { get; set; }
        public FSM<Behaviours, Flags> Fsm;

        protected int movement = 3;
        protected SimNodeType foodTarget;
        public int FoodLimit { get; protected set; } = 5;
        public int Food { get; protected set; } = 0;
        protected Action OnMove;
        protected Action OnEat;
        protected float dt;
        protected const int NoTarget = -1;

        protected SimNode<TVector> TargetNode
        {
            get => targetNode;
            set => targetNode = value;
        }

        private SimNode<TVector> targetNode;
        Genome[] genomes;
        public float[][] output;
        public float[][] input;
        public Dictionary<int, BrainType> brainTypes = new();

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
            output = new float[brainTypes.Count][];
            foreach (var brain in brainTypes.Values)
            {
                EcsPopulationManager.NeuronInputCount inputsCount =
                    EcsPopulationManager.InputCountCache[(brain, agentType)];
                output[GetBrainTypeKeyByValue(brain)] = new float[inputsCount.outputCount];
            }

            OnMove += Move;
            OnEat += Eat;

            FsmBehaviours();

            FsmTransitions();
            Fsm.ForceTransition(Behaviours.Walk);
        }

        public virtual void Reset()
        {
            Food = 0;
            Fsm.ForceTransition(Behaviours.Walk);
            CalculateInputs();
        }

        protected void CalculateInputs()
        {
            int brainTypesCount = brainTypes.Count;
            input = new float[brainTypesCount][];
            output = new float[brainTypesCount][];

            for (int i = 0; i < brainTypesCount; i++)
            {
                var brainType = brainTypes[i];
                input[i] = new float[GetInputCount(brainType)];
                int outputCount = EcsPopulationManager.InputCountCache[(brainType, agentType)].outputCount;
                output[i] = new float[outputCount];
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
            int brain = GetBrainTypeKeyByValue(BrainType.Eat);
            var inputCount = GetInputCount(BrainType.Eat);
            input[brain] = new float[inputCount];

            input[brain][0] = Transform.position.X;
            input[brain][1] = Transform.position.Y;
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
            int extraBrain = agentType == SimAgentTypes.Carnivore
                ? GetBrainTypeKeyByValue(BrainType.Attack)
                : GetBrainTypeKeyByValue(BrainType.Escape);
            object[] objects =
            {
                CurrentNode, foodTarget, OnMove, output[GetBrainTypeKeyByValue(BrainType.Eat)],
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
            int extraBrain = agentType == SimAgentTypes.Carnivore
                ? GetBrainTypeKeyByValue(BrainType.Attack)
                : GetBrainTypeKeyByValue(BrainType.Escape);

            object[] objects =
                { CurrentNode, foodTarget, OnEat, output[GetBrainTypeKeyByValue(BrainType.Eat)], output[extraBrain] };
            return objects;
        }

        protected virtual void Eat()
        {
            if (CurrentNode.Food <= 0) return;
            Food++;
            CurrentNode.Food--;
            if (CurrentNode.Food <= 0) CurrentNode.NodeType = SimNodeType.Empty;
        }

        protected virtual void Move()
        {
            int brain = GetBrainTypeKeyByValue(BrainType.Movement);

            IVector currentPos = new MyVector(CurrentNode.GetCoordinate().X, CurrentNode.GetCoordinate().Y);
            currentPos = CalculateNewPosition(currentPos, output[brain]);

            if (!EcsPopulationManager.graph.IsWithinGraphBorders(currentPos))
            {
                if (currentPos.X < EcsPopulationManager.graph.MinX)
                {
                    currentPos.X = EcsPopulationManager.graph.MaxX-1;
                }

                if (currentPos.X > EcsPopulationManager.graph.MaxX)
                {
                    currentPos.X = EcsPopulationManager.graph.MinX;
                }

                if (currentPos.Y < EcsPopulationManager.graph.MinY)
                {
                    currentPos.Y = EcsPopulationManager.graph.MaxY-1;
                }

                if (currentPos.Y > EcsPopulationManager.graph.MaxY)
                {
                    currentPos.Y = EcsPopulationManager.graph.MinY;
                }
            }

            var newPos = EcsPopulationManager.CoordinateToNode(currentPos);
            if (newPos != null) SetPosition(newPos.GetCoordinate());
        }

        private float CalculateSpeed(float rawSpeed)
        {
            if (rawSpeed < 1) return movement;
            if (rawSpeed < 0.5) return movement - 1;
            if (rawSpeed < 0.2) return movement - 2;
            return rawSpeed;
        }

        private IVector CalculateNewPosition(IVector targetPos, float[] brainOutput)
        {
            if (brainOutput.Length < 2) return default;
            
            float horizontalMovement = brainOutput[0];
            float verticalMovement = brainOutput[1];

            // Calculate horizontal movement
            if (horizontalMovement > 0.5f)
            {
                targetPos.X += movement * (horizontalMovement - 0.5f) * 2;
            }
            else
            {
                targetPos.X -= movement * (0.5f - horizontalMovement) * 2;
            }

            // Calculate vertical movement
            if (verticalMovement > 0.5f)
            {
                targetPos.Y += movement * (verticalMovement - 0.5f) * 2;
            }
            else
            {
                targetPos.Y -= movement * (0.5f - verticalMovement) * 2;
            }

            return targetPos;
        }

        public virtual INode<IVector> GetTarget(SimNodeType nodeType = SimNodeType.Empty)
        {
            return EcsPopulationManager.GetNearestNode(nodeType, transform.position);
        }

        protected int GetInputCount(BrainType brainType)
        {
            return InputCountCache.GetInputCount(agentType, brainType);
        }

        public virtual void SetPosition(IVector position)
        {
            if (!EcsPopulationManager.graph.IsWithinGraphBorders(position)) return;
            Transform = (TTransform)new ITransform<IVector>(position);
        }

        public int GetBrainTypeKeyByValue(BrainType value)
        {
            foreach (var kvp in brainTypes)
            {
                if (EqualityComparer<BrainType>.Default.Equals(kvp.Value, value))
                {
                    return kvp.Key;
                }
            }

            throw new KeyNotFoundException("The value is not present in the brainTypes dictionary.");
        }
    }
}