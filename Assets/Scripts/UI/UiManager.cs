using System;
using System.Collections.Generic;
using NeuralNetworkDirectory;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.GraphDirectory.Voronoi;
using NeuralNetworkLib.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [Serializable]
    public class UIConfig
    {
        public float Bias;
        public float MutChance;
        public float MutationRate;
        public int Elites;
        public int SpeciesCount;
        public int GensPerSave;
        public int GenDuration;
        public int WhichGenToLoad;
        public bool ActivateSave;
        public bool ActivateLoad;
    }

    public class UiManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text fpsCounter;
        [SerializeField] private TMP_Text generationNum;
        [SerializeField] private TMP_Text genTime;
        [SerializeField] private TMP_Text survivorsPerSpecies;
        [SerializeField] private TMP_Text fitnessAvg;
        [SerializeField] private Slider voronoiToDraw;
        [SerializeField] private Slider speed;
        [SerializeField] private TMP_InputField bias;
        [SerializeField] private TMP_InputField mutChance;
        [SerializeField] private TMP_InputField mutationRate;
        [SerializeField] private TMP_InputField elites;
        [SerializeField] private TMP_InputField speciesCount;
        [SerializeField] private TMP_InputField gensPerSave;
        [SerializeField] private TMP_InputField genDuration;
        [SerializeField] private TMP_InputField whichGenToLoad;
        [SerializeField] private TMP_InputField nodeX;
        [SerializeField] private TMP_InputField nodeY;
        [SerializeField] private TMP_Dropdown nodeTerrain;
        [SerializeField] private TMP_Dropdown nodeType;
        [SerializeField] private TMP_Dropdown fitnessAgentType;
        [SerializeField] private TMP_Dropdown fitnessBrain;

        [SerializeField] private Toggle activateSave;
        [SerializeField] private Toggle activateLoad;
        [SerializeField] private Toggle activateVoronoi;
        [SerializeField] private Button balanceVoronoiButton;
        [SerializeField] private Button startSimulationButton;
        [SerializeField] private Button updateNodeButton;
        [SerializeField] private double correctionFactor;
        [SerializeField] private double snapDistance;
        [SerializeField] private int iterations = 3;
        public Action<int> OnGenUpdate => UpdateGenerationNum;
        public Action<float> OnGenTimeUpdate => UpdateGenTime;
        public Action<int[]> OnSurvivorsPerSpeciesUpdate => UpdateSurvivorsPerSpecies;
        public Action<int> OnFitnessAvgUpdate => UpdateFitnessAvg;
        public Action<int> onVoronoiUpdate;
        public Action onDrawVoronoi;
        public Action<int> onSpeedUpdate;
        public Action<float> onBiasUpdate;
        public Action<float> onMutChanceUpdate;
        public Action<float> onMutationRateUpdate;
        public Action<int> onElitesUpdate;
        public Action<int> onSpeciesCountUpdate;
        public Action<int> onGensPerSaveUpdate;
        public Action<int> onGenDurationUpdate;
        public Action<int> onWhichGenToLoadUpdate;
        public Action<bool, bool> onActivateSaveLoadUpdate;
        public static Action OnSimulationStart;

        private float deltaTime;

        private void Awake()
        {
            speed.minValue = 1;
            speed.maxValue = 10;
            voronoiToDraw.onValueChanged.AddListener(value => onVoronoiUpdate?.Invoke((int)value));
            speed.onValueChanged.AddListener(value => onSpeedUpdate?.Invoke((int)value));

            activateLoad.onValueChanged.AddListener(activate =>
                onActivateSaveLoadUpdate?.Invoke(activateSave.isOn, activate));
            activateSave.onValueChanged.AddListener(activate =>
                onActivateSaveLoadUpdate?.Invoke(activate, activateLoad.isOn));
            bias.onEndEdit.AddListener(value => onBiasUpdate?.Invoke(float.Parse(value)));
            mutChance.onEndEdit.AddListener(value => onMutChanceUpdate?.Invoke(float.Parse(value)));
            mutationRate.onEndEdit.AddListener(value => onMutationRateUpdate?.Invoke(float.Parse(value)));
            elites.onEndEdit.AddListener(value => onElitesUpdate?.Invoke(int.Parse(value)));
            speciesCount.onEndEdit.AddListener(value => onSpeciesCountUpdate?.Invoke(int.Parse(value)));
            gensPerSave.onEndEdit.AddListener(value => onGensPerSaveUpdate?.Invoke(int.Parse(value)));
            genDuration.onEndEdit.AddListener(value => onGenDurationUpdate?.Invoke(int.Parse(value)));
            whichGenToLoad.onEndEdit.AddListener(value => onWhichGenToLoadUpdate?.Invoke(int.Parse(value)));
            activateVoronoi.onValueChanged.AddListener(value => onDrawVoronoi?.Invoke());
            balanceVoronoiButton.onClick.AddListener(BalanceVoronoi);
            nodeType.onValueChanged.AddListener(UpdateNodeType);
            nodeX.onValueChanged.AddListener(value => CheckValue(value, nodeY.text));
            nodeY.onValueChanged.AddListener(value => CheckValue(nodeX.text, value));
            updateNodeButton.onClick.AddListener(UpdateNode);
            fitnessAgentType.onValueChanged.AddListener(UpdateFitnessBrain);
            fitnessBrain.onValueChanged.AddListener(UpdateFitnessAvg);
            startSimulationButton.onClick.AddListener(SimulationStart);
        }

        private void Update()
        {
            if(!EcsPopulationManager.isRunning) return;
            
            UpdateFPSCounter();
        }

        private void UpdateFPSCounter()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsCounter.text = $"FPS: " + fps.ToString("0.00");
        }

        public void Init(int genNum, float genTime, int[] survivorsPerSpecies)
        {
            UpdateGenerationNum(genNum);
            UpdateGenTime(genTime);
            UpdateSurvivorsPerSpecies(survivorsPerSpecies);
        }
        
        private void SimulationStart()
        {
            OnSimulationStart?.Invoke();
            startSimulationButton.interactable = false;
            startSimulationButton.gameObject.SetActive(false);
        }

        public void SaveConfig()
        {
            UIConfig config = new UIConfig
            {
                Bias = float.Parse(bias.text),
                MutChance = float.Parse(mutChance.text),
                MutationRate = float.Parse(mutationRate.text),
                Elites = int.Parse(elites.text),
                SpeciesCount = int.Parse(speciesCount.text),
                GensPerSave = int.Parse(gensPerSave.text),
                GenDuration = int.Parse(genDuration.text),
                WhichGenToLoad = int.Parse(whichGenToLoad.text),
                ActivateSave = activateSave.isOn,
                ActivateLoad = activateLoad.isOn
            };

            string json = JsonUtility.ToJson(config);
            System.IO.File.WriteAllText(Application.persistentDataPath + "/config.json", json);
        }

        public void LoadConfig()
        {
            string path = Application.persistentDataPath + "/config.json";
            UIConfig config = new UIConfig();
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                config = JsonUtility.FromJson<UIConfig>(json);
            }
            else
            {
                config = new UIConfig
                {
                    Bias = .4f,
                    MutChance = 0.07f,
                    MutationRate = 0.1f,
                    Elites = 4,
                    SpeciesCount = 15,
                    GensPerSave = 50,
                    GenDuration = 20,
                    WhichGenToLoad = 0,
                    ActivateSave = true,
                    ActivateLoad = true
                };
            }

            bias.text = config.Bias.ToString();
            mutChance.text = config.MutChance.ToString();
            mutationRate.text = config.MutationRate.ToString();
            elites.text = config.Elites.ToString();
            speciesCount.text = config.SpeciesCount.ToString();
            gensPerSave.text = config.GensPerSave.ToString();
            genDuration.text = config.GenDuration.ToString();
            whichGenToLoad.text = config.WhichGenToLoad.ToString();

            onVoronoiUpdate?.Invoke(1);
            onBiasUpdate?.Invoke(config.Bias);
            onMutChanceUpdate?.Invoke(config.MutChance);
            onMutationRateUpdate?.Invoke(config.MutationRate);
            onElitesUpdate?.Invoke(config.Elites);
            onSpeciesCountUpdate?.Invoke(config.SpeciesCount);
            onGensPerSaveUpdate?.Invoke(config.GensPerSave);
            onGenDurationUpdate?.Invoke(config.GenDuration);
            onWhichGenToLoadUpdate?.Invoke(config.WhichGenToLoad);
            onActivateSaveLoadUpdate?.Invoke(activateSave.isOn, activateLoad.isOn);
        }

        public void UpdateGenerationNum(int num)
        {
            generationNum.text = "Gen: " + num.ToString();
        }

        public void UpdateGenTime(float time)
        {
            genTime.text = time.ToString("0.0");
        }

        public void UpdateSurvivorsPerSpecies(int[] survivors)
        {
            string text = "";
            for (int i = 0; i < survivors.Length; i++)
            {
                text += $"Especie {i}: {survivors[i]}\n";
            }

            survivorsPerSpecies.text = text;
        }

        public void UpdateFitnessAvg(int fitness)
        {
            string text = "";
            AgentTypes agentType = fitnessAgentType.value == 0 ? AgentTypes.Carnivore : AgentTypes.Herbivore;
            BrainType brainType = BrainType.Movement;
            switch (fitnessBrain.options[fitnessBrain.value].text.ToString())
            {
                case "Move":
                    brainType = BrainType.Movement;
                    break;
                case "Attack":
                    brainType = BrainType.Attack;
                    break;
                case "Escape":
                    brainType = BrainType.Escape;
                    break;
                case "Eat":
                    brainType = BrainType.Eat;
                    break;
                default:
                    brainType = BrainType.Movement;
                    break;
            }

            float totalFitness = EcsPopulationManager.GetFitness(agentType, (BrainType)brainType);
            text = totalFitness.ToString("0.0");
            fitnessAvg.text = text;
        }

        private void UpdateFitnessBrain(int arg0)
        {
            switch (arg0)
            {
                default:
                case 0:
                    fitnessBrain.ClearOptions();

                    fitnessBrain.AddOptions(new List<string> { "Move", "Attack" });
                    break;
                case 1:
                    fitnessBrain.ClearOptions();
                    fitnessBrain.AddOptions(new List<string> { "Move", "Escape", "Eat" });
                    break;
            }

            fitnessBrain.value = 0;
        }

        public static Action<IVector, NodeTerrain, NodeTerrain> OnNodeUpdate;

        private void UpdateNode()
        {
            if (nodeX.text == "" || nodeY.text == "") return;
            int x = int.Parse(nodeX.text);
            int y = int.Parse(nodeY.text);
            NodeType type = (NodeType)nodeType.value;
            NodeTerrain terrain = type switch
            {
                NodeType.Lake => NodeTerrain.Lake,
                NodeType.Mountain => NodeTerrain.Mountain,
                _ => nodeTerrain.value switch
                {
                    0 => NodeTerrain.Empty,
                    1 => NodeTerrain.Mine,
                    2 => NodeTerrain.Tree,
                    3 => NodeTerrain.Stump,
                    4 => NodeTerrain.WatchTower,
                    _ => NodeTerrain.Empty
                }
            };

            IVector coord = new MyVector(x, y);
            NodeTerrain oldTerrain = DataContainer.GetNode(coord).NodeTerrain;
            DataContainer.NodeUpdater.UpdateNode(coord, terrain, type);

            OnNodeUpdate?.Invoke(coord, oldTerrain, terrain);
        }

        private void CheckValue(string x, string y)
        {
            if (nodeY.text != "")
            {
                int newY = int.Parse(y);
                if (newY < 0) newY = 0;
                if (newY >= DataContainer.Graph.MaxY) newY = DataContainer.Graph.MaxY - 1;
                nodeY.text = newY.ToString();
            }

            if (nodeX.text == "") return;
            int newX = int.Parse(x);

            if (newX < 0) newX = 0;
            if (newX >= DataContainer.Graph.MaxX) newX = DataContainer.Graph.MaxX - 1;

            nodeX.text = newX.ToString();
        }

        private void UpdateNodeType(int value)
        {
            nodeTerrain.interactable = (NodeType)value != NodeType.Lake && (NodeType)value != NodeType.Mountain;
        }

        private void BalanceVoronoi()
        {
            DataContainer.Voronois[(int)voronoiToDraw.value].BalanceCells(correctionFactor, 2000, iterations);
        }
    }
}