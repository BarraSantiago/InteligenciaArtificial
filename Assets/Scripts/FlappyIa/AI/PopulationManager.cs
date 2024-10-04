using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    public GameObject BirdPrefab;
    public int PopulationCount = 40;
    public int IterationCount = 1;

    public int EliteCount = 4;
    public float MutationChance = 0.10f;
    public float MutationRate = 0.01f;

    public int InputsCount = 4;
    public int HiddenLayers = 1;
    public int OutputsCount = 2;
    public int NeuronsCountPerHL = 7;
    public float Bias = 1f;
    public float Sigmoid = 0.5f;

    GeneticAlgorithm genAlg;

    List<BirdBase> populationGOs = new List<BirdBase>();
    List<Genome> population = new List<Genome>();
    List<NeuralNetwork> brains = new List<NeuralNetwork>();

    bool isRunning = false;

    public int generation
    {
        get; private set;
    }

    public float bestFitness
    {
        get; private set;
    }

    public float avgFitness
    {
        get; private set;
    }

    public float worstFitness
    {
        get; private set;
    }

    private float GetBestFitness()
    {
        float fitness = 0;
        foreach (Genome g in population)
        {
            if (fitness < g.fitness)
                fitness = g.fitness;
        }

        return fitness;
    }

    private float GetAvgFitness()
    {
        float fitness = 0;
        foreach (Genome g in population)
        {
            fitness += g.fitness;
        }

        return fitness / population.Count;
    }

    private float GetWorstFitness()
    {
        float fitness = float.MaxValue;
        foreach (Genome g in population)
        {
            if (fitness > g.fitness)
                fitness = g.fitness;
        }

        return fitness;
    }

    public BirdBase GetBestAgent()
    {
        if (populationGOs.Count == 0)
        {
            return null;
        }

        BirdBase bird = populationGOs[0];
        Genome bestGenome = population[0];
        for (int i = 0; i < population.Count; i++)
        {
            if (populationGOs[i].state == BirdBase.State.Alive && population[i].fitness > bestGenome.fitness)
            {
                bestGenome = population[i];
                bird = populationGOs[i];
            }
        }

        return bird;
    }

    static PopulationManager instance = null;

    public static PopulationManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PopulationManager>();

            return instance;
        }
    }

    void Awake()
    {
        instance = this;
        Load();
    }

    public void Load()
    {
        PopulationCount = PlayerPrefs.GetInt("PopulationCount", 2);
        EliteCount = PlayerPrefs.GetInt("EliteCount", 0);
        MutationChance = PlayerPrefs.GetFloat("MutationChance", 0);
        MutationRate = PlayerPrefs.GetFloat("MutationRate", 0);
        InputsCount = PlayerPrefs.GetInt("InputsCount", 1);
        HiddenLayers = PlayerPrefs.GetInt("HiddenLayers", 5);
        OutputsCount = PlayerPrefs.GetInt("OutputsCount", 1);
        NeuronsCountPerHL = PlayerPrefs.GetInt("NeuronsCountPerHL", 1);
        Bias = PlayerPrefs.GetFloat("Bias", 0);
        Sigmoid = PlayerPrefs.GetFloat("P", 1);
    }

    void Save()
    {
        PlayerPrefs.SetInt("PopulationCount", PopulationCount);
        PlayerPrefs.SetInt("EliteCount", EliteCount);
        PlayerPrefs.SetFloat("MutationChance", MutationChance);
        PlayerPrefs.SetFloat("MutationRate", MutationRate);
        PlayerPrefs.SetInt("InputsCount", InputsCount);
        PlayerPrefs.SetInt("HiddenLayers", HiddenLayers);
        PlayerPrefs.SetInt("OutputsCount", OutputsCount);
        PlayerPrefs.SetInt("NeuronsCountPerHL", NeuronsCountPerHL);
        PlayerPrefs.SetFloat("Bias", Bias);
        PlayerPrefs.SetFloat("P", Sigmoid);
    }

    public void StartSimulation()
    {
        Save();

        genAlg = new GeneticAlgorithm(EliteCount, MutationChance, MutationRate);

        GenerateInitialPopulation();

        isRunning = true;
    }

    public void PauseSimulation()
    {
        isRunning = !isRunning;
    }

    public void StopSimulation()
    {
        Save();

        isRunning = false;

        generation = 0;

        BackgroundManager.Instance.Reset();
        ObstacleManager.Instance.Reset();
        CameraFollow.Instance.Reset();

        DestroyAgents();
    }

    private void GenerateInitialPopulation()
    {
        generation = 0;
        DestroyAgents();

        for (int i = 0; i < PopulationCount; i++)
        {
            NeuralNetwork brain = CreateBrain();

            Genome genome = new Genome(brain.GetTotalWeightsCount());

            brain.SetWeights(genome.genome);
            brains.Add(brain);

            population.Add(genome);
            populationGOs.Add(CreateBird(genome, brain));
        }
    }

    private NeuralNetwork CreateBrain()
    {
        NeuralNetwork brain = new NeuralNetwork();
        brain.AddFirstNeuronLayer(InputsCount, Bias, Sigmoid);

        for (int i = 0; i < HiddenLayers; i++)
        {
            brain.AddNeuronLayer(NeuronsCountPerHL, Bias, Sigmoid);
        }

        brain.AddNeuronLayer(OutputsCount, Bias, Sigmoid);

        return brain;
    }

    private void Epoch()
    {
        CameraFollow.Instance.Reset();
        ObstacleManager.Instance.Reset();
        BackgroundManager.Instance?.Reset();

        generation++;

        bestFitness = GetBestFitness();
        avgFitness = GetAvgFitness();
        worstFitness = GetWorstFitness();

        Genome[] newGenomes = genAlg.Epoch(population.ToArray());

        population.Clear();

        population.AddRange(newGenomes);

        for (int i = 0; i < PopulationCount; i++)
        {
            NeuralNetwork brain = brains[i];
            brain.SetWeights(newGenomes[i].genome);
            populationGOs[i].SetBrain(newGenomes[i], brain);
        }

    }

    private void FixedUpdate()
    {
        if (!isRunning)
            return;

        float dt = Time.fixedDeltaTime;

        for (int i = 0; i < Mathf.Clamp((float)IterationCount, 1, 100); i++)
        {
            CameraFollow.Instance.UpdateCamera();
            ObstacleManager.Instance.CheckAndInstatiate();

            bool areAllDead = true;

            foreach (BirdBase b in populationGOs)
            {
                b.Think(dt);
                if (b.state == BirdBase.State.Alive)
                    areAllDead = false;
            }

            if (areAllDead)
            {
                Epoch();
                break;
            }
        }
    }

    private BirdBase CreateBird(Genome genome, NeuralNetwork brain)
    {
        Vector3 position = Vector3.zero;
        GameObject go = Instantiate<GameObject>(BirdPrefab, position, Quaternion.identity);
        BirdBase b = go.GetComponent<BirdBase>();
        b.SetBrain(genome, brain);
        return b;
    }

    private void DestroyAgents()
    {
        foreach (BirdBase go in populationGOs)
            Destroy(go.gameObject);

        populationGOs.Clear();
        population.Clear();
        brains.Clear();
    }

}
