using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AINeuralNetwork : MonoBehaviour {

    public struct HiddenLayer
    {
        public List<Perceptron> hiddenPerceptrons;
    }

    [SerializeField]
    float WeightRange = 1f;
    [SerializeField]
    int nbInputPerceptrons = 1;
    [SerializeField]
    int nbHiddenLayers = 1;
    [SerializeField]
    int nbHiddenPerceptrons = 1;
    [SerializeField]
    int nbOutputPerceptrons = 1;
    [SerializeField]
    float m_gain = 1f;

    [SerializeField]
    private bool loadAIWeights = false;

    public float Gain { get { return m_gain; } }

    Perceptron bias;
    List<Perceptron> m_inputPerceptrons;
    //List<Perceptron> m_hiddenPerceptrons;
    List<HiddenLayer> m_hiddenLayers;
    List<Perceptron> m_outputPerceptrons;

	// Use this for initialization
	void Start () {
        m_inputPerceptrons = new List<Perceptron>();
        m_hiddenLayers = new List<HiddenLayer>();
        //m_hiddenPerceptrons = new List<Perceptron>();
        m_outputPerceptrons = new List<Perceptron>();

        InitPerceptrons();
    }

    void InitPerceptrons()
    {
        ANNSaveData savedata = LoadWeights();

        bias = new Perceptron(this);
        bias.State = 1f;

        InitInputPerceptrons();
        InitHiddenPerceptrons(savedata);
        InitOutputPerceptrons(savedata);
    }

    void InitInputPerceptrons()
    {
        for (int i = 0; i < nbInputPerceptrons; i++)
        {
            Perceptron perceptron = new Perceptron(this);
            m_inputPerceptrons.Add(perceptron);
        }
    }

    void InitHiddenPerceptrons(ANNSaveData savedata)
    {
        for (int layerIndex = 0; layerIndex < nbHiddenLayers; layerIndex++)
        {
            int nb;
            List<Perceptron> perceptronList;

            HiddenLayer layer;
            layer.hiddenPerceptrons = new List<Perceptron>();

            if (layerIndex == 0)
            {
                nb = nbInputPerceptrons;
                perceptronList = m_inputPerceptrons;
            }
            else
            {
                nb = nbHiddenPerceptrons;
                perceptronList = m_hiddenLayers[layerIndex - 1].hiddenPerceptrons;
            }

            for (int i = 0; i < nbHiddenPerceptrons; i++)
            {
                Perceptron perceptron = new Perceptron(this);

                for (int j = 0; j < nb; j++)
                {
                    Perceptron.Input input = new Perceptron.Input();
                    input.inputPerceptron = perceptronList[j];

                    if (savedata != null)
                        input.weight = savedata.inputLayerWeights[i].weightList[j];
                    else
                        input.weight = Random.Range(-WeightRange, WeightRange);
                    perceptron.inputs.Add(input);
                }

                Perceptron.Input biasInput = new Perceptron.Input();
                biasInput.inputPerceptron = bias;

                if (savedata != null)
                {
                    List<float> weightList = savedata.inputLayerWeights[i].weightList;
                    int lastIndex = weightList.Count - 1;
                    biasInput.weight = weightList[lastIndex];
                }
                else
                    biasInput.weight = Random.Range(-WeightRange, WeightRange);

                perceptron.inputs.Add(biasInput);

                layer.hiddenPerceptrons.Add(perceptron);
                m_hiddenLayers.Add(layer);
            }
        }
        
    }

    void InitOutputPerceptrons(ANNSaveData savedata)
    {
        List<Perceptron> lastHiddenLayerPerceptrons = m_hiddenLayers[m_hiddenLayers.Count - 1].hiddenPerceptrons;

        for (int i = 0; i < nbOutputPerceptrons; i++)
        {
            Perceptron perceptron = new Perceptron(this);

            for (int j = 0; j < nbHiddenPerceptrons; j++)
            {
                Perceptron.Input input = new Perceptron.Input();
                input.inputPerceptron = lastHiddenLayerPerceptrons[j];

                if (savedata != null)
                    input.weight = savedata.hiddenLayerWeights[i].weightList[j];
                else
                    input.weight = Random.Range(-WeightRange, WeightRange);

                perceptron.inputs.Add(input);
            }

            Perceptron.Input biasInput = new Perceptron.Input();
            biasInput.inputPerceptron = bias;

            if (savedata != null)
            {
                List<float> weightList = savedata.hiddenLayerWeights[i].weightList;
                int lastIndex = weightList.Count - 1;
                biasInput.weight = weightList[lastIndex];
            }
            else
                biasInput.weight = Random.Range(-WeightRange, WeightRange);

            perceptron.inputs.Add(biasInput);

            m_outputPerceptrons.Add(perceptron);
        }
    }

    ANNSaveData LoadWeights()
    {
        if (!loadAIWeights)
            return null;

        TextAsset file = Resources.Load<TextAsset>("AI/rec_weights");

        if (file)
            return JsonUtility.FromJson<ANNSaveData>(file.text);

        return null;
    }

    public void ComputeLearn(List<float> inputs, List<float> wantedOutputs)
    {
        Compute(inputs);
        Backpropagation(wantedOutputs);

        #if UNITY_EDITOR
            SaveTrainingData();
        #endif
    }

    public List<float> GetOutputs()
    {
        List<float> outputs = new List<float>();

        for (int i = 0; i < m_outputPerceptrons.Count; i++)
        {
            outputs.Add(m_outputPerceptrons[i].State);
        }

        return outputs;
    }

    public void Compute(List<float> inputs)
    {
        for (int i = 0; i < m_inputPerceptrons.Count; i++)
            m_inputPerceptrons[i].State = inputs[i];

        for (int i = 0; i < m_hiddenLayers.Count; i++)
        {
            List<Perceptron> perceptronList = m_hiddenLayers[i].hiddenPerceptrons;

            for (int j = 0; j < perceptronList.Count; j++)
                perceptronList[j].FeedForward();
        }

        for (int i = 0; i < m_outputPerceptrons.Count; i++)
            m_outputPerceptrons[i].FeedForward();
    }

    private void Backpropagation(List<float> wantedOutputs)
    {
        for (int i = 0; i < m_outputPerceptrons.Count; i++)
        {
            Perceptron perceptron = m_outputPerceptrons[i];
            float state = perceptron.State;

            float error = state * (1 - state) * (wantedOutputs[i] - state);
            perceptron.AdjustWeight(error);
        }

        for (int layerIndex = m_hiddenLayers.Count - 1; layerIndex >= 0; layerIndex--)
        {
            List<Perceptron> perceptronList = m_hiddenLayers[layerIndex].hiddenPerceptrons;

            for (int i = 0; i < perceptronList.Count; i++)
            {
                Perceptron perceptron = perceptronList[i];
                float state = perceptron.State;

                List<Perceptron> nextList;

                if (layerIndex < m_hiddenLayers.Count - 1)
                    nextList = m_hiddenLayers[layerIndex + 1].hiddenPerceptrons;
                else
                    nextList = m_outputPerceptrons;

                float sum = 0;
                for (int j = 0; j < nextList.Count; j++)
                    sum += nextList[j].GetIncomingWeight(perceptron) * nextList[j].Error;

                float error = state * (1 - state) * sum;
                perceptron.AdjustWeight(error);
            }
        }

        
    }

#if UNITY_EDITOR
    public void SaveTrainingData()
    {
        ANNSaveData savedata = GetData();

        string path = Application.dataPath + "/Resources/AI/rec_weights.json";
        File.WriteAllText(path, JsonUtility.ToJson(savedata));
    }

    ANNSaveData GetData()
    {
        ANNSaveData savedata = new ANNSaveData();
        savedata.inputLayerWeights = new List<PerceptronSaveData>();
        savedata.hiddenLayerWeights = new List<PerceptronSaveData>();

        for (int i = 0; i < m_hiddenLayers.Count; i++)
            GetWeightData(ref savedata.inputLayerWeights, m_hiddenLayers[i].hiddenPerceptrons);
        
        GetWeightData(ref savedata.hiddenLayerWeights, m_outputPerceptrons);

        return savedata;
    }

    void GetWeightData(ref List<PerceptronSaveData> dataList, List<Perceptron> perceptronList)
    {
        for (int i = 0; i < perceptronList.Count; i++)
        {
            Perceptron perceptron = perceptronList[i];
            PerceptronSaveData data;
            data.weightList = new List<float>();

            for (int j = 0; j < perceptron.inputs.Count; j++)
                data.weightList.Add(perceptron.inputs[j].weight);

            dataList.Add(data);
        }
    }
}
#endif