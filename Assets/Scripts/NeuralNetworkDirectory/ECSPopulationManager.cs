using System.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Graph;
using NeuralNetworkLib.Agents.AnimalAgents;
using NeuralNetworkLib.Agents.TCAgent;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.ECS.FlockingECS;
using NeuralNetworkLib.ECS.NeuralNetECS;
using NeuralNetworkLib.ECS.PathfinderECS;
using NeuralNetworkLib.ECS.Patron;
using NeuralNetworkLib.GraphDirectory.Voronoi;
using NeuralNetworkLib.NeuralNetDirectory;
using NeuralNetworkLib.NeuralNetDirectory.NeuralNet;
using NeuralNetworkLib.Utils;
using Pathfinder.Graph;
using Simulation;
using UI;
using UnityEngine;

namespace NeuralNetworkDirectory
{
    using AnimalAgentType = AnimalAgent<IVector, ITransform<IVector>>;
    using TCAgentType = TcAgent<IVector, ITransform<IVector>>;

    public class EcsPopulationManager : MonoBehaviour
    {
        #region Variables

        [Header("Population Settings")] [SerializeField]
        private int carnivoreCount = 10;

        [SerializeField] private int herbivoreCount = 20;
        [SerializeField] private float mutationRate = 0.01f;
        [SerializeField] private float mutationChance = 0.10f;
        [SerializeField] private int eliteCount = 4;

        [Header("Modifiable Settings")] [SerializeField] [Range(1, 5)]
        private int voronoiToDraw = 0;

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
        private UiManager uiManager;
        public static bool isRunning = false;

        private int behaviourCount;
        private const int CellSize = 1;
        private const float SigmoidP = .5f;
        private float accumTime;
        private string DirectoryPath = "NeuronData";
        private AgentFactory agentFactory;
        private GraphManager<IVector, ITransform<IVector>> graphManager;
        private FitnessManager<IVector, ITransform<IVector>> fitnessManager;
        private TownCenter[] townCenters = new TownCenter[3];
        AgentsRenderer agentsRenderer;

        private KeyValuePair<uint, AnimalAgentType>[] animalAgentsCopy =
            new KeyValuePair<uint, AnimalAgentType>[DataContainer.Animals.Count];

        private KeyValuePair<uint, TCAgentType>[] tcAgentsCopy = new KeyValuePair<uint, TCAgentType>[72];
        private EpochManager epochManager;
        private ParallelOptions parallelOptions;

        #endregion


        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public void Awake()
        {
            agentsRenderer = FindObjectOfType<AgentsRenderer>();
            DirectoryPath = Application.dataPath + "/../" + DirectoryPath;
            DataContainer.FilePath = DirectoryPath;
            parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            uiManager = FindObjectOfType<UiManager>();
            UiManagerInit();
            graphManager = new GraphManager<IVector, ITransform<IVector>>(gridWidth, gridHeight);
            DataContainer.Graph = new Sim2DGraph(gridWidth, gridHeight, CellSize);

            ECSManager.Init();
            DataContainer.Init();
            foreach (VoronoiDiagram<Point2D> variable in DataContainer.Voronois)
            {
                if (variable == null) continue;
                variable.ComputeCellsStandard();
            }

            agentFactory = new AgentFactory(graphManager, parallelOptions, Bias, SigmoidP);
            epochManager = new EpochManager(graphManager, uiManager, agentFactory, Generation, DirectoryPath, activateSave,
                generationsPerSave, carnivoreCount, herbivoreCount, parallelOptions);


            NeuronDataSystem.OnSpecificLoaded += SpecificLoaded;
            Herbivore<IVector, ITransform<IVector>>.OnDeath += RemoveEntity;

            DataContainer.IncreaseTerrain += RecreateTerrain;
            UiManager.OnSimulationStart += StartSimulation;
            UiManager.OnAlarmCall += CallAlarm;

            ThreadPool.SetMinThreads(Environment.ProcessorCount, Environment.ProcessorCount);
            ThreadPool.SetMaxThreads(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);
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

                if (accumTime > generationDuration)
                {
                    accumTime -= generationDuration;
                    Epoch();

                    UpdateAgentsCopy();
                }
            }

            bool unitSpawned = false;
            for (int j = 0; j < townCenters.Length; j++)
            {
                if (townCenters[j].ManageSpawning()) unitSpawned = true;
            }

            if (unitSpawned) UpdateTcAgentsCopy();


            agentsRenderer.Render();
            uiManager.OnGenTimeUpdate?.Invoke(accumTime);
            uiManager.UpdateFitnessAvg(0);
        }

        private void UpdateAgentsCopy()
        {
            animalAgentsCopy = DataContainer.Animals.ToArray();
            fitnessManager = new FitnessManager<IVector, ITransform<IVector>>(
                new Dictionary<uint, AnimalAgentType>(DataContainer.Animals));
        }

        private void UpdateTcAgentsCopy()
        {
            int index = 0;
            foreach (KeyValuePair<uint, TCAgentType> kvp in DataContainer.TcAgents)
            {
                if (index < tcAgentsCopy.Length)
                {
                    tcAgentsCopy[index++] = kvp;
                }
            }
        }

        private void EntitiesTurn(float dt)
        {
            TCAgentType.Time = dt;
            AnimalAgentType.Time = dt;


            for (int i = 0; i < animalAgentsCopy.Length; i++)
            {
                AnimalAgentType agent = animalAgentsCopy[i].Value;
                agent.UpdateInputs();
                ECSManager.GetComponent<InputComponent>(animalAgentsCopy[i].Key).inputs = agent.input;
            }

            for (int i = 0; i < tcAgentsCopy.Length; i++)
            {
                if (tcAgentsCopy[i].Value == null) continue;
                uint agent = tcAgentsCopy[i].Key;
                ECSManager.GetComponent<TransformComponent>(agent).Transform = DataContainer.TcAgents[agent].Transform;
            }

            ECSManager.Tick(dt);


            for (int i = 0; i < animalAgentsCopy.Length; i++)
            {
                KeyValuePair<uint, AnimalAgentType> agent = animalAgentsCopy[i];

                agent.Value.output = ECSManager.GetComponent<OutputComponent>(agent.Key).Outputs;
            }

            for (int i = 0; i < tcAgentsCopy.Length; i++)
            {
                if (tcAgentsCopy[i].Value == null) continue;
                uint agent = tcAgentsCopy[i].Key;
                PathResultComponent<SimNode<IVector>> pathResult =
                    ECSManager.GetComponent<PathResultComponent<SimNode<IVector>>>(agent);
                if (pathResult.PathFound)
                {
                    DataContainer.TcAgents[agent].Path = pathResult.Path;
                }

                DataContainer.TcAgents[agent].AcsVector = ECSManager.GetComponent<ACSComponent>(agent).ACS;
            }


            for (int i = 0; i < behaviourCount; i++)
            {
                int tickIndex = i;

                Parallel.For(0, animalAgentsCopy.Length, parallelOptions,
                    j => { animalAgentsCopy[j].Value.Fsm.MultiThreadTick(tickIndex); });
                Parallel.For(0, tcAgentsCopy.Length, parallelOptions, j =>
                {
                    if (tcAgentsCopy[j].Value == null) return;

                    tcAgentsCopy[j].Value.Fsm.MultiThreadTick(tickIndex);
                });

                for (int j = 0; j < animalAgentsCopy.Length; j++)
                {
                    animalAgentsCopy[j].Value.Fsm.MainThreadTick(tickIndex);
                }

                for (int j = 0; j < tcAgentsCopy.Length; j++)
                {
                    if (tcAgentsCopy[j].Value == null) continue;

                    tcAgentsCopy[j].Value.Fsm.MainThreadTick(tickIndex);
                }
            }

            fitnessManager.Tick();
        }

        private void Epoch()
        {
            epochManager.ProcessEpoch();
        }

        private static void AddFitnessData()
        {
            GetAvgFitness(AgentTypes.Carnivore, DataContainer.CarnBrainTypes.Values.ToArray());
            GetAvgFitness(AgentTypes.Herbivore, DataContainer.HerbBrainTypes.Values.ToArray());
        }

        public static void GetAvgFitness(AgentTypes agentType, params BrainType[] brainTypes)
        {
            foreach (var brain in brainTypes)
            {
                DataContainer.FitnessStagnationManager.AddFitnessData(agentType, brain, GetFitness(agentType, brain));
            }
        }

        public static float GetFitness(AgentTypes agentType, BrainType brainType)
        {
            float fitness = 0;
            int agentCount = 0;
            int key = GetBrainTypeKeyByValue(brainType, agentType);
            foreach (KeyValuePair<uint, AnimalAgentType> variable in DataContainer.Animals)
            {
                if (variable.Value.agentType != agentType) continue;

                fitness += ECSManager.GetComponent<NeuralNetComponent>(variable.Key).Fitness[key];

                agentCount++;
            }

            return fitness / agentCount;
        }


        private void GenerateInitialPopulation()
        {
            agentFactory.CreateAnimalAgents(herbivoreCount, AgentTypes.Herbivore);
            agentFactory.CreateAnimalAgents(carnivoreCount, AgentTypes.Carnivore);
            accumTime = 0.0f;
        }


        public void RecreateTerrain(NodeTerrain terrain)
        {
            RegenerateTerrain(terrain, 15);
        }

        private void RegenerateTerrain(NodeTerrain terrainType, int count)
        {
            SimNode<IVector>[,] allNodes = DataContainer.Graph.NodesType;
            List<SimNode<IVector>> emptyNodes = new List<SimNode<IVector>>();

            foreach (SimNode<IVector> node in allNodes)
            {
                if (node.NodeTerrain == NodeTerrain.Empty) emptyNodes.Add(node);
            }

            if (emptyNodes.Count >= count)
            {
                System.Random random = new System.Random();
                // Shuffle using Fisher-Yates algorithm
                for (int i = emptyNodes.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (emptyNodes[i], emptyNodes[j]) = (emptyNodes[j], emptyNodes[i]);
                }

                // Take first count nodes and convert to specified terrain
                for (int i = 0; i < count; i++)
                {
                    emptyNodes[i].NodeTerrain = terrainType;
                }

                // Update relevant visual data
                DataContainer.UpdateVoronoi(terrainType);
                Debug.Log($"Regenerated {count} {terrainType} nodes as they were missing from the map");
            }
            else
            {
                Debug.LogWarning($"Not enough empty nodes to create {count} {terrainType} nodes");
            }
        }

        private void Save(string directoryPath, int generation)
        {
            if (!activateSave) return;

            List<AgentNeuronData> agentsData = new List<AgentNeuronData>();

            if (DataContainer.Animals.Count == 0) return;

            List<KeyValuePair<uint, AnimalAgentType>> entitiesCopy = DataContainer.Animals.ToList();

            agentsData.Capacity = entitiesCopy.Count * DataContainer.InputCountCache.Count;

            foreach (KeyValuePair<uint, AnimalAgentType> entity in entitiesCopy)
            {
                NeuralNetComponent netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                foreach (NeuronLayer[] neuronLayers in netComponent.Layers)
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


        private void StartSimulation()
        {
            StartTownCenters();
            DataContainer.Animals = new ConcurrentDictionary<uint, AnimalAgentType>();
            GenerateInitialPopulation();
            if (activateLoad) epochManager.Load(DirectoryPath);
            isRunning = true;
            fitnessManager = new FitnessManager<IVector, ITransform<IVector>>(
                new Dictionary<uint, AnimalAgentType>(DataContainer.Animals));
            behaviourCount = GetHighestBehaviourCount();

            UpdateAgentsCopy();
            UpdateTcAgentsCopy();
        }

        private void StartTownCenters()
        {
            DataContainer.TcAgents = new ConcurrentDictionary<uint, TCAgentType>();

            townCenters[0] = new TownCenter(graphManager.GetRandomPositionInUpperQuarter());
            townCenters[1] = new TownCenter(graphManager.GetRandomPosition());
            townCenters[2] = new TownCenter(graphManager.GetRandomPositionInLowerQuarter());

            foreach (TownCenter townCenter in townCenters)
            {
                agentFactory.CreateTownCenterAgents(townCenter.InitialGatherer, townCenter, AgentTypes.Gatherer);
                agentFactory.CreateTownCenterAgents(townCenter.InitialBuilders, townCenter, AgentTypes.Builder);
                agentFactory.CreateTownCenterAgents(townCenter.InitialCarts, townCenter, AgentTypes.Cart);
                townCenter.OnSpawnUnit += agentFactory.CreateTownCenterAgents;
            }

            DataContainer.UpdateVoronoi(NodeTerrain.TownCenter);
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

            foreach (AnimalAgentType entity in DataContainer.Animals.Values)
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

        public static int GetBrainTypeKeyByValue(BrainType value, AgentTypes agentType)
        {
            Dictionary<int, BrainType> brainTypes = agentType switch
            {
                AgentTypes.Carnivore => DataContainer.CarnBrainTypes,
                AgentTypes.Herbivore => DataContainer.HerbBrainTypes,
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


        private void PurgingSpecials()
        {
            List<uint> agentsToRemove = new List<uint>();

            foreach (KeyValuePair<uint, AnimalAgentType> agentEntry in DataContainer.Animals)
            {
                AnimalAgentType agent = agentEntry.Value;
                if (agent.agentType == AgentTypes.Herbivore)
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
                if (DataContainer.Animals.ContainsKey(agentId))
                {
                    RemoveEntity(DataContainer.Animals[agentId]);
                }
            }

            agentsToRemove.Clear();
        }

        public static void RemoveEntity(AnimalAgentType simAgent)
        {
            simAgent.Uninit();
            uint agentId = 0;

            // Find the key without locking
            foreach (var pair in DataContainer.Animals)
            {
                if (pair.Value == simAgent)
                {
                    agentId = pair.Key;
                    break;
                }
            }

            if (agentId != 0)
            {
                // Thread-safe removal
                DataContainer.Animals.TryRemove(agentId, out _);
                ECSManager.RemoveEntity(agentId);
            }
        }

        public static void SetWeights(NeuronLayer[] layers, float[] newWeights)
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

        #region UiConfig

        private void UiManagerInit()
        {
            uiManager.Init(Generation, generationDuration, new[] { 0, 0 });

            uiManager.onBiasUpdate += UpdateBias;
            uiManager.onMutChanceUpdate += UpdateMutChance;
            uiManager.onMutationRateUpdate += UpdateMutationRate;
            uiManager.onElitesUpdate += UpdateElites;
            uiManager.onSpeciesCountUpdate += UpdateSpeciesCount;
            uiManager.onGensPerSaveUpdate += UpdateGensPerSave;
            uiManager.onGenDurationUpdate += UpdateGenDuration;
            uiManager.onWhichGenToLoadUpdate += UpdateWhichGenToLoad;
            uiManager.onActivateSaveLoadUpdate += UpdateActivateSaveLoad;
            uiManager.onVoronoiUpdate += VoronoiToDraw;
            uiManager.onSpeedUpdate += i => speed = i;
            uiManager.LoadConfig();
        }

        private void VoronoiToDraw(int obj)
        {
            voronoiToDraw = obj;
        }

        private void UpdateActivateSaveLoad(bool save, bool load)
        {
            activateSave = save;
            activateLoad = load;
        }

        private void UpdateWhichGenToLoad(int obj)
        {
            generationToLoad = obj;
        }

        private void UpdateGensPerSave(int obj)
        {
            generationsPerSave = obj;
        }

        private void UpdateGenDuration(int obj)
        {
            generationDuration = obj;
        }

        private void UpdateSpeciesCount(int obj)
        {
            herbivoreCount = obj;
            carnivoreCount = obj / 2;
        }

        private void UpdateElites(int obj)
        {
            eliteCount = obj;
        }

        private void UpdateMutationRate(float obj)
        {
            mutationRate = obj;
        }

        private void UpdateMutChance(float obj)
        {
            mutationChance = obj;
        }

        private void UpdateBias(float obj)
        {
            Bias = obj;
        }

        #endregion

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


        private void CallAlarm()
        {
            foreach (TownCenter townCenter in townCenters)
            {
                townCenter.SoundAlarm();
            }
        }
    }
}