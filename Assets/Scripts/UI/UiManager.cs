using System;
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
        /*
         - UI
            generacion num
            gen time
            sobrevivientes por especie
            fitness avg de cada cerebro
            que voronoi dubujar

        - Config
            bias
            gens per save
            gen duration
            activate save/load
            which gen to load
            species count
            mutation rate
            mut chance
            elites
         */
        [SerializeField] TMP_Text fpsCounter;
        [SerializeField] TMP_Text generationNum;
        [SerializeField] TMP_Text genTime;
        [SerializeField] TMP_Text survivorsPerSpecies;
        [SerializeField] TMP_Text fitnessAvg;
        [SerializeField] Slider voronoiToDraw;
        [SerializeField] TMP_InputField bias;
        [SerializeField] TMP_InputField mutChance;
        [SerializeField] TMP_InputField mutationRate;
        [SerializeField] TMP_InputField elites;
        [SerializeField] TMP_InputField speciesCount;
        [SerializeField] TMP_InputField gensPerSave;
        [SerializeField] TMP_InputField genDuration;
        [SerializeField] TMP_InputField whichGenToLoad;
        [SerializeField] Toggle activateSave;
        [SerializeField] Toggle activateLoad;

        public Action<int> OnGenUpdate => UpdateGenerationNum;
        public Action<float> OnGenTimeUpdate => UpdateGenTime;
        public Action<int[]> OnSurvivorsPerSpeciesUpdate => UpdateSurvivorsPerSpecies;
        public Action<float[]> OnFitnessAvgUpdate => UpdateFitnessAvg;
        public Action<int> onVoronoiUpdate;
        public Action<float> onBiasUpdate;
        public Action<float> onMutChanceUpdate;
        public Action<float> onMutationRateUpdate;
        public Action<int> onElitesUpdate;
        public Action<int> onSpeciesCountUpdate;
        public Action<int> onGensPerSaveUpdate;
        public Action<int> onGenDurationUpdate;
        public Action<int> onWhichGenToLoadUpdate;
        public Action<bool, bool> onActivateSaveLoadUpdate;

        private float deltaTime;
        private void Awake()
        {
            voronoiToDraw.onValueChanged.AddListener(value => onVoronoiUpdate?.Invoke((int)value));

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
        }

        private void Update()
        {
            UpdateFPSCounter();
        }

        private void UpdateFPSCounter()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsCounter.text = $"FPS: " + fps.ToString("0.00");
        }

        public void Init(int genNum, float genTime, int[] survivorsPerSpecies, float[] fitnessAvg)
        {
            UpdateGenerationNum(genNum);
            UpdateGenTime(genTime);
            UpdateSurvivorsPerSpecies(survivorsPerSpecies);
            UpdateFitnessAvg(fitnessAvg);
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
                    Bias = 0.5f,
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
            activateSave.isOn = config.ActivateSave;
            activateLoad.isOn = config.ActivateLoad;

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
            genTime.text = time.ToString();
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

        public void UpdateFitnessAvg(float[] fitness)
        {
            string text = "";
            for (int i = 0; i < fitness.Length; i++)
            {
                text += $"Especie {i}: {fitness[i]}\n";
            }

            fitnessAvg.text = text;
        }
    }
}