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

namespace NeuralNetworkDirectory
{
    using SimAgentType = SimAgent<IVector, ITransform<IVector>>;
    using SimBoid = Boid<IVector, ITransform<IVector>>;

    public class EcsPopulationManager : MonoBehaviour
    {
        #region Variables

        [Header("Population Setup")] 
        [SerializeField] private Mesh carnivoreMesh;
        [SerializeField] private Material carnivoreMat;
        [SerializeField] private Mesh herbivoreMesh;
        [SerializeField] private Material herbivoreMat;
        [SerializeField] private Mesh scavengerMesh;
        [SerializeField] private Material scavengerMat;

        [Header("Population Settings")] 
        [SerializeField] private int carnivoreCount = 10;
        [SerializeField] private int herbivoreCount = 20;
        [SerializeField] private int scavengerCount = 10;
        [SerializeField] private float mutationRate = 0.01f;
        [SerializeField] private float mutationChance = 0.10f;
        [SerializeField] private int eliteCount = 4;

        [Header("Modifiable Settings")]
        [SerializeField] public int Generation;
        [SerializeField] private float Bias = 0.0f;
        [SerializeField] private int generationsPerSave = 25;
        [SerializeField] private float generationDuration = 20.0f;
        [SerializeField] private bool activateSave;
        [SerializeField] private bool activateLoad;
        [SerializeField] private int generationToLoad = 0;
        [SerializeField] [Range(1, 1500)] private float speed = 1.0f;

        public int gridWidth = 10;
        public int gridHeight = 10;
        private bool isRunning = true;
        private int missingCarnivores;
        private int missingHerbivores;
        private int missingScavengers;
        private int plantCount;
        private int behaviourCount;
        private const int CellSize = 1;
        private const float SigmoidP = .5f;
        private float accumTime;
        private const string DirectoryPath = "NeuronData";
        private GeneticAlgorithm genAlg;
        private GraphManager<IVector, ITransform<IVector>> gridManager;
        private FitnessManager<IVector, ITransform<IVector>> fitnessManager;
        private static readonly int BrainsAmount = Enum.GetValues(typeof(BrainType)).Length;

        private ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 32
        };

        #endregion

        private void Awake()
        {
            DataContainer.Init();
            NeuronDataSystem.OnSpecificLoaded += SpecificLoaded;
            Herbivore<IVector, ITransform<IVector>>.OnDeath += RemoveEntity;
            ECSManager.Init();
            gridManager = new GraphManager<IVector, ITransform<IVector>>(gridWidth, gridHeight);
            DataContainer.graph = new Sim2Graph(gridWidth, gridHeight, CellSize);
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

            for (int i = 0; i < speed; i++)
            {
                EntitiesTurn(dt);
                accumTime += dt;
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

            missingCarnivores = carnivoreCount -
                                DataContainer.Agents.Count(agent => agent.Value.agentType == SimAgentTypes.Carnivore);
            missingHerbivores = herbivoreCount -
                                DataContainer.Agents.Count(agent => agent.Value.agentType == SimAgentTypes.Herbivore);
            missingScavengers = scavengerCount -
                                DataContainer.Agents.Count(agent => agent.Value.agentType == SimAgentTypes.Scavenger);
            bool remainingPopulation = DataContainer.Agents.Count > 0;

            bool remainingCarn = carnivoreCount - missingCarnivores > 1;
            bool remainingHerb = herbivoreCount - missingHerbivores > 1;
            bool remainingScav = scavengerCount - missingScavengers > 1;

            ECSManager.GetSystem<NeuralNetSystem>().Deinitialize();
            if (Generation % generationsPerSave == 0)
            {
                Save(DirectoryPath, Generation);
            }

            if (remainingPopulation)
            {
                foreach (SimAgentType agent in DataContainer.Agents.Values)
                {
                    Debug.Log(agent.agentType + " survived.");
                }
            }

            CleanMap();
            InitializePlants();

            if (missingCarnivores == carnivoreCount) Load(SimAgentTypes.Carnivore);
            if (missingHerbivores == herbivoreCount) Load(SimAgentTypes.Herbivore);
            if (missingScavengers == scavengerCount) Load(SimAgentTypes.Scavenger);

            if (!remainingPopulation)
            {
                FillPopulation();
                return;
            }

            var genomes = new Dictionary<SimAgentTypes, Dictionary<BrainType, List<Genome>>>
            {
                [SimAgentTypes.Scavenger] = new(),
                [SimAgentTypes.Herbivore] = new(),
                [SimAgentTypes.Carnivore] = new()
            };
            var indexes = new Dictionary<SimAgentTypes, Dictionary<BrainType, int>>
            {
                [SimAgentTypes.Scavenger] = new(),
                [SimAgentTypes.Herbivore] = new(),
                [SimAgentTypes.Carnivore] = new()
            };

            foreach (SimAgentType agent in DataContainer.Agents.Values) agent.Reset();

            if (remainingCarn)
                CreateNewGenomes(genomes, DataContainer.carnBrainTypes, SimAgentTypes.Carnivore, carnivoreCount);
            if (remainingScav)
                CreateNewGenomes(genomes, DataContainer.scavBrainTypes, SimAgentTypes.Scavenger, scavengerCount);
            if (remainingHerb)
                CreateNewGenomes(genomes, DataContainer.herbBrainTypes, SimAgentTypes.Herbivore, herbivoreCount);

            FillPopulation();
            BrainsHandler(indexes, genomes, remainingCarn, remainingScav, remainingHerb);

            genomes.Clear();
            indexes.Clear();

            if (Generation % 100 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
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
                    BrainType brainType = BrainType.Movement;
                    Genome genome =
                        new Genome(brain.Layers.Sum(layerList =>
                            layerList.Sum(layer => GetWeights(layer).Length)));
                    int j = 0;
                    foreach (List<NeuronLayer> layerList in brain.Layers)
                    {
                        brainType = layerList[j++].BrainType;
                        SetWeights(layerList, genome.genome);
                        layerList.ForEach(neuron => neuron.AgentType = agentType);
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
                    DataContainer.flockingManager.Separation, DataContainer.flockingManager.Direction);
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

                    if (index >= genomes[agentType][brain].Count) continue;


                    SetWeights(neuralNetComponent.Layers[GetBrainTypeKeyByValue(brain, agent.Value.agentType)],
                        genomes[agentType][brain][index].genome);

                    genomes[agentType][brain].Remove(genomes[agentType][brain][index]);

                    agent.Value.Transform = new ITransform<IVector>(new MyVector(
                        gridManager.GetRandomPosition().GetCoordinate().X,
                        gridManager.GetRandomPosition().GetCoordinate().Y));
                    agent.Value.Reset();
                }
            }
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
            if (!activateSave) return;

            List<AgentNeuronData> agentsData = new List<AgentNeuronData>();

            if (DataContainer.Agents.Count == 0) return;

            List<KeyValuePair<uint, SimAgentType>> entitiesCopy = DataContainer.Agents.ToList();

            agentsData.Capacity = entitiesCopy.Count * DataContainer.InputCountCache.Count;

            //Parallel.ForEach(entitiesCopy, parallelOptions, entity =>
            foreach (KeyValuePair<uint, SimAgentType> entity in entitiesCopy)
            {
                NeuralNetComponent netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                foreach (List<NeuronLayer> neuronLayers in netComponent.Layers)
                {
                    List<float> weights = new List<float>();
                    AgentNeuronData neuronData = new AgentNeuronData();
                    foreach (NeuronLayer layer in neuronLayers)
                    {
                        neuronData.AgentType = layer.AgentType;
                        neuronData.BrainType = layer.BrainType;
                        weights.AddRange(GetWeights(layer));
                    }

                    neuronData.NeuronWeights = weights.ToArray();
                    lock (agentsData)
                    {
                        agentsData.Add(neuronData);
                    }
                }
            }

            NeuronDataSystem.SaveNeurons(agentsData, directoryPath, generation);
        }

        public void Load(SimAgentTypes agentType)
        {
            if (!activateLoad) return;

            Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> loadedData =
                NeuronDataSystem.LoadLatestNeurons(DirectoryPath);

            if (loadedData.Count == 0 || !loadedData.ContainsKey(agentType)) return;
            System.Random random = new System.Random();

            foreach (KeyValuePair<uint, SimAgentType> entity in DataContainer.Agents)
            {
                NeuralNetComponent netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                if (netComponent == null || entity.Value.agentType != agentType) continue;

                if (!loadedData.TryGetValue(agentType, out Dictionary<BrainType, List<AgentNeuronData>> brainData))
                    return;

                foreach (KeyValuePair<int, BrainType> brainType in entity.Value.brainTypes)
                {
                    if (!brainData.TryGetValue(brainType.Value, out List<AgentNeuronData> neuronDataList)) continue;
                    if (neuronDataList.Count == 0) continue;

                    int index = random.Next(0, neuronDataList.Count);
                    AgentNeuronData neuronData = neuronDataList[index];
                    foreach (List<NeuronLayer> neuronLayer in netComponent.Layers)
                    {
                        lock (neuronLayer)
                        {
                            SetWeights(neuronLayer, neuronData.NeuronWeights);
                            neuronLayer.ForEach(neuron => neuron.AgentType = neuronData.AgentType);
                            neuronLayer.ForEach(neuron => neuron.BrainType = neuronData.BrainType);
                        }
                    }

                    lock (loadedData)
                    {
                        loadedData[agentType][brainType.Value].Remove(neuronData);
                    }
                }
            }
        }

        public void Load(string directoryPath)
        {
            if (!activateLoad) return;
            Dictionary<SimAgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> loadedData =
                generationToLoad > 0
                    ? NeuronDataSystem.LoadSpecificNeurons(directoryPath, generationToLoad)
                    : NeuronDataSystem.LoadLatestNeurons(directoryPath);

            if (loadedData.Count == 0) return;
            System.Random random = new System.Random();

            foreach (KeyValuePair<uint, SimAgentType> entity in DataContainer.Agents)
            {
                NeuralNetComponent netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                if (netComponent == null || !DataContainer.Agents.TryGetValue(entity.Key, out SimAgentType agent))
                {
                    return;
                }

                if (!loadedData.TryGetValue(agent.agentType,
                        out Dictionary<BrainType, List<AgentNeuronData>> brainData)) return;

                foreach (KeyValuePair<int, BrainType> brainType in agent.brainTypes)
                {
                    if (!brainData.TryGetValue(brainType.Value, out List<AgentNeuronData> neuronDataList)) return;
                    if (neuronDataList.Count == 0) continue;

                    int index = random.Next(0, neuronDataList.Count);
                    AgentNeuronData neuronData = neuronDataList[index];
                    foreach (List<NeuronLayer> neuronLayer in netComponent.Layers)
                    {
                        lock (neuronLayer)
                        {
                            SetWeights(neuronLayer, neuronData.NeuronWeights);
                            neuronLayer.ForEach(neuron => neuron.AgentType = neuronData.AgentType);
                            neuronLayer.ForEach(neuron => neuron.BrainType = neuronData.BrainType);
                        }
                    }

                    lock (loadedData)
                    {
                        loadedData[agent.agentType][brainType.Value]
                            .Remove(loadedData[agent.agentType][brainType.Value][index]);
                    }
                }
            }
        }

        private void StartSimulation()
        {
            DataContainer.Agents = new Dictionary<uint, SimAgentType>();
            genAlg = new GeneticAlgorithm(eliteCount, mutationChance, mutationRate);
            GenerateInitialPopulation();
            Load(DirectoryPath);
            isRunning = true;
        }

        public void StopSimulation()
        {
            isRunning = false;
            Generation = 0;
        }

        public void PauseSimulation()
        {
            isRunning = !isRunning;
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

            foreach (KeyValuePair<int, BrainType> kvp in brainTypes)
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
            DataContainer.Scavengers.Remove(agentId);
            ECSManager.RemoveEntity(agentId);
        }

        public static void SetWeights(List<NeuronLayer> layers, float[] newWeights)
        {
            if (newWeights == null || newWeights.Length == 0)
            {
                return;
            }

            int id = 0;
            foreach (NeuronLayer layer in layers)
            {
                for (int i = 0; i < layer.NeuronsCount; i++)
                {
                    float[] ws = layer.neurons[i].weights;
                    for (int j = 0; j < ws.Length; j++)
                    {
                        if (id >= newWeights.Length)
                        {
                            break;
                        }

                        ws[j] = newWeights[id++];
                    }
                }
            }
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

        private void SpecificLoaded(bool obj)
        {
            if (obj)
            {
                Debug.Log("Specific generation loaded correctly.");
            }
            else
            {
                Debug.LogWarning("Specific generation couldn't be loaded.");
            }
        }
    }
}