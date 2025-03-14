using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeuralNetworkDirectory;
using NeuralNetworkLib.Agents.AnimalAgents;
using NeuralNetworkLib.Agents.TCAgent;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.ECS.FlockingECS;
using NeuralNetworkLib.ECS.NeuralNetECS;
using NeuralNetworkLib.ECS.PathfinderECS;
using NeuralNetworkLib.ECS.Patron;
using NeuralNetworkLib.NeuralNetDirectory.NeuralNet;
using NeuralNetworkLib.Utils;
using Pathfinder.Graph;

namespace Simulation
{
    using AnimalAgentType = AnimalAgent<IVector, ITransform<IVector>>;
    using TCAgentType = TcAgent<IVector, ITransform<IVector>>;

    public class AgentFactory
    {
        private readonly GraphManager<IVector, ITransform<IVector>> gridManager;
        private readonly ParallelOptions parallelOptions;
        private readonly float bias;
        private readonly float sigmoidP;

        public AgentFactory(GraphManager<IVector, ITransform<IVector>> gridManager, ParallelOptions parallelOptions,
            float bias = 0.0f, float sigmoidP = 0.5f)
        {
            this.gridManager = gridManager;
            this.parallelOptions = parallelOptions;
            this.bias = bias;
            this.sigmoidP = sigmoidP;
        }

        public void CreateAnimalAgents(int count, AgentTypes agentType)
        {
            Parallel.For((long)0, count, parallelOptions, i =>
            {
                uint entityID = ECSManager.CreateEntity();

                NeuralNetComponent neuralNetComponent = new NeuralNetComponent();
                InputComponent inputComponent = new InputComponent();
                BrainAmountComponent brainAmountComponent = new BrainAmountComponent
                {
                    BrainAmount = agentType switch
                    {
                        AgentTypes.Carnivore => DataContainer.CarnBrainTypes.Count,
                        AgentTypes.Herbivore => DataContainer.HerbBrainTypes.Count,
                        _ => throw new ArgumentException("Invalid agent type")
                    }
                };

                ECSManager.AddComponent(entityID, inputComponent);
                ECSManager.AddComponent(entityID, neuralNetComponent);
                ECSManager.AddComponent(entityID, brainAmountComponent);

                Dictionary<int, BrainType> brainTypes = agentType switch
                {
                    AgentTypes.Carnivore => DataContainer.CarnBrainTypes,
                    AgentTypes.Herbivore => DataContainer.HerbBrainTypes,
                    _ => throw new ArgumentException("Invalid agent type")
                };

                ECSManager.AddFlag(entityID, new ECSFlag(agentType switch
                {
                    AgentTypes.Carnivore => FlagType.Carnivore,
                    AgentTypes.Herbivore => FlagType.Herbivore,
                    _ => FlagType.Herbivore
                }));

                OutputComponent outputComponent = new OutputComponent();
                ECSManager.AddComponent(entityID, outputComponent);
                outputComponent.Outputs = new float[3][];

                foreach (BrainType brain in brainTypes.Values)
                {
                    BrainConfiguration inputsCount = DataContainer.InputCountCache[(brain, agentType)];
                    outputComponent.Outputs[EcsPopulationManager.GetBrainTypeKeyByValue(brain, agentType)] =
                        new float[inputsCount.OutputCount];
                }

                List<NeuralNetComponent> brains = CreateBrain(agentType);
                Dictionary<BrainType, List<Genome>> genomes = new Dictionary<BrainType, List<Genome>>();

                foreach (NeuralNetComponent brain in brains)
                {
                    BrainType brainType = BrainType.Movement;
                    Genome genome = new Genome(brain.Layers.Sum(layerList =>
                        layerList.Sum(layer => EcsPopulationManager.GetWeights(layer).Length)));

                    int j = 0;
                    foreach (NeuronLayer[] layerList in brain.Layers)
                    {
                        brainType = layerList[j++].BrainType;
                        EcsPopulationManager.SetWeights(layerList, genome.genome);
                        foreach (NeuronLayer neuronLayer in layerList)
                        {
                            neuronLayer.AgentType = agentType;
                        }
                    }

                    if (!genomes.ContainsKey(brainType))
                    {
                        genomes[brainType] = new List<Genome>();
                    }

                    genomes[brainType].Add(genome);
                }

                inputComponent.inputs = new float[brains.Count][];
                neuralNetComponent.Layers = brains.SelectMany(brain => brain.Layers).ToArray();
                int brainAmount = agentType switch
                {
                    AgentTypes.Carnivore => DataContainer.CarnBrainTypes.Count,
                    AgentTypes.Herbivore => DataContainer.HerbBrainTypes.Count,
                    _ => throw new ArgumentException("Invalid agent type")
                };
                neuralNetComponent.Fitness = new float[brainAmount];
                neuralNetComponent.FitnessMod = new float[brainAmount];

                for (int j = 0; j < neuralNetComponent.FitnessMod.Length; j++)
                {
                    neuralNetComponent.FitnessMod[j] = 1.0f;
                }

                AnimalAgentType agent = CreateAgent(agentType);
                lock (DataContainer.Animals)
                {
                    DataContainer.Animals[entityID] = agent;
                }
            });
        }

        public void CreateTownCenterAgents(int count, TownCenter townCenter, AgentTypes agentType)
        {
            Parallel.For((long)0, count, parallelOptions, i =>
            {
                uint entityID = ECSManager.CreateEntity();

                BoidConfigComponent boidConfig = new BoidConfigComponent(6, 0.5f, 0.8f, 1.3f, 1.3f);
                ACSComponent acsComponent = new ACSComponent();
                TransformComponent transformComponent = new TransformComponent();
                PathResultComponent<SimNode<IVector>> pathComponent = new PathResultComponent<SimNode<IVector>>();
                PathRequestComponent<SimNode<IVector>> pathRequestComponent =
                    new PathRequestComponent<SimNode<IVector>>()
                    {
                        StartNode = townCenter.Position,
                        DestinationNode = townCenter.Position,
                        IsProcessed = true
                    };

                TCAgentType agent;
                ECSFlag flag = new ECSFlag(FlagType.None);

                switch (agentType)
                {
                    default:
                    case AgentTypes.Gatherer:
                        agent = new Gatherer(entityID);
                        flag.Flag = FlagType.Gatherer;
                        break;
                    case AgentTypes.Cart:
                        agent = new Cart(entityID);
                        flag.Flag = FlagType.Cart;
                        break;
                    case AgentTypes.Builder:
                        agent = new Builder(entityID);
                        flag.Flag = FlagType.Builder;
                        break;
                }

                ECSManager.AddComponent(entityID, acsComponent);
                ECSManager.AddComponent(entityID, boidConfig);
                ECSManager.AddComponent(entityID, transformComponent);
                ECSManager.AddComponent(entityID, pathComponent);
                ECSManager.AddComponent(entityID, pathRequestComponent);
                ECSManager.AddFlag(entityID, flag);

                agent.TownCenter = townCenter;
                agent.CurrentNode = townCenter.Position;
                agent.Init();
                transformComponent.Transform = agent.Transform;

                DataContainer.TcAgents.TryAdd(entityID, agent);
                townCenter.Agents.Add(agent);
            });
        }

        private AnimalAgentType CreateAgent(AgentTypes agentType)
        {
            INode<IVector> randomNode = agentType switch
            {
                AgentTypes.Carnivore => gridManager.GetRandomPositionInUpperQuarter(),
                AgentTypes.Herbivore => gridManager.GetRandomPositionInLowerQuarter(),
                _ => throw new ArgumentOutOfRangeException(nameof(agentType), agentType, null)
            };

            AnimalAgentType agent;

            switch (agentType)
            {
                case AgentTypes.Carnivore:
                    agent = new Carnivore<IVector, ITransform<IVector>>();
                    agent.brainTypes = DataContainer.CarnBrainTypes;
                    agent.agentType = AgentTypes.Carnivore;
                    break;
                case AgentTypes.Herbivore:
                    agent = new Herbivore<IVector, ITransform<IVector>>();
                    agent.brainTypes = DataContainer.HerbBrainTypes;
                    agent.agentType = AgentTypes.Herbivore;
                    break;
                default:
                    throw new ArgumentException("Invalid agent type");
            }

            agent.SetPosition(randomNode.GetCoordinate());
            agent.Init();

            return agent;
        }

        private List<NeuralNetComponent> CreateBrain(AgentTypes agentType)
        {
            List<NeuralNetComponent> brains = new List<NeuralNetComponent>();

            switch (agentType)
            {
                case AgentTypes.Herbivore:
                    brains.Add(CreateSingleBrain(BrainType.Eat, AgentTypes.Herbivore));
                    brains.Add(CreateSingleBrain(BrainType.Movement, AgentTypes.Herbivore));
                    brains.Add(CreateSingleBrain(BrainType.Escape, AgentTypes.Herbivore));
                    break;
                case AgentTypes.Carnivore:
                    brains.Add(CreateSingleBrain(BrainType.Movement, AgentTypes.Carnivore));
                    brains.Add(CreateSingleBrain(BrainType.Attack, AgentTypes.Carnivore));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(agentType), agentType,
                        "Not prepared for this agent type");
            }

            return brains;
        }

        private NeuralNetComponent CreateSingleBrain(BrainType brainType, AgentTypes agentType)
        {
            NeuralNetComponent neuralNetComponent = new NeuralNetComponent();
            List<NeuronLayer[]> layersList = new List<NeuronLayer[]>
                { CreateNeuronLayerList(brainType, agentType).ToArray() };
            neuralNetComponent.Layers = layersList.ToArray();
            return neuralNetComponent;
        }

        private List<NeuronLayer> CreateNeuronLayerList(BrainType brainType, AgentTypes agentType)
        {
            if (!DataContainer.InputCountCache.TryGetValue((brainType, agentType), out BrainConfiguration InputCount))
            {
                throw new ArgumentException("Invalid brainType or agentType");
            }

            List<NeuronLayer> layers = new List<NeuronLayer>
            {
                new(InputCount.InputCount, InputCount.InputCount, bias, sigmoidP)
                    { BrainType = brainType, AgentType = agentType }
            };

            foreach (int hiddenLayerInput in InputCount.HiddenLayers)
            {
                layers.Add(new NeuronLayer(layers[^1].OutputsCount, hiddenLayerInput, bias, sigmoidP)
                    { BrainType = brainType, AgentType = agentType });
            }

            layers.Add(new NeuronLayer(layers[^1].OutputsCount, InputCount.OutputCount, bias, sigmoidP)
                { BrainType = brainType, AgentType = agentType });

            return layers;
        }
    }
}