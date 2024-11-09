using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ECS.Patron;
using FlappyIa.GeneticAlg;
using Flocking;
using NeuralNetworkDirectory.AI;
using NeuralNetworkDirectory.DataManagement;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using Pathfinder.Graph;
using StateMachine.Agents.Simulation;
using Utils;
using Random = UnityEngine.Random;

namespace NeuralNetworkDirectory.ECS
{
    using SimAgentType = SimAgent<IVector, ITransform<IVector>>;
    using SimBoid = Boid<IVector, ITransform<IVector>>;

    public class EcsPopulationManager : MonoBehaviour
    {
        [SerializeField] private GameObject carnivorePrefab;
        [SerializeField] private GameObject herbivorePrefab;
        [SerializeField] private GameObject scavengerPrefab;

        [SerializeField] private int carnivoreCount = 10;
        [SerializeField] private int herbivoreCount = 20;
        [SerializeField] private int scavengerCount = 10;

        [SerializeField] private int eliteCount = 4;
        [SerializeField] private float generationDuration = 20.0f;
        [SerializeField] private float mutationChance = 0.10f;
        [SerializeField] private float mutationRate = 0.01f;

        public static Sim2Graph graph;
        public int gridWidth = 10;
        public int gridHeight = 10;
        public int generationTurns = 100;

        private int currentTurn;
        private float accumTime;
        private bool isRunning;
        private FlockingManager flockingManager = new();
        private Dictionary<uint, GameObject> entities = new();
        private static Dictionary<uint, SimAgentType> _agents = new();
        private static Dictionary<uint, Scavenger<IVector, ITransform<IVector>>> _scavengers = new();
        private static Dictionary<uint, Herbivore<IVector, ITransform<IVector>>> _herbivores = new();
        private static Dictionary<uint, Carnivore<IVector, ITransform<IVector>>> _carnivores = new();
        private Dictionary<uint, List<Genome>> population = new();
        private GraphManager<IVector, ITransform<IVector>> gridManager;
        private GeneticAlgorithm genAlg;
        private FitnessManager<IVector, ITransform<IVector>> fitnessManager;
        private int behaviourCount;

        public int Generation { get; private set; }
        public float BestFitness { get; private set; }
        public float AvgFitness { get; private set; }
        public float WorstFitness { get; private set; }

        private void Awake()
        {
            //ECSManager.AddSystem(new NeuralNetSystem());
            ECSManager.Init();
            entities = new Dictionary<uint, GameObject>();
            gridManager = new GraphManager<IVector, ITransform<IVector>>(gridWidth, gridHeight);
            graph = new Sim2Graph(gridWidth, gridHeight, 1);
            StartSimulation();
            InitializePlants();
            fitnessManager = new FitnessManager<IVector, ITransform<IVector>>(_agents);
            behaviourCount = GetHighestBehaviourCount();
        }

        private void FixedUpdate()
        {
            if (!isRunning)
                return;

            var dt = Time.fixedDeltaTime;

            accumTime += dt;

            EntitiesTurn(dt);

            if (!(accumTime >= generationDuration)) return;
            accumTime -= generationDuration;
            Epoch();
        }

        private void EntitiesTurn(float dt)
        {
            Parallel.ForEach(_agents.Values, entity => { entity.UpdateInputs(); });

            Parallel.ForEach(entities, entity =>
            {
                var inputComponent = ECSManager.GetComponent<InputComponent>(entity.Key);
                if (inputComponent != null && _agents.ContainsKey(entity.Key))
                {
                    inputComponent.inputs = _agents[entity.Key].input;
                }
            });

            ECSManager.Tick(dt);

            Parallel.ForEach(entities, entity =>
            {
                var outputComponent = ECSManager.GetComponent<OutputComponent>(entity.Key);
                if (outputComponent == null || !_agents.ContainsKey(entity.Key)) return;

                _agents[entity.Key].output = outputComponent.outputs;
            });

            Parallel.ForEach(_scavengers, entity =>
            {
                var outputComponent = ECSManager.GetComponent<OutputComponent>(entity.Key);
                var boid = _scavengers[entity.Key]?.boid;

                if (boid != null && outputComponent != null)
                {
                    UpdateBoidOffsets(boid, outputComponent.outputs[(int)BrainType.Flocking]);
                }
            });

            for (int i = 0; i < behaviourCount; i++)
            {
                var tasks = _scavengers.Select(entity => Task.Run(() => entity.Value.Fsm.MultiThreadTick(i)))
                    .ToArray();

                foreach (var entity in _scavengers)
                {
                    entity.Value.Fsm.MainThreadTick(i);
                }

                Task.WaitAll(tasks);
            }

            fitnessManager.Tick();
        }

        private void UpdateBoidOffsets(SimBoid boid, float[] outputs)
        {
            boid.cohesionOffset = outputs[0];
            boid.separationOffset = outputs[1];
            boid.directionOffset = outputs[2];
            boid.alignmentOffset = outputs[3];
        }


        private void GenerateInitialPopulation()
        {
            Generation = 0;
            DestroyAgents();

            CreateAgents(herbivoreCount, SimAgentTypes.Herbivore);
            CreateAgents(carnivoreCount, SimAgentTypes.Carnivorous);
            CreateAgents(scavengerCount, SimAgentTypes.Scavenger);

            accumTime = 0.0f;
        }

        private SimAgentType CreateAgent(SimAgentTypes agentType, out GameObject go)
        {
            GameObject prefab = agentType switch
            {
                SimAgentTypes.Carnivorous => carnivorePrefab,
                SimAgentTypes.Herbivore => herbivorePrefab,
                SimAgentTypes.Scavenger => scavengerPrefab,
                _ => throw new ArgumentException("Invalid agent type")
            };

            var position = GetRandomPos();
            go = Instantiate(prefab, position, Quaternion.identity);
            
            SimAgentType agent;
            
            switch (agentType)
            {
                case SimAgentTypes.Carnivorous:
                    agent = new Carnivore<IVector, ITransform<IVector>>();
                    break;
                case SimAgentTypes.Herbivore:
                    agent = new Herbivore<IVector, ITransform<IVector>>();
                    break;
                case SimAgentTypes.Scavenger:
                    agent = new Scavenger<IVector, ITransform<IVector>>();
                    break;
                default:
                    throw new ArgumentException("Invalid agent type");
            }
            
            agent.Init();
            agent.CurrentNode = gridManager.GetRandomPosition();
            
            if (agentType != SimAgentTypes.Scavenger) return agent;

            var sca = (Scavenger<IVector, ITransform<IVector>>)agent;
            sca.boid.Init(flockingManager.Alignment, flockingManager.Cohesion, flockingManager.Separation,
                flockingManager.Direction);
            return agent;
        }

        private void CreateAgents(int count, SimAgentTypes agentType)
        {
            for (var i = 0; i < count; i++)
            {
                var entityID = ECSManager.CreateEntity();
                var neuralNetComponent = new NeuralNetComponent();
                ECSManager.AddComponent(entityID, new InputComponent());
                ECSManager.AddComponent(entityID, neuralNetComponent);
                ECSManager.AddComponent(entityID, new OutputComponent());

                var brains = CreateBrain(agentType);
                var genomes = new List<Genome>();

                foreach (var brain in brains)
                {
                    var genome =
                        new Genome(brain.Layers.Sum(layerList => layerList.Sum(layer => layer.GetWeights().Length)));
                    foreach (var layerList in brain.Layers)
                    {
                        foreach (var layer in layerList)
                        {
                            layer.SetWeights(genome.genome, 0);
                        }
                    }

                    genomes.Add(genome);
                }

                neuralNetComponent.Layers = brains.SelectMany(brain => brain.Layers).ToList();

                var agent = CreateAgent(agentType, out GameObject go);
                _agents[entityID] = agent;
                entities[entityID] = go;
                population[entityID] = genomes;

                switch (agentType)
                {
                    case SimAgentTypes.Scavenger:
                        _scavengers[entityID] = agent as Scavenger<IVector, ITransform<IVector>>;
                        break;
                    case SimAgentTypes.Carnivorous:
                        _carnivores[entityID] = agent as Carnivore<IVector, ITransform<IVector>>;
                        break;
                    case SimAgentTypes.Herbivore:
                        _herbivores[entityID] = agent as Herbivore<IVector, ITransform<IVector>>;
                        break;
                }
            }
        }

        private List<NeuralNetComponent> CreateBrain(SimAgentTypes agentType)
        {
            var brains = new List<NeuralNetComponent> { CreateSingleBrain(BrainType.Eat) };

            switch (agentType)
            {
                case SimAgentTypes.Herbivore:
                    brains.Add(CreateSingleBrain(BrainType.Escape));
                    brains.Add(CreateSingleBrain(BrainType.Movement));
                    break;
                case SimAgentTypes.Carnivorous:
                    brains.Add(CreateSingleBrain(BrainType.Attack));
                    brains.Add(CreateSingleBrain(BrainType.Movement));
                    break;
                case SimAgentTypes.Scavenger:
                    brains.Add(CreateSingleBrain(BrainType.Flocking));
                    brains.Add(CreateSingleBrain(BrainType.ScavengerMovement));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(agentType), agentType,
                        "Not prepared for this agent type");
            }

            return brains;
        }

        // TODO - Refactor this method
        private NeuralNetComponent CreateSingleBrain(BrainType brainType)
        {
            var neuralNetComponent = new NeuralNetComponent();
            neuralNetComponent.Layers.Add(CreateNeuronLayerList(brainType));
            return neuralNetComponent;
        }


        private List<NeuronLayer> CreateNeuronLayerList(BrainType brainType)
        {
            var layers = new List<NeuronLayer>
            {
                new NeuronLayer(6, 7, 1f, 0.5f) { BrainType = brainType },
                new NeuronLayer(7, 2, 1f, 0.5f) { BrainType = brainType }
            };
            return layers;
        }

        private void DestroyAgents()
        {
            foreach (var entity in entities)
            {
                Destroy(entity.Value);
            }
            
            population.Clear();
        }

        private void Epoch()
        {
            Generation++;
            BestFitness = GetBestFitness();
            AvgFitness = GetAvgFitness();
            WorstFitness = GetWorstFitness();

            var newGenomes = genAlg.Epoch(population.Values.SelectMany(g => g).ToArray());
            population.Clear();

            int genomeIndex = 0;
            foreach (var entityID in _agents.Keys)
            {
                var agent = _agents[entityID];
                var neuralNetComponent = ECSManager.GetComponent<NeuralNetComponent>(entityID);
                var newGenomesForAgent = new List<Genome>();

                foreach (var brainLayers in neuralNetComponent.Layers)
                {
                    var newGenome = newGenomes[genomeIndex++];
                    foreach (var layer in brainLayers)
                    {
                        layer.SetWeights(newGenome.genome, 0);
                    }

                    newGenomesForAgent.Add(newGenome);
                }

                population[entityID] = newGenomesForAgent;
                ECSManager.GetComponent<NeuralNetComponent>(entityID).Layers = neuralNetComponent.Layers;
                var pos = GetRandomPos();
                agent.transform.position = new MyVector { X = pos.x, Y = pos.z };
            }
        }

        private float GetBestFitness()
        {
            float bestFitness = 0;
            foreach (var genomes in population.Values)
            {
                foreach (var genome in genomes)
                {
                    if (genome.fitness > bestFitness)
                    {
                        bestFitness = genome.fitness;
                    }
                }
            }

            return bestFitness;
        }

        private float GetAvgFitness()
        {
            float totalFitness = 0;
            int genomeCount = 0;
            foreach (var genomes in population.Values)
            {
                foreach (var genome in genomes)
                {
                    totalFitness += genome.fitness;
                    genomeCount++;
                }
            }

            return totalFitness / genomeCount;
        }

        private float GetWorstFitness()
        {
            float worstFitness = float.MaxValue;
            foreach (var genomes in population.Values)
            {
                foreach (var genome in genomes)
                {
                    if (genome.fitness < worstFitness)
                    {
                        worstFitness = genome.fitness;
                    }
                }
            }

            return worstFitness;
        }

        private void InitializePlants()
        {
            int plantCount = _agents.Values.Count(agent => agent.agentType == SimAgentTypes.Herbivore) * 2;
            for (int i = 0; i < plantCount; i++)
            {
                var plantPosition = gridManager.GetRandomPosition();
                plantPosition.NodeType = SimNodeType.Bush;
                plantPosition.Food = 5;
            }
        }


        public void Save(string directoryPath, int generation)
        {
            var agentsData = new List<AgentNeuronData>();

            Parallel.ForEach(entities, entity =>
            {
                var netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                for (int i = 0; i < netComponent.Layers.Count; i++)
                {
                    for (int j = 0; j < netComponent.Layers[i].Count; j++)
                    {
                        var layer = netComponent.Layers[i][j];
                        var neuronData = new AgentNeuronData
                        {
                            AgentType = layer.AgentType,
                            BrainType = layer.BrainType,
                            TotalWeights = layer.GetWeights().Length,
                            Bias = layer.Bias,
                            NeuronWeights = layer.GetWeights(),
                            Fitness = netComponent.Fitness[i]
                        };
                        agentsData.Add(neuronData);
                    }
                }

                NeuronDataSystem.SaveNeurons(agentsData, directoryPath, generation);
            });
        }

        public void Load(string directoryPath)
        {
            var loadedData = NeuronDataSystem.LoadLatestNeurons(directoryPath);

            Parallel.ForEach(entities, entity =>
            {
                var netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                var agent = _agents[entity.Key];

                if (!loadedData.TryGetValue(agent.agentType, out var brainData)) return;

                Parallel.ForEach(agent.brainTypes, brainType =>
                {
                    if (!brainData.TryGetValue(brainType, out var neuronDataList)) return;

                    for (var i = 0; i < neuronDataList.Count; i++)
                    {
                        var neuronData = neuronDataList[i];
                        foreach (var neuronLayer in netComponent.Layers)
                        {
                            foreach (var layer in neuronLayer)
                            {
                                lock (layer)
                                {
                                    layer.AgentType = neuronData.AgentType;
                                    layer.BrainType = neuronData.BrainType;
                                    layer.Bias = neuronData.Bias;
                                    layer.SetWeights(neuronData.NeuronWeights, 0);
                                }
                            }
                        }

                        lock (netComponent.Fitness)
                        {
                            netComponent.Fitness[i] = neuronData.Fitness;
                        }
                    }
                });
            });
        }

        public static SimAgentType GetNearestEntity(SimAgentTypes entityType, INode<IVector> position)
        {
            SimAgentType nearestAgent = null;
            float minDistance = float.MaxValue;

            foreach (var agent in _agents.Values)
            {
                if (agent.agentType != entityType) continue;

                float distance = IVector.Distance(position.GetCoordinate(), agent.CurrentNode.GetCoordinate());

                if (minDistance > distance) continue;

                minDistance = distance;
                nearestAgent = agent;
            }

            return nearestAgent;
        }

        public static SimAgentType GetEntity(SimAgentTypes entityType, INode<IVector> position)
        {
            SimAgentType target = null;

            foreach (var agent in _agents.Values)
            {
                if (agent.agentType != entityType) continue;

                if (!position.GetCoordinate().Equals(agent.CurrentNode.GetCoordinate())) continue;

                target = agent;
                break;
            }

            return target;
        }

        public static SimAgentType GetEntity(SimAgentTypes entityType, ICoordinate<IVector> position)
        {
            SimAgentType target = null;

            foreach (var agent in _agents.Values)
            {
                if (agent.agentType != entityType) continue;

                if (!position.GetCoordinate().Equals(agent.CurrentNode.GetCoordinate())) continue;

                target = agent;
                break;
            }

            return target;
        }

        private Vector3 GetRandomPos()
        {
            return new Vector3(Random.value * 40.0f - 20.0f, 0.0f, Random.value * 40.0f - 20.0f);
        }

        public static INode<IVector> CoordinateToNode(ICoordinate<IVector> coordinate)
        {
            return graph.NodesType.Cast<INode<IVector>>()
                .FirstOrDefault(node => node.GetCoordinate().Equals(coordinate.GetCoordinate()));
        }
        
        public static INode<IVector> CoordinateToNode(IVector coordinate)
        {
            return graph.NodesType.Cast<INode<IVector>>()
                .FirstOrDefault(node => node.GetCoordinate().Equals(coordinate));
        }

        public void StartSimulation()
        {
            _agents = new Dictionary<uint, SimAgentType>();
            entities = new Dictionary<uint, GameObject>();
            population = new Dictionary<uint, List<Genome>>();
            genAlg = new GeneticAlgorithm(eliteCount, mutationChance, mutationRate);
            GenerateInitialPopulation();
            isRunning = true;
        }

        public void StopSimulation()
        {
            isRunning = false;
            Generation = 0;
            DestroyAgents();
        }

        public void PauseSimulation()
        {
            isRunning = !isRunning;
        }

        public static List<SimBoid> GetBoidsInsideRadius(SimBoid boid)
        {
            List<SimBoid> insideRadiusBoids = new List<SimBoid>();

            foreach (Scavenger<IVector, ITransform<IVector>> b in _scavengers.Values)
            {
                if (IVector.Distance(boid.transform.position, b.CurrentNode.GetCoordinate()) < boid.detectionRadious)
                {
                    insideRadiusBoids.Add(b.boid);
                }
            }

            return insideRadiusBoids;
        }

        public static INode<IVector> GetNearestNode(SimNodeType carrion, INode<IVector> currentNode)
        {
            INode<IVector> nearestNode = null;
            float minDistance = float.MaxValue;

            foreach (var node in graph.NodesType)
            {
                if (node.NodeType != carrion) continue;

                float distance = IVector.Distance(currentNode.GetCoordinate(), node.GetCoordinate());

                if (minDistance > distance) continue;

                minDistance = distance;

                nearestNode = node;
            }

            return nearestNode;
        }

        public static IVector Vec2ToIVector(Vector2 vec)
        {
            return new MyVector(vec.x, vec.y);
        }

        public static SimNode<IVector> Vec2ToIVector(INode<IVector> node)
        {
            return new SimNode<IVector>(node.GetCoordinate());
        }

        private int GetHighestBehaviourCount()
        {
            int highestCount = 0;

            foreach (var entity in _scavengers.Values)
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
    }
}