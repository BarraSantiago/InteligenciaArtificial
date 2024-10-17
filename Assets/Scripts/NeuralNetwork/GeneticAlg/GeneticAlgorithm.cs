using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FlappyIa.GeneticAlg
{
    public class Genome
    {
        public float[] genome;
        public float fitness = 0;

        public Genome(float[] genes)
        {
            this.genome = genes;
            fitness = 0;
        }

        public Genome(int genesCount)
        {
            genome = new float[genesCount];

            for (int j = 0; j < genesCount; j++)
                genome[j] = Random.Range(-1.0f, 1.0f);

            fitness = 0;
        }

        public Genome()
        {
            fitness = 0;
        }
    }

    public class GeneticAlgorithm
    {
        private readonly List<Genome> population = new();
        private readonly List<Genome> newPopulation = new();

        float totalFitness;

        int eliteCount = 0;
        float mutationChance = 0.0f;
        float mutationRate = 0.0f;

        public GeneticAlgorithm(int eliteCount, float mutationChance, float mutationRate)
        {
            this.eliteCount = eliteCount;
            this.mutationChance = mutationChance;
            this.mutationRate = mutationRate;
        }

        public Genome[] GetRandomGenomes(int count, int genesCount)
        {
            Genome[] genomes = new Genome[count];

            for (int i = 0; i < count; i++)
            {
                genomes[i] = new Genome(genesCount);
            }

            return genomes;
        }


        public Genome[] Epoch(Genome[] oldGenomes)
        {
            totalFitness = 0;

            population.Clear();
            newPopulation.Clear();

            population.AddRange(oldGenomes);
            population.Sort(HandleComparison);

            foreach (Genome g in population)
            {
                totalFitness += g.fitness;
            }

            SelectElite();

            while (newPopulation.Count < population.Count)
            {
                Crossover();
            }

            return newPopulation.ToArray();
        }

        private void SelectElite()
        {
            for (int i = 0; i < eliteCount && newPopulation.Count < population.Count; i++)
            {
                newPopulation.Add(population[i]);
            }
        }

        private void Crossover()
        {
            Genome mom = RouletteSelection();
            Genome dad = RouletteSelection();

            Genome child1;
            Genome child2;

            UniformCrossover(mom, dad, out child1, out child2);

            newPopulation.Add(child1);
            newPopulation.Add(child2);
        }

        private void SinglePivotCrossover(Genome mom, Genome dad, out Genome child1, out Genome child2)
        {
            child1 = new Genome();
            child2 = new Genome();
            
            child1.genome = new float[mom.genome.Length];
            child2.genome = new float[mom.genome.Length];

            int pivot = Random.Range(0, mom.genome.Length);

            for (int i = 0; i < pivot; i++)
            {
                child1.genome[i] = mom.genome[i];

                if (ShouldMutate())
                    child1.genome[i] += Random.Range(-mutationRate, mutationRate);

                child2.genome[i] = dad.genome[i];

                if (ShouldMutate())
                    child2.genome[i] += Random.Range(-mutationRate, mutationRate);
            }

            for (int i = pivot; i < mom.genome.Length; i++)
            {
                child2.genome[i] = mom.genome[i];

                if (ShouldMutate())
                    child2.genome[i] += Random.Range(-mutationRate, mutationRate);

                child1.genome[i] = dad.genome[i];

                if (ShouldMutate())
                    child1.genome[i] += Random.Range(-mutationRate, mutationRate);
            }
        }

        public void DoublePivotCrossover(Genome parent1, Genome parent2, out Genome child1, out Genome child2)
        {
            List<float> parent1Chromosome = parent1.genome.ToList();

            List<float> parent2Chromosome = parent2.genome.ToList();

            int chromosomeLength = parent1Chromosome.Count - 1;

            int locus = Random.Range(0, chromosomeLength);
            int length = Random.Range(0, (int)Math.Ceiling(chromosomeLength / 2.0));

            List<float> child1Chromosome = new List<float>();
            List<float> child2Chromosome = new List<float>();

            if (locus + length > chromosomeLength)
            {
                child1Chromosome.AddRange(parent2Chromosome.GetRange(0, (locus + length) % chromosomeLength));
                child1Chromosome.AddRange(parent1Chromosome.GetRange((locus + length) % chromosomeLength, locus));
                child1Chromosome.AddRange(parent2Chromosome.GetRange(locus, parent2Chromosome.Count - locus));

                child2Chromosome.AddRange(parent1Chromosome.GetRange(0, (locus + length) % chromosomeLength));
                child2Chromosome.AddRange(parent2Chromosome.GetRange((locus + length) % chromosomeLength, locus));
                child2Chromosome.AddRange(parent1Chromosome.GetRange(locus, parent1Chromosome.Count - locus));
            }
            else
            {
                child1Chromosome.AddRange(parent1Chromosome.GetRange(0, locus));
                child1Chromosome.AddRange(parent2Chromosome.GetRange(locus, length));
                child1Chromosome.AddRange(parent1Chromosome.GetRange(locus + length, parent1Chromosome.Count - (locus + length)));

                child2Chromosome.AddRange(parent2Chromosome.GetRange(0, locus));
                child2Chromosome.AddRange(parent1Chromosome.GetRange(locus, length));
                child2Chromosome.AddRange(parent2Chromosome.GetRange(locus + length, parent2Chromosome.Count - (locus + length)));
            }

            child1 = new Genome(child1Chromosome.GetRange(0, child1Chromosome.Count - 1).ToArray());
            child2 = new Genome(child2Chromosome.GetRange(0, child2Chromosome.Count - 1).ToArray());
        }
        
        private void UniformCrossover(Genome parent1, Genome parent2, out Genome child1, out Genome child2)
        {
            child1 = new Genome();
            child2 = new Genome();
            child1.genome = new float[parent1.genome.Length];
            child2.genome = new float[parent1.genome.Length];
            
            float selectionChance = 0.5f;

            for (int i = 0; i < parent1.genome.Length; i++)
            {
                if (Random.value < selectionChance)
                {
                    child1.genome[i] = parent1.genome[i];
                    child2.genome[i] = parent2.genome[i];
                }
                else
                {
                    child1.genome[i] = parent2.genome[i];
                    child2.genome[i] = parent1.genome[i];
                }
            }
        }

        private bool ShouldMutate()
        {
            return Random.Range(0.0f, 1.0f) < mutationChance;
        }

        static int HandleComparison(Genome x, Genome y)
        {
            return x.fitness > y.fitness ? 1 : x.fitness < y.fitness ? -1 : 0;
        }


        public Genome RouletteSelection()
        {
            float rnd = Random.Range(0, Mathf.Max(totalFitness, 0));

            float fitness = 0;

            for (int i = 0; i < population.Count; i++)
            {
                fitness += Mathf.Max(population[i].fitness, 0);
                if (fitness >= rnd)
                    return population[i];
            }

            return null;
        }
    }
}