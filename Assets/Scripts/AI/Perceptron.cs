using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perceptron {

    public class Input
    {
        public Perceptron inputPerceptron;
        public float weight;
    }

    public List<Input> inputs;
    public float State = 0f;
    public float Error = 0f;
    public float BetaWidth = 1f;

    AINeuralNetwork network = null;

    public Perceptron(AINeuralNetwork newNetwork)
    {
        inputs = new List<Input>();
        network = newNetwork;
    }

    public void FeedForward()
    {
        float sum = 0f;

        for (int i = 0; i < inputs.Count; i++)
        {
            sum += inputs[i].inputPerceptron.State * inputs[i].weight;
        }

        State = Threshold(sum);
    }

    float Threshold(float sum)
    {
        return 1f / (1f + Mathf.Exp(-BetaWidth * sum));
    }

    public void AdjustWeight(float currentError)
    {
        for (int i = 0; i < inputs.Count; i++)
        {
            Input input = inputs[i];
            float deltaWeight = network.Gain * currentError * input.inputPerceptron.State;
            input.weight += deltaWeight;
            Error = currentError;
        }
    }

    public float GetIncomingWeight(Perceptron perceptron)
    {
        for (int i = 0; i < inputs.Count; i++)
        {
            if (inputs[i].inputPerceptron == perceptron)
                return inputs[i].weight;
        }

        return 0f;
    }
}
