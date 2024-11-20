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
using NeuralNetworkDirectory.PopulationManager;
using Pathfinder;
using Pathfinder.Graph;
using StateMachine.Agents.Simulation;
using Utils;


namespace NeuralNetworkDirectory.ECS
{
    public class EcsPopulationManager : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Mesh carnivoreMesh;
        [SerializeField] private Material carnivoreMat;
        [SerializeField] private Mesh herbivoreMesh;
        [SerializeField] private Material herbivoreMat;
        [SerializeField] private Mesh scavengerMesh;
        [SerializeField] private Material scavengerMat;
        [SerializeField] private int graphSize = 50;

        [SerializeField] private int carnivoreCount = 10;
        [SerializeField] private int herbivoreCount = 20;
        [SerializeField] private int scavengerCount = 10;

        [SerializeField] private int eliteCount = 4;
        [SerializeField] private float generationDuration = 20.0f;
        [SerializeField] private float mutationChance = 0.10f;
        [SerializeField] private float mutationRate = 0.01f;
        [SerializeField] public int Generation;

        public DataContainer dataContainer;

        #endregion

        private void Awake()
        {
            ECSManager.Init();
            dataContainer = new DataContainer();
            dataContainer.gridWidth = graphSize;
            dataContainer.gridHeight = graphSize;
            dataContainer.Initialize();

            dataContainer.SimulationManager.StartSimulation(eliteCount, mutationChance, mutationRate, herbivoreCount,
                carnivoreCount, scavengerCount);
            dataContainer.accumTime = 0.0f;

            DataContainer.gridManager.InitializePlants( dataContainer.plantCount);
        }

        private void Update()
        {
            foreach (var id in DataContainer._agents.Keys)
            {
                var pos = DataContainer._agents[id].Transform.position;
                Mesh mesh = new Mesh();
                Material material = herbivoreMat;
                switch (DataContainer._agents[id].agentType)
                {
                    case SimAgentTypes.Carnivore:
                        mesh = carnivoreMesh;
                        material = carnivoreMat;
                        break;
                    case SimAgentTypes.Herbivore:
                        mesh = herbivoreMesh;
                        material = herbivoreMat;
                        break;
                    case SimAgentTypes.Scavenger:
                        mesh = scavengerMesh;
                        material = scavengerMat;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Vector3 position = new Vector3(pos.X, pos.Y);
                var renderParams = new RenderParams(material);

                Graphics.RenderMesh(renderParams, mesh, 0, Matrix4x4.Translate(position));
            }
        }

        private void FixedUpdate()
        {
            if (!DataContainer.isRunning)
                return;

            var dt = Time.fixedDeltaTime;


            for (int i = 0; i < Mathf.Clamp(dataContainer.speed / 100.0f * 50, 1, 50); i++)
            {
                EntitiesTurn(dt);

                dataContainer.accumTime += dt;
                if (!(dataContainer.accumTime >= generationDuration)) return;
                dataContainer.accumTime -= generationDuration;
                DataContainer.EpochManager.Epoch(ref Generation, dataContainer.plantCount, dataContainer.SimulationManager);
            }
        }

        private void EntitiesTurn(float dt)
        {
            dataContainer.TurnManager.UpdateInputs(DataContainer._agents);

            ECSManager.Tick(dt);

            dataContainer.TurnManager.UpdateOutputs(DataContainer._agents, DataContainer._scavengers);

            dataContainer.TurnManager.AgentsTick(DataContainer._agents, dataContainer.behaviourCount);

            dataContainer.fitnessManager.Tick();
        }

        


        public void Save(string directoryPath, int generation)
        {
            var agentsData = new List<AgentNeuronData>();

            // Create a copy of the entities collection
            var entitiesCopy = DataContainer._agents.ToList();

            Parallel.ForEach(entitiesCopy, entity =>
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

            Parallel.ForEach(DataContainer._agents, entity =>
            {
                var netComponent = ECSManager.GetComponent<NeuralNetComponent>(entity.Key);
                var agent = DataContainer._agents[entity.Key];

                if (!loadedData.TryGetValue(agent.agentType, out var brainData)) return;

                Parallel.ForEach(agent.brainTypes, brainType =>
                {
                    if (!brainData.TryGetValue(brainType.Value, out var neuronDataList)) return;

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

      
        // ENTITIES O GRAPH
        


        // ENTITIES

        // ENTITIES

        // TURN


        // ENTITIES


        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;


            foreach (var node in DataContainer.graph.NodesType)
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

                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().X, node.GetCoordinate().Y), (float)DataContainer.CellSize / 5);
                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().X, node.GetCoordinate().Y), (float)DataContainer.CellSize / 5);
            }
        }

        // EPOCH


        // EPOCH Y ENTITIES
        

        //EPOCH
    }
}