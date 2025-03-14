using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeuralNetworkLib.Agents.AnimalAgents;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.ECS.NeuralNetECS;
using NeuralNetworkLib.ECS.Patron;
using NeuralNetworkLib.NeuralNetDirectory.NeuralNet;
using NeuralNetworkLib.Utils;
using Pathfinder.Graph;
using Simulation;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NeuralNetworkDirectory
{
    using AnimalAgentType = AnimalAgent<IVector, ITransform<IVector>>;

    public class EpochManager
    {
        private readonly GraphManager<IVector, ITransform<IVector>> graphManager;
        private readonly GeneticAlgorithm genAlg;
        private readonly UiManager uiManager;
        private readonly string directoryPath;
        private readonly int generationsPerSave;
        private readonly int carnivoreCount;
        private readonly int herbivoreCount;
        private CancellationTokenSource cancellationTokenSource;
        private ParallelOptions parallelOptions;
        private AgentFactory agentFactory;
        private int missingCarnivores;
        private int missingHerbivores;
        private int generationToLoad = 0;
        private bool activateSave;
        private bool activateLoad;
        public int Generation { get; private set; }

        public EpochManager(GraphManager<IVector, ITransform<IVector>> graphManager, UiManager uiManager,
            AgentFactory agentFactory, int generation, string directoryPath, bool activateSave, int generationsPerSave,
            int carnivoreCount, int herbivoreCount, ParallelOptions parallelOptions)
        {
            this.agentFactory = agentFactory;
            this.graphManager = graphManager;
            this.uiManager = uiManager;
            this.Generation = generation;
            this.directoryPath = directoryPath;
            this.activateSave = activateSave;
            this.generationsPerSave = generationsPerSave;
            this.carnivoreCount = carnivoreCount;
            this.herbivoreCount = herbivoreCount;
            this.parallelOptions = parallelOptions;

            cancellationTokenSource = new CancellationTokenSource();
            this.parallelOptions.CancellationToken = cancellationTokenSource.Token;
            uiManager.onWhichGenToLoadUpdate += UpdateWhichGenToLoad;
            uiManager.onActivateSaveLoadUpdate += UpdateActivateSaveLoad;

            genAlg = new GeneticAlgorithm(2, 0.1f, 0.1f);
        }

        private void UpdateWhichGenToLoad(int obj)
        {
            generationToLoad = obj;
        }

        public void ProcessEpoch()
        {
            ResetCancellationToken();
            IncrementGeneration();
            PurgeSpecials();
            CalculateMissingAgents();
            AddFitnessData();

            missingCarnivores = carnivoreCount - DataContainer.Animals.Count(agent =>
                agent.Value.agentType == AgentTypes.Carnivore);
            missingHerbivores = herbivoreCount - DataContainer.Animals.Count(agent =>
                agent.Value.agentType == AgentTypes.Herbivore);
            bool remainingCarn = carnivoreCount - missingCarnivores > 1;
            bool remainingHerb = herbivoreCount - missingHerbivores > 1;
            bool remainingPopulation = DataContainer.Animals.Count > 0;

            ECSManager.GetSystem<NeuralNetSystem>().Deinitialize();

            if (Generation % generationsPerSave == 0)
            {
                Save();
            }

            graphManager.CleanMap();

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
            HandleBrains(indexes, genomes, remainingCarn, remainingHerb);

            ResetFitnessValues();
            ResetAgentPositions();

            if (Generation % 100 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void ResetAgentPositions()
        {
            foreach (KeyValuePair<uint, AnimalAgentType> animalAgent in DataContainer.Animals)
            {
                INode<IVector> randomNode = animalAgent.Value.agentType switch
                {
                    AgentTypes.Carnivore => graphManager.GetRandomPositionInUpperQuarter(),
                    AgentTypes.Herbivore => graphManager.GetRandomPositionInLowerQuarter(),
                    _ => graphManager.GetRandomPosition()
                };
                animalAgent.Value.SetPosition(randomNode.GetCoordinate());
            }
        }

        private void ResetFitnessValues()
        {
            NeuralNetComponent[] comps = ECSManager.GetComponentsDirect<NeuralNetComponent>().components;
            foreach (NeuralNetComponent comp in comps)
            {
                comp.Fitness = new float[3];
                comp.FitnessMod = new float[3];

                for (int j = 0; j < comp.FitnessMod.Length; j++)
                {
                    comp.FitnessMod[j] = 1.0f;
                }
            }
        }

        private void FillPopulation()
        {
            agentFactory.CreateAnimalAgents(missingHerbivores, AgentTypes.Herbivore);
            agentFactory.CreateAnimalAgents(missingCarnivores, AgentTypes.Carnivore);
        }

        private void ResetCancellationToken()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            cancellationTokenSource = new CancellationTokenSource();
            parallelOptions.CancellationToken = cancellationTokenSource.Token;
        }

        private void IncrementGeneration()
        {
            Generation++;
            uiManager.OnGenUpdate(Generation);
        }

        private void PurgeSpecials()
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
                    EcsPopulationManager.RemoveEntity(DataContainer.Animals[agentId]);
                }
            }
        }

        private void CalculateMissingAgents()
        {
            missingCarnivores = carnivoreCount - DataContainer.Animals.Count(agent =>
                agent.Value.agentType == AgentTypes.Carnivore);
            missingHerbivores = herbivoreCount - DataContainer.Animals.Count(agent =>
                agent.Value.agentType == AgentTypes.Herbivore);

            uiManager.OnSurvivorsPerSpeciesUpdate?.Invoke(new[]
                { carnivoreCount - missingCarnivores, herbivoreCount - missingHerbivores });
        }

        private void AddFitnessData()
        {
            EcsPopulationManager.GetAvgFitness(AgentTypes.Carnivore, DataContainer.CarnBrainTypes.Values.ToArray());
            EcsPopulationManager.GetAvgFitness(AgentTypes.Herbivore, DataContainer.HerbBrainTypes.Values.ToArray());
            DataContainer.FitnessStagnationManager.AnalyzeData();
        }

        private void Save()
        {
            if (!activateSave || DataContainer.Animals.Count == 0) return;

            List<AgentNeuronData> agentsData = new List<AgentNeuronData>();
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
                        weights.AddRange(EcsPopulationManager.GetWeights(layer));
                    }

                    neuronData.NeuronWeights = weights.ToArray();
                    lock (agentsData)
                    {
                        agentsData.Add(neuronData);
                    }
                }
            }

            NeuronDataSystem.SaveNeurons(agentsData, directoryPath, Generation);
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

                if (agent.agentType != agentType) continue;

                NeuralNetComponent neuralNetComponent = ECSManager.GetComponent<NeuralNetComponent>(agentId);
                List<float> weights = new List<float>();

                foreach (NeuronLayer[] layerList in neuralNetComponent.Layers)
                {
                    foreach (NeuronLayer layer in layerList)
                    {
                        if (layer.BrainType != brainType) continue;
                        weights.AddRange(EcsPopulationManager.GetWeights(layer));
                    }
                }

                Genome genome = new Genome(weights.ToArray());
                genomes.Add(genome);
                genome.fitness = neuralNetComponent.Fitness[
                    EcsPopulationManager.GetBrainTypeKeyByValue(brainType, agentType)];
            }

            return genomes;
        }

        private void HandleBrains(Dictionary<AgentTypes, Dictionary<BrainType, int>> indexes,
            Dictionary<AgentTypes, Dictionary<BrainType, List<Genome>>> genomes, bool remainingCarn, bool remainingHerb)
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
                        continue;
                }

                NeuralNetComponent neuralNetComponent = ECSManager.GetComponent<NeuralNetComponent>(agent.Key);

                foreach (BrainType brain in agent.Value.brainTypes.Values)
                {
                    if (!indexes[agentType].ContainsKey(brain))
                    {
                        indexes[agentType][brain] = 0;
                    }

                    int index = Random.Range(0, genomes[agentType][brain].Count);

                    if (index >= genomes[agentType][brain].Count) continue;


                    EcsPopulationManager.SetWeights(
                        neuralNetComponent.Layers[
                            EcsPopulationManager.GetBrainTypeKeyByValue(brain, agent.Value.agentType)],
                        genomes[agentType][brain][index].genome);

                    genomes[agentType][brain].Remove(genomes[agentType][brain][index]);

                    agent.Value.Transform = new ITransform<IVector>(new MyVector(
                        graphManager.GetRandomPosition().GetCoordinate().X,
                        graphManager.GetRandomPosition().GetCoordinate().Y));
                    agent.Value.Reset();
                }
            }
        }

        public void Load(AgentTypes agentType)
        {
            if (!activateLoad) return;

            (Dictionary<AgentTypes, Dictionary<BrainType, List<AgentNeuronData>>>, int) data =
                NeuronDataSystem.LoadLatestNeurons(directoryPath);
            Dictionary<AgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> loadedData = data.Item1;

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
                        if (neuronLayer[0].BrainType != brainType.Value) continue;
                        lock (neuronLayer)
                        {
                            EcsPopulationManager.SetWeights(neuronLayer, neuronData.NeuronWeights);
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
            (Dictionary<AgentTypes, Dictionary<BrainType, List<AgentNeuronData>>>, int) data = generationToLoad > 0
                ? NeuronDataSystem.LoadSpecificNeurons(directoryPath, generationToLoad)
                : NeuronDataSystem.LoadLatestNeurons(directoryPath);

            Generation = data.Item2;
            uiManager.OnGenUpdate.Invoke(Generation);
            Dictionary<AgentTypes, Dictionary<BrainType, List<AgentNeuronData>>> loadedData = data.Item1;


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
                        if (neuronLayer[0].BrainType != brainType.Value) continue;
                        lock (neuronLayer)
                        {
                            EcsPopulationManager.SetWeights(neuronLayer, neuronData.NeuronWeights);
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

        private void UpdateActivateSaveLoad(bool save, bool load)
        {
            activateSave = save;
            activateLoad = load;
        }
    }
}