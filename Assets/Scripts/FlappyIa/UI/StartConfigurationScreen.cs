using FlappyIa.AI;
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

            populationCountSlider.value = PopulationManager.Instance.PopulationCount;
            minesCountSlider.value = PopulationManager.Instance.MinesCount;
            generationDurationSlider.value = PopulationManager.Instance.GenerationDuration;
            eliteCountSlider.value = PopulationManager.Instance.EliteCount;
            mutationChanceSlider.value = PopulationManager.Instance.MutationChance * 100.0f;
            mutationRateSlider.value = PopulationManager.Instance.MutationRate * 100.0f;
            hiddenLayersCountSlider.value = PopulationManager.Instance.HiddenLayers;
            neuronsPerHLSlider.value = PopulationManager.Instance.NeuronsCountPerHL;
            biasSlider.value = -PopulationManager.Instance.Bias;
            sigmoidSlopeSlider.value = PopulationManager.Instance.P;

            startButton.onClick.AddListener(OnStartButtonClick);        
        }

        private void OnPopulationCountChange(float value)
        {
            PopulationManager.Instance.PopulationCount = (int)value;
        
            populationCountTxt.text = string.Format(populationText, PopulationManager.Instance.PopulationCount);
        }

        private void OnMinesCountChange(float value)
        {
            PopulationManager.Instance.MinesCount = (int)value;        

            minesCountTxt.text = string.Format(minesText, PopulationManager.Instance.MinesCount);
        }

        private void OnGenerationDurationChange(float value)
        {
            PopulationManager.Instance.GenerationDuration = (int)value;
        
            generationDurationTxt.text = string.Format(generationDurationText, PopulationManager.Instance.GenerationDuration);
        }

        private void OnEliteCountChange(float value)
        {
            PopulationManager.Instance.EliteCount = (int)value;

            eliteCountTxt.text = string.Format(elitesText, PopulationManager.Instance.EliteCount);
        }

        private void OnMutationChanceChange(float value)
        {
            PopulationManager.Instance.MutationChance = value / 100.0f;

            mutationChanceTxt.text = string.Format(mutationChanceText, (int)(PopulationManager.Instance.MutationChance * 100));
        }

        private void OnMutationRateChange(float value)
        {
            PopulationManager.Instance.MutationRate = value / 100.0f;

            mutationRateTxt.text = string.Format(mutationRateText, (int)(PopulationManager.Instance.MutationRate * 100));
        }

        private void OnHiddenLayersCountChange(float value)
        {
            PopulationManager.Instance.HiddenLayers = (int)value;
        

            hiddenLayersCountTxt.text = string.Format(hiddenLayersCountText, PopulationManager.Instance.HiddenLayers);
        }

        private void OnNeuronsPerHLChange(float value)
        {
            PopulationManager.Instance.NeuronsCountPerHL = (int)value;

            neuronsPerHLCountTxt.text = string.Format(neuronsPerHLCountText, PopulationManager.Instance.NeuronsCountPerHL);
        }

        private void OnBiasChange(float value)
        {
            PopulationManager.Instance.Bias = -value;

            biasTxt.text = string.Format(biasText, PopulationManager.Instance.Bias.ToString("0.00"));
        }

        private void OnSigmoidSlopeChange(float value)
        {
            PopulationManager.Instance.P = value;

            sigmoidSlopeTxt.text = string.Format(sigmoidSlopeText, PopulationManager.Instance.P.ToString("0.00"));
        }


        private void OnStartButtonClick()
        {
            PopulationManager.Instance.StartSimulation();
            this.gameObject.SetActive(false);
            simulationScreen.SetActive(true);
        }
    
    }
}
