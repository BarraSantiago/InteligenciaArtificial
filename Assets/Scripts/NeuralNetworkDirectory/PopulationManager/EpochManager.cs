using System;
using System.Collections.Generic;
using ECS.Patron;
using FlappyIa.GeneticAlg;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.NeuralNet;
using StateMachine.Agents.Simulation;
using Utils;

namespace NeuralNetworkDirectory.PopulationManager
{
    using SimAgentType = SimAgent<IVector, ITransform<IVector>>;

    public class EpochManager
    {
        private static int missingHerbivores;
        private static int missingCarnivores;
        private static int missingScavengers;
        
        public void Epoch(ref int Generation, int plantCount, SimulationManager simulationManager)
        {
            Generation++;

            PurgingSpecials();
            bool remainingPopulation = DataContainer._agents.Count > 0;
            ECSManager.GetSystem<NeuralNetSystem>().Deinitialize();
            
            // TODO save
            //if (Generation % 5 == 0) Save("NeuronData", Generation);

            DataContainer.gridManager.CleanMap();
            DataContainer.gridManager.InitializePlants(plantCount);


            FillPopulation(simulationManager);

            DataContainer._population.Clear();
            if (!remainingPopulation)
            {
                return;
            }

            Dictionary<SimAgentTypes, Dictionary<BrainType, Genome[]>> genomes = new()
            {
                [SimAgentTypes.Scavenger] = new Dictionary<BrainType, Genome[]>(),
                [SimAgentTypes.Herbivore] = new Dictionary<BrainType, Genome[]>(),
                [SimAgentTypes.Carnivore] = new Dictionary<BrainType, Genome[]>()
            };
            Dictionary<SimAgentTypes, Dictionary<BrainType, int>> indexes = new()
            {
                [SimAgentTypes.Scavenger] = new Dictionary<BrainType, int>(),
                [SimAgentTypes.Herbivore] = new Dictionary<BrainType, int>(),
                [SimAgentTypes.Carnivore] = new Dictionary<BrainType, int>()
            };


            CreateNewGenomes(genomes);

            BrainsHandler(indexes, genomes);
        }

        // EPOCH
        private void BrainsHandler(Dictionary<SimAgentTypes, Dictionary<BrainType, int>> indexes,
            Dictionary<SimAgentTypes, Dictionary<BrainType, Genome[]>> genomes)
        {
            foreach (KeyValuePair<uint, SimAgentType> agent in DataContainer._agents)
            {
                SimAgentTypes agentType = agent.Value.agentType;
                NeuralNetComponent neuralNetComponent = ECSManager.GetComponent<NeuralNetComponent>(agent.Key);

                foreach (var brain in agent.Value.brainTypes.Values)
                {
                    int brainId = agent.Value.GetBrainTypeKeyByValue(brain);
                    if (!indexes[agentType].ContainsKey(brain))
                    {
                        indexes[agentType][brain] = 0;
                    }

                    int index = indexes[agentType][brain]++;
                    if (!DataContainer._population.ContainsKey(agent.Key))
                    {
                        DataContainer._population[agent.Key] = new Dictionary<BrainType, List<Genome>>();
                    }

                    if (!DataContainer._population[agent.Key].ContainsKey(brain))
                    {
                        DataContainer._population[agent.Key][brain] = new List<Genome>();
                    }

                    neuralNetComponent.SetWeights(brainId, genomes[agentType][brain][index].genome);
                    DataContainer._population[agent.Key][brain].Add(genomes[agentType][brain][index]);
                    agent.Value.Transform = new ITransform<IVector>(new MyVector(
                        DataContainer.gridManager.GetRandomPosition().GetCoordinate().X,
                        DataContainer.gridManager.GetRandomPosition().GetCoordinate().Y));
                    agent.Value.Reset();
                }
            }
        }

        // EPOCH
        private void FillPopulation(SimulationManager simulationManager)
        {
            simulationManager.CreateAgents(missingHerbivores, SimAgentTypes.Herbivore);
            simulationManager.CreateAgents(missingCarnivores, SimAgentTypes.Carnivore);
            simulationManager.CreateAgents(missingScavengers, SimAgentTypes.Scavenger);
            missingCarnivores = 0;
            missingHerbivores = 0;
            missingScavengers = 0;
        }

        // EPOCH
        private void CreateNewGenomes(Dictionary<SimAgentTypes, Dictionary<BrainType, Genome[]>> genomes)
        {
            foreach (var brain in DataContainer.scavBrainTypes.Values)
            {
                genomes[SimAgentTypes.Scavenger][brain] =
                    DataContainer.genAlg.Epoch(GetGenomesByBrainAndAgentType(SimAgentTypes.Scavenger, brain).ToArray());
            }

            foreach (var brain in DataContainer.herbBrainTypes.Values)
            {
                genomes[SimAgentTypes.Herbivore][brain] =
                    DataContainer.genAlg.Epoch(GetGenomesByBrainAndAgentType(SimAgentTypes.Herbivore, brain).ToArray());
            }

            foreach (var brain in DataContainer.carnBrainTypes.Values)
            {
                genomes[SimAgentTypes.Carnivore][brain] =
                    DataContainer.genAlg.Epoch(GetGenomesByBrainAndAgentType(SimAgentTypes.Carnivore, brain).ToArray());
            }
        }

        // EPOCH
        public List<Genome> GetGenomesByBrainAndAgentType(SimAgentTypes agentType, BrainType brainType)
        {
            var genomes = new List<Genome>();

            foreach (var agentEntry in DataContainer._population)
            {
                var agentId = agentEntry.Key;
                var brainDict = agentEntry.Value;

                if (DataContainer._agents[agentId].agentType != agentType || !brainDict.ContainsKey(brainType)) continue;

                genomes.AddRange(brainDict[brainType]);
                genomes[^1].fitness = ECSManager.GetComponent<NeuralNetComponent>(agentId).Fitness[DataContainer._agents[agentId]
                    .GetBrainTypeKeyByValue(brainType)];
            }

            return genomes;
        }

        private void PurgingSpecials()
        {
            var agentsToRemove = new List<uint>();

            foreach (var agentEntry in DataContainer._agents)
            {
                var agent = agentEntry.Value;
                if (agent.agentType == SimAgentTypes.Herbivore)
                {
                    if (agent is Herbivore<IVector, ITransform<IVector>> { Hp: < 0 })
                    {
                        agentsToRemove.Add(agentEntry.Key);
                    }
                }

                if (agent.Food < agent.FoodLimit)
                {
                    agentsToRemove.Add(agentEntry.Key);
                }
            }

            foreach (var agentId in agentsToRemove)
            {
                DataContainer.RemoveEntity(DataContainer._agents[agentId]);
            }
        }
        
        public static void CountMissing(SimAgentTypes agentType)
        {
            switch (agentType)
            {
                case SimAgentTypes.Carnivore:
                    missingCarnivores++;
                    break;
                case SimAgentTypes.Herbivore:
                    missingHerbivores++;
                    break;
                case SimAgentTypes.Scavenger:
                    missingScavengers++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


    }
}