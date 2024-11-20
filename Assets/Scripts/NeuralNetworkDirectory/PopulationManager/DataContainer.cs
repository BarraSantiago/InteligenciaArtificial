using System;
using System.Collections.Generic;
using System.Linq;
using ECS.Patron;
using FlappyIa.GeneticAlg;
using Flocking;
using NeuralNetworkDirectory.AI;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder.Graph;
using StateMachine.Agents.Simulation;
using Utils;

namespace NeuralNetworkDirectory.PopulationManager
{
    using SimAgentType = SimAgent<IVector, ITransform<IVector>>;

    public class DataContainer
    {
        public struct NeuronInputCount
        {
            public SimAgentTypes agentType;
            public BrainType brainType;
            public int inputCount;
            public int outputCount;
            public int[] hiddenLayersInputs;
        }

        public TurnManager TurnManager = new();
        public SimulationManager SimulationManager = new();
        public static EpochManager EpochManager = new();
        public static EntitiesManager EntitiesManager = new();
        
        public int gridWidth = 50;
        public int gridHeight = 50;
        public int generationTurns = 100;
        public float speed = 1.0f;
        public static Sim2Graph graph;
        public static NeuronInputCount[] inputCounts;
        public static Dictionary<(BrainType, SimAgentTypes), NeuronInputCount> InputCountCache;
        public static FlockingManager flockingManager = new();

        public const float Bias = 0.0f;
        public const float SigmoidP = .5f;
        public static bool isRunning = true;
        
        public int plantCount;
        public int currentTurn;
        public int behaviourCount;
        public const int CellSize = 1;
        public float accumTime;
        public static GeneticAlgorithm genAlg;
        public static GraphManager<IVector, ITransform<IVector>> gridManager;
        public FitnessManager<IVector, ITransform<IVector>> fitnessManager;
        public static Dictionary<uint, SimAgentType> _agents = new();
        public static Dictionary<uint, Dictionary<BrainType, List<Genome>>> _population = new();
        public static Dictionary<uint, Scavenger<IVector, ITransform<IVector>>> _scavengers = new();
        public static Dictionary<int, BrainType> herbBrainTypes = new();
        public static Dictionary<int, BrainType> scavBrainTypes = new();
        public static Dictionary<int, BrainType> carnBrainTypes = new();
        public static readonly int BrainsAmount = Enum.GetValues(typeof(BrainType)).Length;

        public void Initialize()
        {
            herbBrainTypes[0] = BrainType.Eat;
            herbBrainTypes[1] = BrainType.Movement;
            herbBrainTypes[2] = BrainType.Escape;

            scavBrainTypes[0] = BrainType.Eat;
            scavBrainTypes[1] = BrainType.ScavengerMovement;
            scavBrainTypes[2] = BrainType.Flocking;

            carnBrainTypes[0] = BrainType.Eat;
            carnBrainTypes[1] = BrainType.Movement;
            carnBrainTypes[2] = BrainType.Attack;


            inputCounts = new NeuronInputCount[]
            {
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Carnivore, brainType = BrainType.Eat, inputCount = 4, outputCount = 1,
                    hiddenLayersInputs = new[] { 1 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Carnivore, brainType = BrainType.Movement, inputCount = 7,
                    outputCount = 3, hiddenLayersInputs = new[] { 3 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Carnivore, brainType = BrainType.Attack, inputCount = 4,
                    outputCount = 1, hiddenLayersInputs = new[] { 1 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Herbivore, brainType = BrainType.Eat, inputCount = 4, outputCount = 1,
                    hiddenLayersInputs = new[] { 1 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Herbivore, brainType = BrainType.Movement, inputCount = 8,
                    outputCount = 2, hiddenLayersInputs = new[] { 3 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Herbivore, brainType = BrainType.Escape, inputCount = 4, outputCount = 1,
                    hiddenLayersInputs = new[] { 1 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Scavenger, brainType = BrainType.Eat, inputCount = 4, outputCount = 1,
                    hiddenLayersInputs = new[] { 1 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Scavenger, brainType = BrainType.ScavengerMovement, inputCount = 7,
                    outputCount = 2, hiddenLayersInputs = new[] { 3 }
                },
                new NeuronInputCount
                {
                    agentType = SimAgentTypes.Scavenger, brainType = BrainType.Flocking, inputCount = 16,
                    outputCount = 4,
                    hiddenLayersInputs = new[] { 12, 8, 6, 4 }
                },
            };
            InputCountCache = inputCounts.ToDictionary(input => (input.brainType, input.agentType));
            gridManager = new GraphManager<IVector, ITransform<IVector>>(gridWidth, gridHeight);
            graph = new Sim2Graph(gridWidth, gridHeight, CellSize);
            plantCount = _agents.Values.Count(agent => agent.agentType == SimAgentTypes.Herbivore) * 2;
            SimulationManager.StartSimulation(4, 0.1f, 0.01f, 5,5,5);
            fitnessManager = new FitnessManager<IVector, ITransform<IVector>>(_agents);
            behaviourCount = GetHighestBehaviourCount();
        }

        private int GetHighestBehaviourCount()
        {
            int highestCount = 0;

            foreach (var entity in _agents.Values)
            {
                int multiThreadCount = entity.Fsm.GetMultiThreadCount();
                int mainThreadCount =
                    entity.Fsm.GetMainThreadCount(); // Assuming a similar method exists for main thread count

                int maxCount = Math.Max(multiThreadCount, mainThreadCount);
                if (maxCount > highestCount)
                {
                    highestCount = maxCount;
                }
            }

            return highestCount;
        }

        public static int GetBrainTypeKeyByValue(BrainType value, SimAgentTypes agentType)
        {
            var brainTypes = agentType switch
            {
                SimAgentTypes.Carnivore => carnBrainTypes,
                SimAgentTypes.Herbivore => herbBrainTypes,
                SimAgentTypes.Scavenger => scavBrainTypes,
                _ => throw new ArgumentException("Invalid agent type")
            };
            foreach (var kvp in brainTypes)
            {
                if (EqualityComparer<BrainType>.Default.Equals(kvp.Value, value))
                {
                    return kvp.Key;
                }
            }

            throw new KeyNotFoundException("The value is not present in the brainTypes dictionary.");
        }
        
        public static void RemoveEntity(SimAgentType simAgent)
        {
            EpochManager.CountMissing(simAgent.agentType);
            uint agentId = _agents.FirstOrDefault(agent => agent.Value == simAgent).Key;
            _agents.Remove(agentId);
            _population.Remove(agentId);
            _scavengers.Remove(agentId);
            ECSManager.RemoveEntity(agentId);
        }
    }
}