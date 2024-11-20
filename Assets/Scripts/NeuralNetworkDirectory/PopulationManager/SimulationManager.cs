using System;
using System.Collections.Generic;
using System.Linq;
using ECS.Patron;
using FlappyIa.GeneticAlg;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using StateMachine.Agents.Simulation;
using Utils;

namespace NeuralNetworkDirectory.PopulationManager
{
    using SimAgentType = SimAgent<IVector, ITransform<IVector>>;

    public class SimulationManager
    {
        private void GenerateInitialPopulation(int herbivoreCount, int carnivoreCount, int scavengerCount)
        {
            DataContainer._population.Clear();

            CreateAgents(herbivoreCount, SimAgentTypes.Herbivore);
            CreateAgents(carnivoreCount, SimAgentTypes.Carnivore);
            CreateAgents(scavengerCount, SimAgentTypes.Scavenger);

        }
        
        public void CreateAgents(int count, SimAgentTypes agentType)
        {
            for (var i = 0; i < count; i++)
            {
                var entityID = ECSManager.CreateEntity();
                var neuralNetComponent = new NeuralNetComponent();
                var inputComponent = new InputComponent();
                ECSManager.AddComponent(entityID, inputComponent);
                ECSManager.AddComponent(entityID, neuralNetComponent);

                var num = agentType switch
                {
                    SimAgentTypes.Carnivore => DataContainer.carnBrainTypes,
                    SimAgentTypes.Herbivore => DataContainer.herbBrainTypes,
                    SimAgentTypes.Scavenger => DataContainer.scavBrainTypes,
                    _ => throw new ArgumentException("Invalid agent type")
                };

                ECSManager.AddComponent(entityID, new OutputComponent(agentType, num));

                var brains = CreateBrain(agentType);
                var genomes = new Dictionary<BrainType, List<Genome>>();

                foreach (var brain in brains)
                {
                    BrainType brainType = BrainType.Movement;
                    var genome =
                        new Genome(brain.Layers.Sum(layerList => layerList.Sum(layer => layer.GetWeights().Length)));
                    foreach (var layerList in brain.Layers)
                    {
                        foreach (var layer in layerList)
                        {
                            brainType = layer.BrainType;
                            layer.SetWeights(genome.genome, 0);
                        }
                    }

                    if (!genomes.ContainsKey(brainType))
                    {
                        genomes[brainType] = new List<Genome>();
                    }

                    genomes[brainType].Add(genome);
                }

                inputComponent.inputs = new float[brains.Count][];
                neuralNetComponent.Layers = brains.SelectMany(brain => brain.Layers).ToList();
                neuralNetComponent.Fitness = new float[DataContainer.BrainsAmount];
                neuralNetComponent.FitnessMod = new float[DataContainer.BrainsAmount];

                for (int j = 0; j < neuralNetComponent.FitnessMod.Length; j++)
                {
                    neuralNetComponent.FitnessMod[j] = 1.0f;
                }

                var agent = CreateAgent(agentType);
                DataContainer._agents[entityID] = agent;

                if (agentType == SimAgentTypes.Scavenger)
                {
                    DataContainer._scavengers[entityID] = (Scavenger<IVector, ITransform<IVector>>)agent;
                }

                foreach (var brain in agent.brainTypes.Values)
                {
                    if (!DataContainer._population.ContainsKey(entityID))
                    {
                        DataContainer._population[entityID] = new Dictionary<BrainType, List<Genome>>();
                    }

                    DataContainer._population[entityID][brain] = genomes[brain];
                }
            }
        }

        // SIMULATION
        private SimAgentType CreateAgent(SimAgentTypes agentType)
        {
            var randomNode = DataContainer.gridManager.GetRandomPosition();

            SimAgentType agent;

            switch (agentType)
            {
                case SimAgentTypes.Carnivore:
                    agent = new Carnivore<IVector, ITransform<IVector>>();
                    agent.brainTypes = DataContainer.carnBrainTypes;
                    agent.agentType = SimAgentTypes.Carnivore;
                    break;
                case SimAgentTypes.Herbivore:
                    agent = new Herbivore<IVector, ITransform<IVector>>();
                    agent.brainTypes = DataContainer.herbBrainTypes;
                    agent.agentType = SimAgentTypes.Herbivore;
                    break;
                case SimAgentTypes.Scavenger:
                    agent = new Scavenger<IVector, ITransform<IVector>>();
                    agent.brainTypes = DataContainer.scavBrainTypes;
                    agent.agentType = SimAgentTypes.Scavenger;
                    break;
                default:
                    throw new ArgumentException("Invalid agent type");
            }

            agent.SetPosition(randomNode.GetCoordinate());
            agent.Init();

            if (agentType == SimAgentTypes.Scavenger)
            {
                var sca = (Scavenger<IVector, ITransform<IVector>>)agent;
                sca.boid.Init(DataContainer.flockingManager.Alignment, DataContainer.flockingManager.Cohesion, 
                    DataContainer.flockingManager.Separation, DataContainer.flockingManager.Direction);
            }

            return agent;
        }
        
        private List<NeuralNetComponent> CreateBrain(SimAgentTypes agentType)
        {
            var brains = new List<NeuralNetComponent> { CreateSingleBrain(BrainType.Eat, agentType) };


            switch (agentType)
            {
                case SimAgentTypes.Herbivore:
                    brains.Add(CreateSingleBrain(BrainType.Movement, SimAgentTypes.Herbivore));
                    brains.Add(CreateSingleBrain(BrainType.Escape, SimAgentTypes.Herbivore));
                    break;
                case SimAgentTypes.Carnivore:
                    brains.Add(CreateSingleBrain(BrainType.Movement, SimAgentTypes.Carnivore));
                    brains.Add(CreateSingleBrain(BrainType.Attack, SimAgentTypes.Carnivore));
                    break;
                case SimAgentTypes.Scavenger:
                    brains.Add(CreateSingleBrain(BrainType.ScavengerMovement, SimAgentTypes.Scavenger));
                    brains.Add(CreateSingleBrain(BrainType.Flocking, SimAgentTypes.Scavenger));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(agentType), agentType,
                        "Not prepared for this agent type");
            }

            return brains;
        }

        // SIMULATION
        private NeuralNetComponent CreateSingleBrain(BrainType brainType, SimAgentTypes agentType)
        {
            var neuralNetComponent = new NeuralNetComponent();
            neuralNetComponent.Layers.Add(CreateNeuronLayerList(brainType, agentType));
            return neuralNetComponent;
        }
        
        private List<NeuronLayer> CreateNeuronLayerList(BrainType brainType, SimAgentTypes agentType)
        {
            if (!DataContainer.InputCountCache.TryGetValue((brainType, agentType), out var inputCount))
            {
                throw new ArgumentException("Invalid brainType or agentType");
            }

            var layers = new List<NeuronLayer>
            {
                new(inputCount.inputCount, inputCount.inputCount, DataContainer.Bias, DataContainer.SigmoidP)
                    { BrainType = brainType, AgentType = agentType }
            };

            foreach (int hiddenLayerInput in inputCount.hiddenLayersInputs)
            {
                layers.Add(new NeuronLayer(layers[^1].OutputsCount, hiddenLayerInput, 
                        DataContainer.Bias, DataContainer.SigmoidP) { BrainType = brainType, AgentType = agentType });
            }

            layers.Add(new NeuronLayer(layers[^1].OutputsCount, inputCount.outputCount, DataContainer.Bias, DataContainer.SigmoidP)
                { BrainType = brainType, AgentType = agentType });

            return layers;
        }
        
        public void StartSimulation(int eliteCount, float mutationChance, float mutationRate, int herbCount, int carnCount, int scavCount)
        {
            DataContainer._agents = new Dictionary<uint, SimAgentType>();
            DataContainer._population = new Dictionary<uint, Dictionary<BrainType, List<Genome>>>();
            DataContainer.genAlg = new GeneticAlgorithm(eliteCount, mutationChance, mutationRate);
            GenerateInitialPopulation(herbCount, carnCount, scavCount);
            DataContainer.isRunning = true;
        }

        public void StopSimulation(Action DestroyAgents, ref int Generation)
        {
            DataContainer.isRunning = false;
            Generation = 0;
            DestroyAgents();
        }

        public void PauseSimulation()
        {
            DataContainer.isRunning = !DataContainer.isRunning;
        }
    }
}