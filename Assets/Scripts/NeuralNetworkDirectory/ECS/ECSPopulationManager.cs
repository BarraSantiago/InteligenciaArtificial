using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ECS.Patron;
using FlappyIa.GeneticAlg;
using Flocking;
using NeuralNetworkDirectory.DataManagement;
using NeuralNetworkDirectory.NeuralNet;
using Pathfinder;
using Pathfinder.Graph;
using StateMachine.Agents.Simulation;
using Random = UnityEngine.Random;

namespace NeuralNetworkDirectory.ECS
{
    public class EcsPopulationManager : MonoBehaviour
    {
        [SerializeField] private GameObject carnivorePrefab;
        [SerializeField] private GameObject herbivorePrefab;
        [SerializeField] private GameObject scavengerPrefab;
        [SerializeField] private FlockingManager flockingManager;

        [SerializeField] private int CarnivoreCount = 10;
        [SerializeField] private int HerbivoreCount = 20;
        [SerializeField] private int ScavengerCount = 10;

        [SerializeField] private int EliteCount = 4;
        [SerializeField] private float GenerationDuration = 20.0f;
        [SerializeField] private float MutationChance = 0.10f;
        [SerializeField] private float MutationRate = 0.01f;
        
        public static Graph<SimNode<Vector2>, NodeVoronoi, Vector2> graph;
        public int gridWidth = 10;
        public int gridHeight = 10;
        public int generationTurns = 100;

        private int currentTurn;
        private float accumTime;
        private bool isRunning;
        private Dictionary<uint, GameObject> entities;
        private static Dictionary<uint, SimAgent> agents;
        private Dictionary<uint, List<Genome>> population = new();
        private GraphManager gridManager;
        private GeneticAlgorithm genAlg;
        private readonly List<SimAgent> populationGOs = new();

        public int Generation { get; private set; }
        public float BestFitness { get; private set; }
        public float AvgFitness { get; private set; }
        public float WorstFitness { get; private set; }

        private void Awake()
        {
            ECSManager.AddSystem(new NeuralNetSystem());
            ECSManager.Init();
            entities = new Dictionary<uint, GameObject>();
            gridManager = new GraphManager(gridWidth, gridHeight);
            graph = new Sim2Graph(gridWidth,gridHeight,1);
            StartSimulation();
            InitializePlants();
        }

        private void Update()
        {
            if (currentTurn >= generationTurns) return;
            ECSManager.Tick(Time.deltaTime);
            currentTurn++;
        }

        private void FixedUpdate()
        {
            if (!isRunning)
                return;

            var dt = Time.fixedDeltaTime;

            accumTime += dt;

            if (!(accumTime >= GenerationDuration)) return;
            accumTime -= GenerationDuration;
            Epoch();
        }


        private void LateUpdate()
        {
            Parallel.ForEach(entities, entity =>
            {
                if (agents[entity.Key].agentType != SimAgentTypes.Scavenger) return;
                var outputComponent = ECSManager.GetComponent<OutputComponent>(entity.Key);
                var agent = agents[entity.Key];
                var scavenger = (Scavenger)agent;
                var boid = scavenger.boid;

                if (boid)
                {
                    UpdateBoidOffsets(boid, outputComponent.outputs[(int)BrainType.Flocking]);
                }
            });

            Parallel.ForEach(entities, entity =>
            {
                ECSManager.GetComponent<InputComponent>(entity.Key).inputs = agents[entity.Key].input;

                agents[entity.Key].output = ECSManager.GetComponent<OutputComponent>(entity.Key).outputs;

                agents[entity.Key].Tick(Time.deltaTime);
            });
        }

        private void UpdateBoidOffsets(Boid boid, float[] outputs)
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

            CreateAgents(herbivorePrefab, HerbivoreCount, SimAgentTypes.Herbivore);
            CreateAgents(carnivorePrefab, CarnivoreCount, SimAgentTypes.Carnivorous);
            CreateAgents(scavengerPrefab, ScavengerCount, SimAgentTypes.Scavenger);

            accumTime = 0.0f;
        }

        private void CreateEntities(int count, SimAgentTypes agentType)
        {
            for (var i = 0; i < count; i++)
            {
                var entityID = ECSManager.CreateEntity();
                ECSManager.AddComponent(entityID, new InputComponent());
                ECSManager.AddComponent(entityID, new NeuralNetComponent());
                ECSManager.AddComponent(entityID, new OutputComponent());

                var agent = CreateAgent(agentType);
                agents[entityID] = agent;
                entities[entityID] = agent.gameObject;
            }
        }

        private SimAgent CreateAgent(SimAgentTypes agentType)
        {
            GameObject prefab = agentType switch
            {
                SimAgentTypes.Carnivorous => carnivorePrefab,
                SimAgentTypes.Herbivore => herbivorePrefab,
                SimAgentTypes.Scavenger => scavengerPrefab,
                _ => throw new ArgumentException("Invalid agent type")
            };

            var position = GetRandomPos();
            var go = Instantiate(prefab, position, Quaternion.identity);
            var agent = go.GetComponent<SimAgent>();
            
            if (agentType != SimAgentTypes.Scavenger) return agent;
            
            var sca = (Scavenger)agent;
            sca.boid.Init(flockingManager.Alignment, flockingManager.Cohesion, flockingManager.Separation,
                flockingManager.Direction);
            return agent;
        }

        private void CreateAgents(GameObject prefab, int count, SimAgentTypes agentType)
        {
            for (var i = 0; i < count; i++)
            {
                var entityID = ECSManager.CreateEntity();
                ECSManager.AddComponent(entityID, new InputComponent());
                var neuralNetComponent = new NeuralNetComponent();
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

                var agent = CreateAgent(prefab);
                agents[entityID] = agent;
                entities[entityID] = agent.gameObject;
                population[entityID] = genomes;
                populationGOs.Add(agent);
                agent.Init();
            }
        }

        private List<NeuralNetComponent> CreateBrain(SimAgentTypes agentType)
        {
            var brains = new List<NeuralNetComponent>();

            brains.Add(CreateSingleBrain(BrainType.Eat));

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

        private SimAgent CreateAgent(GameObject prefab)
        {
            var position = gridManager.GetRandomPosition();
            var go = Instantiate(prefab, position.GetCoordinate(), Quaternion.identity);
            var agent = go.GetComponent<SimAgent>();
            agent.CurrentNode = NodeToCoordinate(position);
            
            return agent;
        }

        private void DestroyAgents()
        {
            foreach (var agent in populationGOs)
            {
                Destroy(agent.gameObject);
            }

            populationGOs.Clear();
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
            foreach (var entityID in agents.Keys)
            {
                var agent = agents[entityID];
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
                agent.transform.position = GetRandomPos();
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
            int plantCount = agents.Values.Count(agent => agent.agentType == SimAgentTypes.Herbivore) * 2;
            for (int i = 0; i < plantCount; i++)
            {
                var plantPosition = gridManager.GetRandomPosition();
                plantPosition.NodeType = SimNodeType.Bush;
                plantPosition.food = 5;
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
                var agent = agents[entity.Key];

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

        public static SimAgent GetNearestEntity(SimAgentTypes entityType, NodeVoronoi position)
        {
            SimAgent nearestAgent = null;
            float minDistance = float.MaxValue;

            foreach (var agent in agents.Values)
            {
                if (agent.agentType != entityType) continue;

                float distance = Vector2.Distance(position.GetCoordinate(), agent.CurrentNode.GetCoordinate());

                if (minDistance > distance) continue;

                minDistance = distance;
                nearestAgent = agent;
            }

            return nearestAgent;
        }

        public static SimAgent GetEntity(SimAgentTypes entityType, SimNode<Vector2> position)
        {
            SimAgent target = null;

            foreach (var agent in agents.Values)
            {
                if (agent.agentType != entityType) continue;

                if (!position.GetCoordinate().Equals(agent.CurrentNode.GetCoordinate())) continue;

                target = agent;
                break;
            }

            return target;
        }

        public static SimAgent GetEntity(SimAgentTypes entityType, NodeVoronoi position)
        {
            SimAgent target = null;

            foreach (var agent in agents.Values)
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

        public static SimNode<Vector2> CoordinateToNode(NodeVoronoi coordinate)
        {
            return graph.NodesType
                .FirstOrDefault(node => node.GetCoordinate().Equals(coordinate.GetCoordinate()));
        }
        
        public static NodeVoronoi NodeToCoordinate(SimNode<Vector2> coordinate)
        {
            return graph.CoordNodes[(int)coordinate.GetCoordinate().x, (int)coordinate.GetCoordinate().y];
        }

        public void StartSimulation()
        {
            agents = new Dictionary<uint, SimAgent>();
            entities = new Dictionary<uint, GameObject>();
            population = new Dictionary<uint, List<Genome>>();
            genAlg = new GeneticAlgorithm(EliteCount, MutationChance, MutationRate);
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
    }
}