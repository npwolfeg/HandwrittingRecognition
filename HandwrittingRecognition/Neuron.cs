using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwrittingRecognition
{
    public class Neuron
    {
        public double[] inputWeights;
        public double lastValue;

        public void setInputsCount(int inputsCount)
        {
            inputWeights = new double[inputsCount+1];
            Random rand = new Random();
            for (int i = 0; i < inputsCount+1; i++)
                inputWeights[i] = rand.Next(100) / 100;
        }

        public void updateWeights(double[] deltaWeights)
        {
            for (int i = 0; i < inputWeights.Count(); i++)
                inputWeights[i] += deltaWeights[i];
        }

        public double calculate(double[] inputVector)
        {
            lastValue = 0;
            for (int j = 0; j < inputVector.Length; j++)
                lastValue += inputVector[j] * inputWeights[j];
            lastValue += inputWeights[inputWeights.Count()-1];
            return lastValue;
        }
    }
}
