using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeuralNetworkLib.Agents.AnimalAgents;
using NeuralNetworkLib.Agents.Flocking;
using NeuralNetworkLib.Agents.TCAgent;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.ECS.FlockingECS;
using NeuralNetworkLib.ECS.NeuralNetECS;
using NeuralNetworkLib.ECS.Patron;
using NeuralNetworkLib.GraphDirectory.Voronoi;
using NeuralNetworkLib.NeuralNetDirectory;
using NeuralNetworkLib.NeuralNetDirectory.NeuralNet;
using NeuralNetworkLib.Utils;
using Pathfinder.Graph;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NeuralNetworkDirectory
{
    using AnimalAgentType = AnimalAgent<IVector, ITransform<IVector>>;
    using TCAgentType = TcAgent<IVector, ITransform<IVector>>;
    using SimBoid = Boid<IVector, ITransform<IVector>>;

    public class EcsPopulationManager : MonoBehaviour
    {
        #region Variables

        [Header("Population Setup")] 
        [SerializeField] private Mesh carnivoreMesh;
        [SerializeField] private Material carnivoreMat;
        [SerializeField] private Mesh herbivoreMesh;
        [SerializeField] private Material herbivoreMat;
        [SerializeField] private Mesh cartMesh;
        [SerializeField] private Material cartMat;
        [SerializeField] private Mesh builderMesh;
        [SerializeField] private Material builderMat;
        [SerializeField] private Mesh gathererMesh;
        [SerializeField] private Material gathererMat;

        [Header("Population Settings")] 
        [SerializeField] private int carnivoreCount = 10;
        [SerializeField] private int herbivoreCount = 20;
        [SerializeField] private float mutationRate = 0.01f;
        [SerializeField] private float mutationChance = 0.10f;
        [SerializeField] private int eliteCount = 4;

        [Header("Modifiable Settings")] 
        [SerializeField] [Range(1, 5)] private int voronoiToDraw = 0;
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
        private bool isRunning = false;
        private int missingCarnivores;
        private int missingHerbivores;
        private int behaviourCount;
        private const int CellSize = 1;
        private const float SigmoidP = .5f;
        private float accumTime;
        private const string DirectoryPath = "NeuronData";
        private GeneticAlgorithm genAlg;
        private GraphManager<IVector, ITransform<IVector>> gridManager;
        private FitnessManager<IVector, ITransform<IVector>> fitnessManager;
        private TownCenter[] townCenters = new TownCenter[3];

        private KeyValuePair<uint, AnimalAgentType>[] agentsCopy =
            new KeyValuePair<uint, AnimalAgentType>[DataContainer.Animals.Count];

        private KeyValuePair<uint, TCAgentType>[] tcAgentsCopy = new KeyValuePair<uint, TCAgentType>[72];

        private ParallelOptions parallelOptions;

        #endregion

        private Matrix4x4[] carnivoreMatrices;
        private Matrix4x4[] herbivoreMatrices;
        private Matrix4x4[] builderMatrices;
        private Matrix4x4[] cartMatrices;
        private Matrix4x4[] gathererMatrices;
        private bool _requiresRedraw;
        private readonly object _renderLock = new object();
        private const int maxBuildersCarts = 18;
        private const int maxGatherers = 18;
        private bool startSimulation = false;

        public void Awake()
        {
            parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
            carnivoreMatrices = new Matrix4x4[carnivoreCount];
            herbivoreMatrices = new Matrix4x4[herbivoreCount];
            builderMatrices = new Matrix4x4[maxBuildersCarts];
            cartMatrices = new Matrix4x4[maxBuildersCarts];
            gathererMatrices = new Matrix4x4[maxGatherers];

            uiManager = FindObjectOfType<UiManager>();
            UiManagerInit();
            gridManager = new GraphManager<IVector, ITransform<IVector>>(gridWidth, gridHeight);
            DataContainer.Graph = new Sim2DGraph(gridWidth, gridHeight, CellSize);
            DataContainer.Init();
            foreach (VoronoiDiagram<Point2D> variable in DataContainer.Voronois)
            {
                if (variable == null) continue;
                variable.ComputeCellsStandard();
            }

            NeuronDataSystem.OnSpecificLoaded += SpecificLoaded;
            Herbivore<IVector, ITransform<IVector>>.OnDeath += RemoveEntity;
            ECSManager.Init();

            //DataContainer.Graph.LoadGraph("GraphData.json");
            StartSimulation();
            fitnessManager = new FitnessManager<IVector, ITransform<IVector>>(DataContainer.Animals);
            behaviourCount = GetHighestBehaviourCount();
            startSimulation = true;
            isRunning = true;

            UpdateAgentsCopy();
            UpdateTcAgentsCopy();
        }

        private void Update()
        {
            if (!_requiresRedraw) return;

            int carnivoreIndex = 0;
            int herbivoreIndex = 0;
            int carIndex = 0;
            int buiIndex = 0;
            int gatIndex = 0;

            Parallel.ForEach(DataContainer.Animals.Keys, id =>
            {
                IVector pos = DataContainer.Animals[id].Transform.position;
                Vector3 position = new Vector3(pos.X, pos.Y);
                Matrix4x4 matrix = Matrix4x4.Translate(position);

                switch (DataContainer.Animals[id].agentType)
                {
                    case AgentTypes.Carnivore:
                        int carnIndex = Interlocked.Increment(ref carnivoreIndex) - 1;
                        if (carnIndex < carnivoreMatrices.Length)
                        {
                            carnivoreMatrices[carnIndex] = matrix;
                        }

                        break;
                    case AgentTypes.Herbivore:
                        int herbIndex = Interlocked.Increment(ref herbivoreIndex) - 1;
                        if (herbIndex < herbivoreMatrices.Length)
                        {
                            herbivoreMatrices[herbIndex] = matrix;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            Parallel.ForEach(DataContainer.TcAgents.Keys, id =>
            {
                IVector pos = DataContainer.TcAgents[id].Transform.position;
                Vector3 position = new Vector3(pos.X, pos.Y);
                Matrix4x4 matrix = Matrix4x4.Translate(position);

                switch (DataContainer.TcAgents[id].AgentType)
                {
                    case AgentTypes.Builder:
                        int builderIndex = Interlocked.Increment(ref buiIndex) - 1;
                        if (builderIndex < builderMatrices.Length)
                        {
                            builderMatrices[builderIndex] = matrix;
                        }

                        break;

                    case AgentTypes.Cart:
                        int cartIndex = Interlocked.Increment(ref carIndex) - 1;
                        if (cartIndex < cartMatrices.Length)
                        {
                            cartMatrices[cartIndex] = matrix;
                        }

                        break;

                    case AgentTypes.Gatherer:
                        int gathererIndex = Interlocked.Increment(ref gatIndex) - 1;
                        if (gathererIndex < gathererMatrices.Length)
                        {
                            gathererMatrices[gathererIndex] = matrix;
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            lock (_renderLock)
            {
                if (carnivoreMatrices.Length > 0)
                    Graphics.DrawMeshInstanced(carnivoreMesh, 0, carnivoreMat, carnivoreMatrices);

                if (herbivoreMatrices.Length > 0)
                    Graphics.DrawMeshInstanced(herbivoreMesh, 0, herbivoreMat, herbivoreMatrices.ToArray());

                if (builderMatrices.Length > 0)
                    Graphics.DrawMeshInstanced(builderMesh, 0, builderMat, builderMatrices.ToArray());

                if (gathererMatrices.Length > 0)
                    Graphics.DrawMeshInstanced(gathererMesh, 0, gathererMat, gathererMatrices.ToArray());

                if (cartMatrices.Length > 0)
                    Graphics.DrawMeshInstanced(cartMesh, 0, cartMat, cartMatrices.ToArray());
            }

            _requiresRedraw = false;
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

            _requiresRedraw = true;
            uiManager.OnGenTimeUpdate?.Invoke(accumTime);
        }

        private void UpdateAgentsCopy()
        {
            agentsCopy = DataContainer.Animals.ToArray();
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
            int index = 0;

            TCAgentType.Time = dt;
            AnimalAgentType.Time = dt;

            ECSManager.RunFlocking(dt);

            for (int i = 0; i < agentsCopy.Length; i++)
            {
                AnimalAgentType agent = agentsCopy[i].Value;
                agent.UpdateInputs();
                ECSManager.GetComponent<InputComponent>(agentsCopy[i].Key).inputs = agent.input;
            }

            for (int i = 0; i < tcAgentsCopy.Length; i++)
            {
                KeyValuePair<uint, TCAgentType> agent = tcAgentsCopy[i];
                ECSManager.GetComponent<TransformComponent>(agent.Key).Transform = agent.Value.Transform;
                ECSManager.GetComponent<InputComponent>(agentsCopy[i].Key).inputs = agent.Value.input;
            }

            ECSManager.RunSystem(dt, typeof(NeuralNetSystem));

            int flockingBrainNum = tcAgentsCopy[0].Value.GetBrainTypeKeyByValue(BrainType.Flocking);
            for (int i = 0; i < tcAgentsCopy.Length; i++)
            {
                KeyValuePair<uint, TCAgentType> agent = tcAgentsCopy[i];

                ECSManager.GetComponent<BoidConfigComponent>(agent.Key).cohesionOffset =
                    agent.Value.output[flockingBrainNum][0];
                ECSManager.GetComponent<BoidConfigComponent>(agent.Key).alignmentOffset =
                    agent.Value.output[flockingBrainNum][1];
                ECSManager.GetComponent<BoidConfigComponent>(agent.Key).directionOffset =
                    agent.Value.output[flockingBrainNum][2];
                ECSManager.GetComponent<BoidConfigComponent>(agent.Key).separationOffset =
                    agent.Value.output[flockingBrainNum][3];
            }

            ECSManager.RunSystem(dt, typeof(ACSSystem));


            for (int i = 0; i < agentsCopy.Length; i++)
            {
                KeyValuePair<uint, AnimalAgentType> agent = agentsCopy[i];

                agent.Value.output = ECSManager.GetComponent<OutputComponent>(agent.Key).Outputs;
            }

            for (int i = 0; i < tcAgentsCopy.Length; i++)
            {
                uint agent = tcAgentsCopy[i].Key;

                tcAgentsCopy[i].Value.output = ECSManager.GetComponent<OutputComponent>(agent).Outputs;
                DataContainer.TcAgents[agent].AcsVector = ECSManager.GetComponent<ACSComponent>(agent).ACS;
            }


            for (int i = 0; i < behaviourCount; i++)
            {
                int tickIndex = i;

                Parallel.For(0, agentsCopy.Length, j => { agentsCopy[j].Value.Fsm.MultiThreadTick(tickIndex); });
                Parallel.For(0, tcAgentsCopy.Length, j =>
                {
                    if (tcAgentsCopy[j].Value == null) return;

                    tcAgentsCopy[j].Value.Fsm.MultiThreadTick(tickIndex);
                });

                for (int j = 0; j < agentsCopy.Length; j++)
                {
                    agentsCopy[j].Value.Fsm.MainThreadTick(tickIndex);
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
            Generation++;
            uiManager.OnGenUpdate(Generation);
            PurgingSpecials();

            missingCarnivores = carnivoreCount - DataContainer.Animals.Count(agent =>
                agent.Value.agentType == AgentTypes.Carnivore);
            missingHerbivores = herbivoreCount - DataContainer.Animals.Count(agent =>
                agent.Value.agentType == AgentTypes.Herbivore);

            AddFitnessData();
            DataContainer.FitnessStagnationManager.AnalyzeData();

            bool remainingPopulation = DataContainer.Animals.Count > 0;

            bool remainingCarn = carnivoreCount - missingCarnivores > 1;
            bool remainingHerb = herbivoreCount - missingHerbivores > 1;
            uiManager.OnSurvivorsPerSpeciesUpdate?.Invoke(new[]
                { carnivoreCount - missingCarnivores, herbivoreCount - missingHerbivores });

            ECSManager.GetSystem<NeuralNetSystem>().Deinitialize();
            if (Generation % generationsPerSave == 0)
            {
                Save(DirectoryPath, Generation);
            }

            if (remainingPopulation)
            {
                foreach (AnimalAgentType agent in DataContainer.Animals.Values)
                {
                    Debug.Log(agent.agentType + " survived.");
                }
            }

            CleanMap();

            if (missingCarnivores == carnivoreCount) Load(AgentTypes.Carnivore);
            if (missingHerbivores == herbivoreCount) Load(AgentTypes.Herbivore);

            if (!remainingPopulation)
            {
                FillPopulation();
                return;
            }

            Dictionary<AgentTypes, Dictionary<BrainType, List<Genome>>> genomes =
                new Dictionary<AgentTypes, Dictionary<BrainType, List<Genome>>>
                {
                    [AgentTypes.Herbivore] = new(),
                    [AgentTypes.Carnivore] = new()
                };
            Dictionary<AgentTypes, Dictionary<BrainType, int>> indexes =
                new Dictionary<AgentTypes, Dictionary<BrainType, int>>
                {
                    [AgentTypes.Herbivore] = new(),
                    [AgentTypes.Carnivore] = new()
                };

            foreach (AnimalAgentType agent in DataContainer.Animals.Values) agent.Reset();

            if (remainingCarn)
                CreateNewGenomes(genomes, DataContainer.CarnBrainTypes, AgentTypes.Carnivore, carnivoreCount);
            if (remainingHerb)
                CreateNewGenomes(genomes, DataContainer.HerbBrainTypes, AgentTypes.Herbivore, herbivoreCount);

            FillPopulation();
            BrainsHandler(indexes, genomes, remainingCarn, remainingHerb);

            genomes.Clear();
            indexes.Clear();

            if (Generation % 100 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static void AddFitnessData()
        {
            GetAvgFitness(AgentTypes.Carnivore, DataContainer.CarnBrainTypes.Values.ToArray());
            GetAvgFitness(AgentTypes.Herbivore, DataContainer.HerbBrainTypes.Values.ToArray());
        }

        private static void GetAvgFitness(AgentTypes agentType, params BrainType[] brainTypes)
        {
            float[] fitness = new float[brainTypes.Length];

            int agentCount = 0;

            foreach (KeyValuePair<uint, AnimalAgentType> variable in DataContainer.Animals)
            {
                if (variable.Value.agentType != agentType) continue;
                for (int i = 0; i < fitness.Length; i++)
                {
                    fitness[i] += ECSManager.GetComponent<NeuralNetComponent>(variable.Key).Fitness[i];
                }

                agentCount++;
            }

            for (int i = 0; i < fitness.Length; i++)
            {
                DataContainer.FitnessStagnationManager.AddFitnessData(agentType, brainTypes[i],
                    fitness[i] / agentCount);
            }
        }


        private void GenerateInitialPopulation()
        {
            CreateAgents(herbivoreCount, AgentTypes.Herbivore);
            CreateAgents(carnivoreCount, AgentTypes.Carnivore);

            accumTime = 0.0f;
        }

        private void CreateAgents(int count, AgentTypes agentType)
        {
            Parallel.For((long)0, count, i =>
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

                Dictionary<int, BrainType> num = agentType switch
                {
                    AgentTypes.Carnivore => DataContainer.CarnBrainTypes,
                    AgentTypes.Herbivore => DataContainer.HerbBrainTypes,
                    _ => throw new ArgumentException("Invalid agent type")
                };

                OutputComponent outputComponent = new OutputComponent();

                ECSManager.AddComponent(entityID, outputComponent);
                outputComponent.Outputs = new float[3][];

                foreach (BrainType brain in num.Values)
                {
                    BrainConfiguration inputsCount = DataContainer.InputCountCache[(brain, agentType)];
                    outputComponent.Outputs[GetBrainTypeKeyByValue(brain, agentType)] =
                        new float[inputsCount.OutputCount];
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
                    foreach (NeuronLayer[] layerList in brain.Layers)
                    {
                        brainType = layerList[j++].BrainType;
                        SetWeights(layerList, genome.genome);
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


        private void CreateTCAgents(int count, TownCenter townCenter, AgentTypes agentType)
        {
            Parallel.For((long)0, count, i =>
            {
                uint entityID = ECSManager.CreateEntity();

                BoidConfigComponent boidConfig = new BoidConfigComponent(6, 1, 1, 1, 1);
                ACSComponent acsComponent = new ACSComponent();
                TransformComponent transformComponent = new TransformComponent();
                InputComponent inputComponent = new InputComponent();

                TCAgentType agent = agentType switch
                {
                    AgentTypes.Gatherer => new Gatherer(),
                    AgentTypes.Cart => new Cart(),
                    AgentTypes.Builder => new Builder(),
                    _ => throw new ArgumentException("Invalid agent type")
                };

                agent.TownCenter = townCenter;
                agent.CurrentNode = townCenter.Position;
                agent.Init();
                transformComponent.Transform = agent.Transform;
                ECSManager.AddComponent(entityID, acsComponent);
                ECSManager.AddComponent(entityID, boidConfig);
                ECSManager.AddComponent(entityID, transformComponent);
                ECSManager.AddComponent(entityID, inputComponent);

                lock (DataContainer.TcAgents)
                {
                    DataContainer.TcAgents[entityID] = agent;
                }
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
                new(InputCount.InputCount, InputCount.InputCount, Bias, SigmoidP)
                    { BrainType = brainType, AgentType = agentType }
            };

            foreach (int hiddenLayerInput in InputCount.HiddenLayers)
            {
                layers.Add(new NeuronLayer(layers[^1].OutputsCount, hiddenLayerInput, Bias, SigmoidP)
                    { BrainType = brainType, AgentType = agentType });
            }

            layers.Add(new NeuronLayer(layers[^1].OutputsCount, InputCount.OutputCount, Bias, SigmoidP)
                { BrainType = brainType, AgentType = agentType });

            return layers;
        }

        private void BrainsHandler(Dictionary<AgentTypes, Dictionary<BrainType, int>> indexes,
            Dictionary<AgentTypes, Dictionary<BrainType, List<Genome>>> genomes,
            bool remainingCarn, bool remainingHerb)
        {
            foreach (KeyValuePair<uint, AnimalAgentType> agent in DataContainer.Animals)
            {
                AgentTypes agentType = agent.Value.agentType;

                switch (agentType)
                {
                    case AgentTypes.Carnivore:
                        if (!remainingCarn) continue;
                        break;
                    case AgentTypes.Herbivore:
                        if (!remainingHerb) continue;
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
            CreateAgents(missingHerbivores, AgentTypes.Herbivore);
            CreateAgents(missingCarnivores, AgentTypes.Carnivore);
        }

        private void CreateNewGenomes(Dictionary<AgentTypes, Dictionary<BrainType, List<Genome>>> genomes,
            Dictionary<int, BrainType> brainTypes, AgentTypes agentType, int count)
        {
            foreach (BrainType brain in brainTypes.Values)
            {
                genomes[agentType][brain] =
                    genAlg.Epoch(GetGenomesByBrainAndAgentType(agentType, brain).ToArray(), count);
            }
        }

        private List<Genome> GetGenomesByBrainAndAgentType(AgentTypes agentType, BrainType brainType)
        {
            List<Genome> genomes = new List<Genome>();

            foreach (KeyValuePair<uint, AnimalAgentType> agentEntry in DataContainer.Animals)
            {
                uint agentId = agentEntry.Key;
                AnimalAgentType agent = agentEntry.Value;

                if (agent.agentType != agentType)
                {
                    continue;
                }

                NeuralNetComponent neuralNetComponent = ECSManager.GetComponent<NeuralNetComponent>(agentId);

                List<float> weights = new List<float>();
                foreach (NeuronLayer[] layerList in neuralNetComponent.Layers)
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

        private void CleanMap()
        {
            // TODO  clean map
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

        public void Load(AgentTypes agentType)
        {
            if (!activateLoad) return;

            //  TODO BIT MATRIX
            Dictionary<AgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> loadedData =
                NeuronDataSystem.LoadLatestNeurons(DirectoryPath);

            if (loadedData.Count == 0 || !loadedData.ContainsKey(agentType)) return;
            System.Random random = new System.Random();

            foreach (KeyValuePair<uint, AnimalAgentType> entity in DataContainer.Animals)
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
                    foreach (NeuronLayer[] neuronLayer in netComponent.Layers)
                    {
                        lock (neuronLayer)
                        {
                            SetWeights(neuronLayer, neuronData.NeuronWeights);
                            foreach (NeuronLayer layer in neuronLayer)
                            {
                                layer.AgentType = neuronData.AgentType;
                                layer.BrainType = neuronData.BrainType;
                            }
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
            Dictionary<AgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> loadedData =
                generationToLoad > 0
                    ? NeuronDataSystem.LoadSpecificNeurons(directoryPath, generationToLoad)
                    : NeuronDataSystem.LoadLatestNeurons(directoryPath);

            if (loadedData.Count == 0) return;
            System.Random random = new System.Random();

            foreach (KeyValuePair<uint, AnimalAgentType> entity in DataContainer.Animals)
            {
                NeuralNetComponent netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                if (netComponent == null || !DataContainer.Animals.TryGetValue(entity.Key, out AnimalAgentType agent))
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
                    foreach (NeuronLayer[] neuronLayer in netComponent.Layers)
                    {
                        lock (neuronLayer)
                        {
                            SetWeights(neuronLayer, neuronData.NeuronWeights);
                            foreach (NeuronLayer layer in neuronLayer)
                            {
                                layer.AgentType = neuronData.AgentType;
                                layer.BrainType = neuronData.BrainType;
                            }
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
            StartTownCenters();
            DataContainer.Animals = new Dictionary<uint, AnimalAgentType>();
            genAlg = new GeneticAlgorithm(eliteCount, mutationChance, mutationRate);
            GenerateInitialPopulation();
            Load(DirectoryPath);
            isRunning = true;
        }

        private void StartTownCenters()
        {
            DataContainer.TcAgents = new Dictionary<uint, TCAgentType>();

            townCenters[0] = new TownCenter(gridManager.GetRandomPositionInUpperQuarter());
            townCenters[1] = new TownCenter(gridManager.GetRandomPosition());
            townCenters[2] = new TownCenter(gridManager.GetRandomPositionInLowerQuarter());

            foreach (TownCenter townCenter in townCenters)
            {
                CreateTCAgents(townCenter.InitialGatherer, townCenter, AgentTypes.Gatherer);
                CreateTCAgents(townCenter.InitialBuilders, townCenter, AgentTypes.Builder);
                CreateTCAgents(townCenter.InitialCarts, townCenter, AgentTypes.Cart);
                townCenter.OnSpawnUnit += CreateTCAgents;
            }

            DataContainer.UpdateVoronoi2(NodeTerrain.TownCenter);
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
            uint agentId = DataContainer.Animals.FirstOrDefault(agent => agent.Value == simAgent).Key;
            DataContainer.Animals.Remove(agentId);
            ECSManager.RemoveEntity(agentId);
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
            uiManager.Init(Generation, generationDuration, new[] { 0, 0 }, new[] { 1f });

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
    }
}