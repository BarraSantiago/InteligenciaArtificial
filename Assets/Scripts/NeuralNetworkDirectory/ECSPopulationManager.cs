using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeuralNetworkLib.Agents.Flocking;
using NeuralNetworkLib.Agents.SimAgents;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.NeuralNetDirectory;
using NeuralNetworkLib.NeuralNetDirectory.ECS;
using NeuralNetworkLib.NeuralNetDirectory.ECS.Patron;
using NeuralNetworkLib.NeuralNetDirectory.NeuralNet;
using NeuralNetworkLib.Utils;
using Pathfinder.Graph;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NeuralNetworkDirectory.ECS
{
    using SimAgentType = SimAgent<IVector, ITransform<IVector>>;
    using SimBoid = Boid<IVector, ITransform<IVector>>;

    public class EcsPopulationManager : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Mesh carnivoreMesh;
        [SerializeField] private Material carnivoreMat;
        [SerializeField] private Mesh herbivoreMesh;
        [SerializeField] private Material herbivoreMat;
        [SerializeField] private Mesh scavengerMesh;
        [SerializeField] private Material scavengerMat;


        [SerializeField] private int carnivoreCount = 10;
        [SerializeField] private int herbivoreCount = 20;
        [SerializeField] private int scavengerCount = 10;

        [SerializeField] private int eliteCount = 4;
        [SerializeField] private float generationDuration = 20.0f;
        [SerializeField] private float mutationChance = 0.10f;
        [SerializeField] private float mutationRate = 0.01f;
        [SerializeField] public int Generation;

        public int gridWidth = 10;
        public int gridHeight = 10;
        public float speed = 1.0f;
        public static Sim2Graph graph;

        private int missingCarnivores;
        private int missingHerbivores;
        private int missingScavengers;
        private const float Bias = 0.0f;
        private const float SigmoidP = .5f;
        private bool isRunning = true;
        private int plantCount;
        private int currentTurn;
        private int behaviourCount;
        private const int CellSize = 1;
        private float accumTime;
        private GeneticAlgorithm genAlg;
        private GraphManager<IVector, ITransform<IVector>> gridManager;
        private FitnessManager<IVector, ITransform<IVector>> fitnessManager;
        private static Dictionary<uint, Dictionary<BrainType, List<Genome>>> _population = new();
        private static readonly int BrainsAmount = Enum.GetValues(typeof(BrainType)).Length;

        private ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 32
        };

        #endregion

        private void Awake()
        {
            Herbivore<IVector, ITransform<IVector>>.OnDeath += RemoveEntity;
            DataContainer.herbBrainTypes = new Dictionary<int, BrainType>();
            DataContainer.scavBrainTypes = new Dictionary<int, BrainType>();
            DataContainer.carnBrainTypes = new Dictionary<int, BrainType>();
            DataContainer.herbBrainTypes[0] = BrainType.Eat;
            DataContainer.herbBrainTypes[1] = BrainType.Movement;
            DataContainer.herbBrainTypes[2] = BrainType.Escape;

            DataContainer.scavBrainTypes[0] = BrainType.Eat;
            DataContainer.scavBrainTypes[1] = BrainType.ScavengerMovement;
            DataContainer.scavBrainTypes[2] = BrainType.Flocking;

            DataContainer.carnBrainTypes[0] = BrainType.Eat;
            DataContainer.carnBrainTypes[1] = BrainType.Movement;
            DataContainer.carnBrainTypes[2] = BrainType.Attack;

            DataContainer.inputCounts = new[]
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

            DataContainer.InputCountCache =
                DataContainer.inputCounts.ToDictionary(input => (input.brainType, input.agentType));
            DataContainer.inputCounts = DataContainer.inputCounts;
            ECSManager.Init();
            gridManager = new GraphManager<IVector, ITransform<IVector>>(gridWidth, gridHeight);
            graph = new Sim2Graph(gridWidth, gridHeight, CellSize);
            DataContainer.graph = graph;
            StartSimulation();
            plantCount = DataContainer.Agents.Values.Count(agent => agent.agentType == SimAgentTypes.Herbivore) * 2;
            InitializePlants();
            fitnessManager = new FitnessManager<IVector, ITransform<IVector>>(DataContainer.Agents);
            behaviourCount = GetHighestBehaviourCount();
        }

        private void Update()
        {
            Matrix4x4[] carnivoreMatrices = new Matrix4x4[carnivoreCount];
            Matrix4x4[] herbivoreMatrices = new Matrix4x4[herbivoreCount];
            Matrix4x4[] scavengerMatrices = new Matrix4x4[scavengerCount];

            int carnivoreIndex = 0;
            int herbivoreIndex = 0;
            int scavengerIndex = 0;

            Parallel.ForEach(DataContainer.Agents.Keys, id =>
            {
                IVector pos = DataContainer.Agents[id].Transform.position;
                Vector3 position = new Vector3(pos.X, pos.Y);
                Matrix4x4 matrix = Matrix4x4.Translate(position);

                switch (DataContainer.Agents[id].agentType)
                {
                    case SimAgentTypes.Carnivore:
                        int carnIndex = Interlocked.Increment(ref carnivoreIndex) - 1;
                        if (carnIndex < carnivoreMatrices.Length)
                        {
                            carnivoreMatrices[carnIndex] = matrix;
                        }

                        break;
                    case SimAgentTypes.Herbivore:
                        int herbIndex = Interlocked.Increment(ref herbivoreIndex) - 1;
                        if (herbIndex < herbivoreMatrices.Length)
                        {
                            herbivoreMatrices[herbIndex] = matrix;
                        }

                        break;
                    case SimAgentTypes.Scavenger:
                        int scavIndex = Interlocked.Increment(ref scavengerIndex) - 1;
                        if (scavIndex < scavengerMatrices.Length)
                        {
                            scavengerMatrices[scavIndex] = matrix;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            if (carnivoreMatrices.Length > 0)
            {
                Graphics.DrawMeshInstanced(carnivoreMesh, 0, carnivoreMat, carnivoreMatrices);
            }

            if (herbivoreMatrices.Length > 0)
            {
                Graphics.DrawMeshInstanced(herbivoreMesh, 0, herbivoreMat, herbivoreMatrices);
            }

            if (scavengerMatrices.Length > 0)
            {
                Graphics.DrawMeshInstanced(scavengerMesh, 0, scavengerMat, scavengerMatrices);
            }
        }

        private void FixedUpdate()
        {
            if (!isRunning)
                return;

            float dt = Time.fixedDeltaTime;

            float clampSpeed = Mathf.Clamp(speed / 100.0f * 50, 1, 50);
            for (int i = 0; i < clampSpeed; i++)
            {
                EntitiesTurn(dt);

                accumTime += dt * clampSpeed;
                if (!(accumTime >= generationDuration)) return;
                accumTime -= generationDuration;
                Epoch();
            }
        }

        private void EntitiesTurn(float dt)
        {
            KeyValuePair<uint, SimAgentType>[] agentsCopy = DataContainer.Agents.ToArray();

            Parallel.ForEach(agentsCopy, parallelOptions, entity =>
            {
                entity.Value.UpdateInputs();
                InputComponent inputComponent = ECSManager.GetComponent<InputComponent>(entity.Key);
                if (inputComponent != null && DataContainer.Agents.TryGetValue(entity.Key, out SimAgentType agent))
                {
                    inputComponent.inputs = agent.input;
                }
            });

            ECSManager.Tick(dt);

            Parallel.ForEach(agentsCopy, parallelOptions, entity =>
            {
                OutputComponent outputComponent = ECSManager.GetComponent<OutputComponent>(entity.Key);
                if (outputComponent == null ||
                    !DataContainer.Agents.TryGetValue(entity.Key, out SimAgentType agent)) return;

                agent.output = outputComponent.Outputs;

                if (agent.agentType != SimAgentTypes.Scavenger) return;

                SimBoid boid = DataContainer.Scavengers[entity.Key]?.boid;

                if (boid != null)
                {
                    UpdateBoidOffsets(boid, outputComponent.Outputs
                        [GetBrainTypeKeyByValue(BrainType.Flocking, SimAgentTypes.Scavenger)]);
                }
            });

            int batchSize = 10;
            for (int i = 0; i < behaviourCount; i++)
            {
                int i1 = i;
                List<Task> tasks = new List<Task>();

                for (int j = 0; j < agentsCopy.Length; j += batchSize)
                {
                    KeyValuePair<uint, SimAgentType>[] batch = agentsCopy.Skip(j).Take(batchSize).ToArray();
                    tasks.Add(Task.Run(() =>
                    {
                        foreach (KeyValuePair<uint, SimAgentType> entity in batch)
                        {
                            entity.Value.Fsm.MultiThreadTick(i1);
                        }
                    }));
                }

                foreach (KeyValuePair<uint, SimAgentType> entity in agentsCopy)
                {
                    entity.Value.Fsm.MainThreadTick(i);
                }

                Task.WaitAll(tasks.ToArray());

                foreach (Task task in tasks)
                {
                    task.Dispose();
                }

                tasks.Clear();
            }

            fitnessManager.Tick();
        }

        private void Epoch()
        {
            Generation++;

            PurgingSpecials();

            missingCarnivores =
                carnivoreCount - DataContainer.Agents.Count(agent => agent.Value.agentType == SimAgentTypes.Carnivore);
            missingHerbivores =
                herbivoreCount - DataContainer.Agents.Count(agent => agent.Value.agentType == SimAgentTypes.Herbivore);
            missingScavengers =
                scavengerCount - DataContainer.Agents.Count(agent => agent.Value.agentType == SimAgentTypes.Scavenger);
            bool remainingPopulation = DataContainer.Agents.Count > 0;

            bool remainingCarn = carnivoreCount - missingCarnivores > 1;
            bool remainingHerb = herbivoreCount - missingHerbivores > 1;
            bool remainingScav = scavengerCount - missingScavengers > 1;

            ECSManager.GetSystem<NeuralNetSystem>().Deinitialize();
            if (Generation % 100 == 0) Save("NeuronData", Generation);

            if (remainingPopulation)
            {
                foreach (SimAgentType agent in DataContainer.Agents.Values)
                {
                    Debug.Log(agent.agentType + " survived.");
                }
            }

            CleanMap();
            InitializePlants();

            if (!remainingPopulation)
            {
                FillPopulation();
                _population.Clear();

                return;
            }

            Dictionary<SimAgentTypes, Dictionary<BrainType, List<Genome>>> genomes = new()
            {
                [SimAgentTypes.Scavenger] = new Dictionary<BrainType, List<Genome>>(),
                [SimAgentTypes.Herbivore] = new Dictionary<BrainType, List<Genome>>(),
                [SimAgentTypes.Carnivore] = new Dictionary<BrainType, List<Genome>>()
            };
            Dictionary<SimAgentTypes, Dictionary<BrainType, int>> indexes = new()
            {
                [SimAgentTypes.Scavenger] = new Dictionary<BrainType, int>(),
                [SimAgentTypes.Herbivore] = new Dictionary<BrainType, int>(),
                [SimAgentTypes.Carnivore] = new Dictionary<BrainType, int>()
            };


            foreach (SimAgentType agent in DataContainer.Agents.Values)
            {
                agent.Reset();
            }

            if (remainingCarn)
            {
                CreateNewGenomes(genomes, DataContainer.carnBrainTypes, SimAgentTypes.Carnivore, carnivoreCount);
            }

            if (remainingScav)
            {
                CreateNewGenomes(genomes, DataContainer.scavBrainTypes, SimAgentTypes.Scavenger, scavengerCount);
            }

            if (remainingHerb)
            {
                CreateNewGenomes(genomes, DataContainer.herbBrainTypes, SimAgentTypes.Herbivore, herbivoreCount);
            }

            FillPopulation();
            BrainsHandler(indexes, genomes, remainingCarn, remainingScav, remainingHerb);

            genomes.Clear();
            indexes.Clear();

            if (Generation % 100 != 0) return;

            GC.Collect();
            GC.WaitForPendingFinalizers();
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
            DestroyAgents();

            CreateAgents(herbivoreCount, SimAgentTypes.Herbivore);
            CreateAgents(carnivoreCount, SimAgentTypes.Carnivore);
            CreateAgents(scavengerCount, SimAgentTypes.Scavenger);

            accumTime = 0.0f;
        }

        private void CreateAgents(int count, SimAgentTypes agentType)
        {
            Parallel.For(0, count, i =>
            {
                uint entityID = ECSManager.CreateEntity();
                NeuralNetComponent neuralNetComponent = new NeuralNetComponent();
                InputComponent inputComponent = new InputComponent();
                ECSManager.AddComponent(entityID, inputComponent);
                ECSManager.AddComponent(entityID, neuralNetComponent);

                Dictionary<int, BrainType> num = agentType switch
                {
                    SimAgentTypes.Carnivore => DataContainer.carnBrainTypes,
                    SimAgentTypes.Herbivore => DataContainer.herbBrainTypes,
                    SimAgentTypes.Scavenger => DataContainer.scavBrainTypes,
                    _ => throw new ArgumentException("Invalid agent type")
                };

                OutputComponent outputComponent = new OutputComponent();

                ECSManager.AddComponent(entityID, outputComponent);
                outputComponent.Outputs = new float[3][];

                foreach (BrainType brain in num.Values)
                {
                    NeuronInputCount inputsCount = DataContainer.InputCountCache[(brain, agentType)];
                    outputComponent.Outputs[GetBrainTypeKeyByValue(brain, agentType)] =
                        new float[inputsCount.outputCount];
                }

                List<NeuralNetComponent> brains = CreateBrain(agentType);
                Dictionary<BrainType, List<Genome>> genomes = new Dictionary<BrainType, List<Genome>>();

                foreach (NeuralNetComponent brain in brains)
                {
                    int fromId = 0;
                    BrainType brainType = BrainType.Movement;
                    Genome genome =
                        new Genome(brain.Layers.Sum(layerList =>
                            layerList.Sum(layer => GetWeights(layer).Length)));
                    foreach (List<NeuronLayer> layerList in brain.Layers)
                    {
                        foreach (NeuronLayer layer in layerList)
                        {
                            brainType = layer.BrainType;
                            SetWeights(GetWeights(layer), genome.genome, fromId);
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
                neuralNetComponent.Fitness = new float[BrainsAmount];
                neuralNetComponent.FitnessMod = new float[BrainsAmount];

                for (int j = 0; j < neuralNetComponent.FitnessMod.Length; j++)
                {
                    neuralNetComponent.FitnessMod[j] = 1.0f;
                }

                SimAgentType agent = CreateAgent(agentType);
                lock (DataContainer.Agents)
                {
                    DataContainer.Agents[entityID] = agent;
                }

                if (agentType == SimAgentTypes.Scavenger)
                {
                    lock (DataContainer.Scavengers)
                    {
                        DataContainer.Scavengers[entityID] = (Scavenger<IVector, ITransform<IVector>>)agent;
                    }
                }

                foreach (BrainType brain in agent.brainTypes.Values)
                {
                    lock (_population)
                    {
                        if (!_population.ContainsKey(entityID))
                        {
                            _population[entityID] = new Dictionary<BrainType, List<Genome>>();
                        }

                        _population[entityID][brain] = genomes[brain];
                    }
                }
            });
        }

        private SimAgentType CreateAgent(SimAgentTypes agentType)
        {
            INode<IVector> randomNode = agentType switch
            {
                SimAgentTypes.Carnivore => gridManager.GetRandomPositionInUpperQuarter(),
                SimAgentTypes.Herbivore => gridManager.GetRandomPositionInLowerQuarter(),
                SimAgentTypes.Scavenger => gridManager.GetRandomPosition(),
                _ => throw new ArgumentOutOfRangeException(nameof(agentType), agentType, null)
            };

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
                Scavenger<IVector, ITransform<IVector>> sca = (Scavenger<IVector, ITransform<IVector>>)agent;
                sca.boid.Init(DataContainer.flockingManager.Alignment, DataContainer.flockingManager.Cohesion,
                    DataContainer.flockingManager.Separation,
                    DataContainer.flockingManager.Direction);
            }

            return agent;
        }


        private List<NeuralNetComponent> CreateBrain(SimAgentTypes agentType)
        {
            List<NeuralNetComponent> brains = new List<NeuralNetComponent>
                { CreateSingleBrain(BrainType.Eat, agentType) };


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

        // TODO - Refactor this method
        private NeuralNetComponent CreateSingleBrain(BrainType brainType, SimAgentTypes agentType)
        {
            NeuralNetComponent neuralNetComponent = new NeuralNetComponent();
            neuralNetComponent.Layers.Add(CreateNeuronLayerList(brainType, agentType));
            return neuralNetComponent;
        }


        private List<NeuronLayer> CreateNeuronLayerList(BrainType brainType, SimAgentTypes agentType)
        {
            if (!DataContainer.InputCountCache.TryGetValue((brainType, agentType), out NeuronInputCount inputCount))
            {
                throw new ArgumentException("Invalid brainType or agentType");
            }

            List<NeuronLayer> layers = new List<NeuronLayer>
            {
                new(inputCount.inputCount, inputCount.inputCount, Bias, SigmoidP)
                    { BrainType = brainType, AgentType = agentType }
            };

            foreach (int hiddenLayerInput in inputCount.hiddenLayersInputs)
            {
                layers.Add(new NeuronLayer(layers[^1].OutputsCount, hiddenLayerInput, Bias, SigmoidP)
                    { BrainType = brainType, AgentType = agentType });
            }

            layers.Add(new NeuronLayer(layers[^1].OutputsCount, inputCount.outputCount, Bias, SigmoidP)
                { BrainType = brainType, AgentType = agentType });

            return layers;
        }

        private void DestroyAgents()
        {
            _population.Clear();
        }


        private void BrainsHandler(Dictionary<SimAgentTypes, Dictionary<BrainType, int>> indexes,
            Dictionary<SimAgentTypes, Dictionary<BrainType, List<Genome>>> genomes,
            bool remainingCarn, bool remainingScav, bool remainingHerb)
        {
            foreach (KeyValuePair<uint, SimAgentType> agent in DataContainer.Agents)
            {
                SimAgentTypes agentType = agent.Value.agentType;

                switch (agentType)
                {
                    case SimAgentTypes.Carnivore:
                        if (!remainingCarn) continue;
                        break;
                    case SimAgentTypes.Herbivore:
                        if (!remainingHerb) continue;
                        break;
                    case SimAgentTypes.Scavenger:
                        if (!remainingScav) continue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                NeuralNetComponent neuralNetComponent = ECSManager.GetComponent<NeuralNetComponent>(agent.Key);

                foreach (BrainType brain in agent.Value.brainTypes.Values)
                {
                    agent.Value.GetBrainTypeKeyByValue(brain);
                    if (!indexes[agentType].ContainsKey(brain))
                    {
                        indexes[agentType][brain] = 0;
                    }

                    int index = Random.Range(0, genomes[agentType][brain].Count);
                    if (!_population.ContainsKey(agent.Key))
                    {
                        _population[agent.Key] = new Dictionary<BrainType, List<Genome>>();
                    }

                    if (!_population[agent.Key].ContainsKey(brain))
                    {
                        _population[agent.Key][brain] = new List<Genome>();
                    }

                    if (index >= genomes[agentType][brain].Count) continue;

                    int fromId = 0;

                    if (index < neuralNetComponent.Layers.Count)
                    {
                        for (int i = 0; i < neuralNetComponent.Layers[index].Count; i++)
                        {
                            fromId = SetWeights(GetWeights(neuralNetComponent.Layers[index][i]),
                                genomes[agentType][brain][index].genome, fromId);
                        }
                    }

                    _population[agent.Key][brain].Add(genomes[agentType][brain][index]);
                    genomes[agentType][brain].Remove(genomes[agentType][brain][index]);

                    agent.Value.Transform = new ITransform<IVector>(new MyVector(
                        gridManager.GetRandomPosition().GetCoordinate().X,
                        gridManager.GetRandomPosition().GetCoordinate().Y));
                    agent.Value.Reset();
                }
            }
        }


        public static int SetWeights(float[] weights, float[] newWeights, int fromId)
        {
            if (newWeights == null || fromId < 0 || fromId + weights.Length > newWeights.Length)
            {
                return newWeights.Length;
            }

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = newWeights[i + fromId];
            }

            return fromId + weights.Length;
        }

        private void FillPopulation()
        {
            CreateAgents(missingHerbivores, SimAgentTypes.Herbivore);
            CreateAgents(missingCarnivores, SimAgentTypes.Carnivore);
            CreateAgents(missingScavengers, SimAgentTypes.Scavenger);
        }

        private void CreateNewGenomes(Dictionary<SimAgentTypes, Dictionary<BrainType, List<Genome>>> genomes,
            Dictionary<int, BrainType> brainTypes, SimAgentTypes agentType, int count)
        {
            foreach (BrainType brain in brainTypes.Values)
            {
                genomes[agentType][brain] =
                    genAlg.Epoch(GetGenomesByBrainAndAgentType(agentType, brain).ToArray(), count);
            }
        }

        private List<Genome> GetGenomesByBrainAndAgentType(SimAgentTypes agentType, BrainType brainType)
        {
            List<Genome> genomes = new List<Genome>();

            foreach (KeyValuePair<uint, SimAgentType> agentEntry in DataContainer.Agents)
            {
                uint agentId = agentEntry.Key;
                SimAgentType agent = agentEntry.Value;

                if (agent.agentType != agentType)
                {
                    continue;
                }

                NeuralNetComponent neuralNetComponent = ECSManager.GetComponent<NeuralNetComponent>(agentId);

                List<float> weights = new List<float>();
                foreach (List<NeuronLayer> layerList in neuralNetComponent.Layers)
                {
                    foreach (NeuronLayer layer in layerList)
                    {
                        if (layer.BrainType != brainType) continue;


                        weights.AddRange(GetWeights(layer));
                    }
                }

                Genome genome = new Genome(weights.ToArray());
                genomes.Add(genome);
            }

            return genomes;
        }

        private void InitializePlants()
        {
            for (int i = 0; i < plantCount; i++)
            {
                INode<IVector> plantPosition = gridManager.GetRandomPosition();
                plantPosition.NodeType = SimNodeType.Bush;
                plantPosition.Food = 5;
            }
        }

        private void CleanMap()
        {
            foreach (SimNode<IVector> node in DataContainer.graph.NodesType)
            {
                node.Food = 0;
                node.NodeType = SimNodeType.Empty;
            }
        }

        private void Save(string directoryPath, int generation)
        {
            /*
            var agentsData = new List<AgentNeuronData>();

            var entitiesCopy = DataContainer.Agents.ToList();

            Parallel.ForEach(entitiesCopy, parallelOptions, entity =>
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
            });*/
        }

        public void Load(string directoryPath)
        {
            /*
            Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> loadedData =
                NeuronDataSystem.LoadLatestNeurons(directoryPath);

            Parallel.ForEach(DataContainer.Agents, parallelOptions, entity =>
            {
                NeuralNetComponent netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                SimAgentType agent = DataContainer.Agents[entity.Key];

                if (!loadedData.TryGetValue(agent.agentType,
                        out Dictionary<BrainType, List<AgentNeuronData>> brainData)) return;

                Parallel.ForEach(agent.brainTypes, parallelOptions, brainType =>
                {
                    if (!brainData.TryGetValue(brainType.Value, out List<AgentNeuronData> neuronDataList)) return;

                    for (int i = 0; i < neuronDataList.Count; i++)
                    {
                        AgentNeuronData neuronData = neuronDataList[i];
                        foreach (List<NeuronLayer> neuronLayer in netComponent.Layers)
                        {
                            int fromId = 0;
                            foreach (NeuronLayer layer in neuronLayer)
                            {
                                lock (layer)
                                {
                                    layer.AgentType = neuronData.AgentType;
                                    layer.BrainType = neuronData.BrainType;
                                    layer.Bias = neuronData.Bias;


                                    fromId = SetWeights(GetWeights(layer), neuronData.NeuronWeights, fromId);
                                }
                            }
                        }

                        lock (netComponent.Fitness)
                        {
                            netComponent.Fitness[i] = neuronData.Fitness;
                        }
                    }
                });
            });*/
        }

        public static SimAgentType GetNearestEntity(SimAgentTypes entityType, IVector position)
        {
            SimAgentType nearestAgent = null;
            float minDistance = float.MaxValue;

            foreach (SimAgentType agent in DataContainer.Agents.Values)
            {
                if (agent.agentType != entityType) continue;

                float distance = IVector.Distance(position, agent.CurrentNode.GetCoordinate());

                if (minDistance < distance) continue;

                minDistance = distance;
                nearestAgent = agent;
            }

            return nearestAgent;
        }

        public static INode<IVector> CoordinateToNode(IVector coordinate)
        {
            if (coordinate.X < 0 || coordinate.Y < 0 || coordinate.X >= DataContainer.graph.MaxX ||
                coordinate.Y >= DataContainer.graph.MaxY)
            {
                return null;
            }

            return DataContainer.graph.NodesType[(int)coordinate.X, (int)coordinate.Y];
        }

        private void StartSimulation()
        {
            DataContainer.Agents = new Dictionary<uint, SimAgentType>();
            _population = new Dictionary<uint, Dictionary<BrainType, List<Genome>>>();
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

        public static List<ITransform<IVector>> GetBoidsInsideRadius(SimBoid boid)
        {
            List<ITransform<IVector>> insideRadiusBoids = new List<ITransform<IVector>>();
            float detectionRadiusSquared = boid.detectionRadious * boid.detectionRadious;
            IVector boidPosition = boid.transform.position;

            Parallel.ForEach(DataContainer.Scavengers.Values, scavenger =>
            {
                if (scavenger?.Transform.position == null || boid == scavenger.boid)
                {
                    return;
                }

                IVector scavengerPosition = scavenger.Transform.position;
                float distanceSquared = IVector.DistanceSquared(boidPosition, scavengerPosition);

                if (distanceSquared > detectionRadiusSquared) return;
                lock (insideRadiusBoids)
                {
                    insideRadiusBoids.Add(scavenger.boid.transform);
                }
            });

            return insideRadiusBoids;
        }

        public static INode<IVector> GetNearestNode(SimNodeType nodeType, IVector position)
        {
            INode<IVector> nearestNode = null;
            float minDistance = float.MaxValue;

            foreach (SimNode<IVector> node in DataContainer.graph.NodesType)
            {
                if (node.NodeType != nodeType) continue;

                float distance = IVector.Distance(position, node.GetCoordinate());

                if (minDistance < distance) continue;

                minDistance = distance;

                nearestNode = node;
            }

            return nearestNode;
        }

        private int GetHighestBehaviourCount()
        {
            int highestCount = 0;

            foreach (SimAgentType entity in DataContainer.Agents.Values)
            {
                int multiThreadCount = entity.Fsm.GetMultiThreadCount();
                int mainThreadCount = entity.Fsm.GetMainThreadCount();

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
            Dictionary<int, BrainType> brainTypes = agentType switch
            {
                SimAgentTypes.Carnivore => DataContainer.carnBrainTypes,
                SimAgentTypes.Herbivore => DataContainer.herbBrainTypes,
                SimAgentTypes.Scavenger => DataContainer.scavBrainTypes,
                _ => throw new ArgumentException("Invalid agent type")
            };

            foreach (var kvp in brainTypes)
            {
                if (kvp.Value == value)
                {
                    return kvp.Key;
                }
            }

            throw new KeyNotFoundException(
                $"The value '{value}' is not present in the brainTypes dictionary for agent type '{agentType}'.");
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;


            foreach (SimNode<IVector> node in DataContainer.graph.NodesType)
            {
                Gizmos.color = node.NodeType switch
                {
                    SimNodeType.Blocked => Color.black,
                    SimNodeType.Bush => Color.green,
                    SimNodeType.Corpse => Color.red,
                    SimNodeType.Carrion => Color.magenta,
                    SimNodeType.Empty => Color.white,
                    _ => Color.white
                };

                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().X, node.GetCoordinate().Y), (float)CellSize / 5);
                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().X, node.GetCoordinate().Y), (float)CellSize / 5);
            }
        }

        private void PurgingSpecials()
        {
            List<uint> agentsToRemove = new List<uint>();

            foreach (KeyValuePair<uint, SimAgentType> agentEntry in DataContainer.Agents)
            {
                SimAgentType agent = agentEntry.Value;
                if (agent.agentType == SimAgentTypes.Herbivore)
                {
                    if (agent is Herbivore<IVector, ITransform<IVector>> { Hp: <= 0 })
                    {
                        agentsToRemove.Add(agentEntry.Key);
                    }
                }

                if (!agent.CanReproduce)
                {
                    agentsToRemove.Add(agentEntry.Key);
                }
            }

            foreach (uint agentId in agentsToRemove)
            {
                if (DataContainer.Agents.ContainsKey(agentId))
                {
                    RemoveEntity(DataContainer.Agents[agentId]);
                }
            }

            agentsToRemove.Clear();
        }

        public static void RemoveEntity(SimAgentType simAgent)
        {
            simAgent.Uninit();
            uint agentId = DataContainer.Agents.FirstOrDefault(agent => agent.Value == simAgent).Key;
            DataContainer.Agents.Remove(agentId);
            _population.Remove(agentId);
            DataContainer.Scavengers.Remove(agentId);
            ECSManager.RemoveEntity(agentId);
        }

        public static float[] GetWeights(NeuronLayer layer)
        {
            int totalWeights = (int)(layer.NeuronsCount * layer.InputsCount);
            float[] weights = new float[totalWeights];
            int id = 0;

            for (int i = 0; i < layer.NeuronsCount; i++)
            {
                float[] ws = layer.neurons[i].weights;

                for (int j = 0; j < ws.Length; j++)
                {
                    weights[id] = ws[j];
                    id++;
                }
            }

            return weights;
        }
    }
}