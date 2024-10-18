public class NeuronLayer
{
    private readonly float bias = 1;
    private readonly float p = 0.5f;
    private Neuron[] neurons;
    private float[] outputs;
    private int totalWeights;

    public NeuronLayer(int inputsCount, int neuronsCount, float bias, float p)
    {
        InputsCount = inputsCount;
        this.bias = bias;
        this.p = p;

        SetNeuronsCount(neuronsCount);
    }

    public int NeuronsCount => neurons.Length;

    public int InputsCount { get; }

    public int OutputsCount => outputs.Length;

    private void SetNeuronsCount(int neuronsCount)
    {
        neurons = new Neuron[neuronsCount];

        for (var i = 0; i < neurons.Length; i++)
        {
            neurons[i] = new Neuron(InputsCount + 1, bias, p);
            totalWeights += InputsCount + 1;
        }

        outputs = new float[neurons.Length];
    }

    public int SetWeights(float[] weights, int fromId)
    {
        for (var i = 0; i < neurons.Length; i++) fromId = neurons[i].SetWeights(weights, fromId);

        return fromId;
    }

    public float[] GetWeights()
    {
        var weights = new float[totalWeights];
        var id = 0;

        for (var i = 0; i < neurons.Length; i++)
        {
            var ws = neurons[i].GetWeights();

            for (var j = 0; j < ws.Length; j++)
            {
                weights[id] = ws[j];
                id++;
            }
        }

        return weights;
    }

    public float[] Synapsis(float[] inputs)
    {
        for (var j = 0; j < neurons.Length; j++) outputs[j] = neurons[j].Synapsis(inputs);

        return outputs;
    }
}