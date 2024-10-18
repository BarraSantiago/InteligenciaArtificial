using NeuralNetworkDirectory.AI;
using UnityEngine;
using UnityEngine.UI;

namespace FlappyIa.UI
{
    public class StartConfigurationScreen : MonoBehaviour
    {
        public Text populationCountTxt;
        public Slider populationCountSlider;
        public Text minesCountTxt;
        public Slider minesCountSlider;
        public Text generationDurationTxt;
        public Slider generationDurationSlider;
        public Text eliteCountTxt;
        public Slider eliteCountSlider;
        public Text mutationChanceTxt;
        public Slider mutationChanceSlider;
        public Text mutationRateTxt;
        public Slider mutationRateSlider;
        public Text hiddenLayersCountTxt;
        public Slider hiddenLayersCountSlider;
        public Text neuronsPerHLCountTxt;
        public Slider neuronsPerHLSlider;
        public Text biasTxt;
        public Slider biasSlider;
        public Text sigmoidSlopeTxt;
        public Slider sigmoidSlopeSlider;
        public Button startButton;
        public GameObject simulationScreen;
        public PopulationManager populationManager1;
        public PopulationManager populationManager2;

        private string populationText;
        private string minesText;
        private string generationDurationText;
        private string elitesText;
        private string mutationChanceText;
        private string mutationRateText;
        private string hiddenLayersCountText;
        private string biasText;
        private string sigmoidSlopeText;
        private string neuronsPerHLCountText;

        private void Start()
        {
            var populations = FindObjectsOfType<PopulationManager>();
            populationManager1 = populations[0];
            populationManager2 = populations[1];
            
            populationCountSlider.onValueChanged.AddListener(OnPopulationCountChange);
            minesCountSlider.onValueChanged.AddListener(OnMinesCountChange);
            generationDurationSlider.onValueChanged.AddListener(OnGenerationDurationChange);
            eliteCountSlider.onValueChanged.AddListener(OnEliteCountChange);
            mutationChanceSlider.onValueChanged.AddListener(OnMutationChanceChange);
            mutationRateSlider.onValueChanged.AddListener(OnMutationRateChange);
            hiddenLayersCountSlider.onValueChanged.AddListener(OnHiddenLayersCountChange);
            neuronsPerHLSlider.onValueChanged.AddListener(OnNeuronsPerHLChange);
            biasSlider.onValueChanged.AddListener(OnBiasChange);
            sigmoidSlopeSlider.onValueChanged.AddListener(OnSigmoidSlopeChange);

            populationText = populationCountTxt.text;
            minesText = minesCountTxt.text;
            generationDurationText = generationDurationTxt.text;
            elitesText = eliteCountTxt.text;
            mutationChanceText = mutationChanceTxt.text;
            mutationRateText = mutationRateTxt.text;
            hiddenLayersCountText = hiddenLayersCountTxt.text;
            neuronsPerHLCountText = neuronsPerHLCountTxt.text;
            biasText = biasTxt.text;
            sigmoidSlopeText = sigmoidSlopeTxt.text;

            populationCountSlider.value = populationManager1.PopulationCount;
            minesCountSlider.value = populationManager1.MinesCount;
            generationDurationSlider.value = populationManager1.GenerationDuration;
            eliteCountSlider.value = populationManager1.EliteCount;
            mutationChanceSlider.value = populationManager1.MutationChance * 100.0f;
            mutationRateSlider.value = populationManager1.MutationRate * 100.0f;
            hiddenLayersCountSlider.value = populationManager1.HiddenLayers;
            neuronsPerHLSlider.value = populationManager1.NeuronsCountPerHL;
            biasSlider.value = -populationManager1.Bias;
            sigmoidSlopeSlider.value = populationManager1.P;

            startButton.onClick.AddListener(OnStartButtonClick);        
        }

        private void OnPopulationCountChange(float value)
        {
            populationManager1.PopulationCount = (int)value;
            populationManager2.PopulationCount = (int)value;
        
            populationCountTxt.text = string.Format(populationText, populationManager1.PopulationCount);
        }

        private void OnMinesCountChange(float value)
        {
            populationManager1.MinesCount = (int)value;        
            populationManager2.MinesCount = 0;        

            minesCountTxt.text = string.Format(minesText, populationManager1.MinesCount);
        }

        private void OnGenerationDurationChange(float value)
        {
            populationManager1.GenerationDuration = (int)value;
            populationManager2.GenerationDuration = (int)value;
        
            generationDurationTxt.text = string.Format(generationDurationText, populationManager1.GenerationDuration);
        }

        private void OnEliteCountChange(float value)
        {
            populationManager1.EliteCount = (int)value;
            populationManager2.EliteCount = (int)value;

            eliteCountTxt.text = string.Format(elitesText, populationManager1.EliteCount);
        }

        private void OnMutationChanceChange(float value)
        {
            populationManager1.MutationChance = value / 100.0f;
            populationManager2.MutationChance = value / 100.0f;

            mutationChanceTxt.text = string.Format(mutationChanceText, (int)(populationManager1.MutationChance * 100));
        }

        private void OnMutationRateChange(float value)
        {
            populationManager1.MutationRate = value / 100.0f;
            populationManager2.MutationRate = value / 100.0f;

            mutationRateTxt.text = string.Format(mutationRateText, (int)(populationManager1.MutationRate * 100));
        }

        private void OnHiddenLayersCountChange(float value)
        {
            populationManager1.HiddenLayers = (int)value;
            populationManager2.HiddenLayers = (int)value;
        

            hiddenLayersCountTxt.text = string.Format(hiddenLayersCountText, populationManager1.HiddenLayers);
        }

        private void OnNeuronsPerHLChange(float value)
        {
            populationManager1.NeuronsCountPerHL = (int)value;
            populationManager2.NeuronsCountPerHL = (int)value;

            neuronsPerHLCountTxt.text = string.Format(neuronsPerHLCountText, populationManager1.NeuronsCountPerHL);
        }

        private void OnBiasChange(float value)
        {
            populationManager1.Bias = -value;
            populationManager2.Bias = -value;

            biasTxt.text = string.Format(biasText, populationManager1.Bias.ToString("0.00"));
        }

        private void OnSigmoidSlopeChange(float value)
        {
            populationManager1.P = value;
            populationManager2.P = value;

            sigmoidSlopeTxt.text = string.Format(sigmoidSlopeText, populationManager1.P.ToString("0.00"));
        }


        private void OnStartButtonClick()
        {
            populationManager1.teamId = 0;
            populationManager2.teamId = 1;
            populationManager1.StartSimulation();
            populationManager2.StartSimulation();
            this.gameObject.SetActive(false);
            simulationScreen.SetActive(true);
        }
    
    }
}
