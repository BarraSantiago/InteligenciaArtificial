using NeuralNetworkDirectory.AI;
using UnityEngine;
using UnityEngine.UI;

public class SimulationScreen : MonoBehaviour
{
    public Text generationsCountTxt;
    public Text bestFitnessTxt;
    public Text avgFitnessTxt;
    public Text worstFitnessTxt;
    public Text timerTxt;
    public Slider timerSlider;
    public Button pauseBtn;
    public Button stopBtn;
    public GameObject startConfigurationScreen;
    public PopulationManager populationManager1;
    public PopulationManager populationManager2;
    private string avgFitnessText;
    private string bestFitnessText;

    private string generationsCountText;
    private int lastGeneration;
    private string timerText;
    private string worstFitnessText;

    // Start is called before the first frame update
    private void Start()
    {
        PopulationManager[] populations = FindObjectsOfType<PopulationManager>();
        populationManager1 = populations[0];
        populationManager2 = populations[1];
        timerSlider.onValueChanged.AddListener(OnTimerChange);
        timerText = timerTxt.text;

        timerTxt.text = string.Format(timerText, populationManager1.IterationCount);

        if (string.IsNullOrEmpty(generationsCountText))
            generationsCountText = generationsCountTxt.text;
        if (string.IsNullOrEmpty(bestFitnessText))
            bestFitnessText = bestFitnessTxt.text;
        if (string.IsNullOrEmpty(avgFitnessText))
            avgFitnessText = avgFitnessTxt.text;
        if (string.IsNullOrEmpty(worstFitnessText))
            worstFitnessText = worstFitnessTxt.text;

        pauseBtn.onClick.AddListener(OnPauseButtonClick);
        stopBtn.onClick.AddListener(OnStopButtonClick);
    }

    private void LateUpdate()
    {
        if (lastGeneration != populationManager1.Generation)
        {
            lastGeneration = populationManager1.Generation;
            generationsCountTxt.text = string.Format(generationsCountText, populationManager1.Generation);
            bestFitnessTxt.text = string.Format(bestFitnessText, populationManager1.BestFitness);
            avgFitnessTxt.text = string.Format(avgFitnessText, populationManager1.AvgFitness);
            worstFitnessTxt.text = string.Format(worstFitnessText, populationManager1.WorstFitness);
        }
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(generationsCountText))
            generationsCountText = generationsCountTxt.text;
        if (string.IsNullOrEmpty(bestFitnessText))
            bestFitnessText = bestFitnessTxt.text;
        if (string.IsNullOrEmpty(avgFitnessText))
            avgFitnessText = avgFitnessTxt.text;
        if (string.IsNullOrEmpty(worstFitnessText))
            worstFitnessText = worstFitnessTxt.text;

        generationsCountTxt.text = string.Format(generationsCountText, 0);
        bestFitnessTxt.text = string.Format(bestFitnessText, 0);
        avgFitnessTxt.text = string.Format(avgFitnessText, 0);
        worstFitnessTxt.text = string.Format(worstFitnessText, 0);
    }

    private void OnTimerChange(float value)
    {
        populationManager1.IterationCount = (int)value;
        populationManager2.IterationCount = (int)value;
        timerTxt.text = string.Format(timerText, populationManager1.IterationCount);
    }

    private void OnPauseButtonClick()
    {
        populationManager1.PauseSimulation();
    }

    private void OnStopButtonClick()
    {
        populationManager1.StopSimulation();
        populationManager2.StopSimulation();
        gameObject.SetActive(false);
        startConfigurationScreen.SetActive(true);
        lastGeneration = 0;
    }
}