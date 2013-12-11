using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.ComponentModel;

namespace HandwrittingRecognition
{
    public class Layer
    {
        public Neuron[] neurons;
        public double[] lastValue;
        public Layer inputLayer;

        public Layer(int neuronsCount)
        {
            neurons = new Neuron[neuronsCount];
            lastValue = new double[neuronsCount];
            for (int i = 0; i < neuronsCount; i++)
                neurons[i] = new Neuron();
        }

        public void connectInputLayer(Layer inputLayer)
        {
            this.inputLayer = inputLayer;
            int inputsCount = inputLayer.neurons.Count();
            for (int i = 0; i < neurons.Count(); i++)
                neurons[i].setInputsCount(inputsCount);
        }

        public void updateWeights(double[][] deltaWeights)
        {
            for (int i = 0; i < neurons.Count(); i++)
                neurons[i].updateWeights(deltaWeights[i]);
        }

        public double[] calculate()
        {
            for(int i=0;i<neurons.Count();i++)
                lastValue[i] = neurons[i].calculate(inputLayer.lastValue);
            return lastValue;
        }
    }


    public class NeuralNetwork
    {
        static int outputsCount = 42;
        static int hiddenNeuronsCount = 20;
        int inputsCount;

        Layer[] layers = new Layer[3];        

        //double[,] inputToHiddenWeights;
        //double[,] hiddenToOutputWeights;
        //double[] hiddenValues = new double[hiddenNeuronsCount];
        //double[,] deltaWights;
        static string pathDigitsAndLetters = @"F:\DigitDB\DigitsAndLetters\";

        int blockRows = 4;
        int blockCols = 4;
        static int picWidth = 100;
        static int picHeight = 100;
        int blockWidth;
        int blockHeight;
        int maxCount;

        public NeuralNetwork(int vectorLength)
        {
            //this.vectorLength = vectorLength;
            this.inputsCount = blockRows * blockCols;
            blockWidth = picWidth / blockCols;
            blockHeight = picHeight / blockRows;
            maxCount = blockWidth * blockHeight;

            //inputToHiddenWeights = new double[vectorLength + 1, hiddenNeuronsCount];
            //hiddenToOutputWeights = new double[hiddenNeuronsCount, outputsCount];  

            layers[0] = new Layer(inputsCount);
            layers[1] = new Layer(hiddenNeuronsCount);
            layers[2] = new Layer(outputsCount);
            

            //layers[1].connectInputLayer(layers[0]);
            layers[2].connectInputLayer(layers[0]);
        }

        /*public double[] calculateLayer(double[] inputVector, double[,] layerWeigths)
        {
            double[] result = new double[layerWeigths.GetLength(1)];
            double[] z = new double[layerWeigths.GetLength(1)];

            for (int i = 0; i < layerWeigths.GetLength(1); i++)
            {
                for (int j = 0; j < inputVector.Length; j++)
                    z[i] += inputVector[j] * layerWeigths[j, i];
                //result[i] = 1 / (1 + Math.Exp(-z[i]));
                result[i] = z[i];
            }
            return result;
        }*/

        public double[] calculateNet(double[] inputVector)
        {
            layers[0].lastValue = inputVector;
            //hiddenValues = calculateLayer(inputVector, inputToHiddenWeights);
            //return calculateLayer(hiddenValues, hiddenToOutputWeights);            

            //layers[1].calculate();
            return layers[2].calculate();
        }

        public double[] calculateNet(Bitmap bmp)
        {
            return calculateNet(getVector(bmp));
        }

        public int answer(Bitmap bmp)
        {
            double[] output = calculateNet(getVector(bmp));
            return output.ToList().IndexOf(output.Max());
        }

        double[] CountPixels(Bitmap bmp)
        {
            double[] result = new double[1];
            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                    if (bmp.GetPixel(i, j).R < 255)
                        result[0] ++;
            return result;
        }

        public double[] getVector(Bitmap bmp)
        {
            //double[] result = new double[inputsCount+1];
            //result[inputsCount] = 1;
            double[] result = new double[inputsCount];

            Rectangle copyRect;
            int counter = 0;

            for (int i = 0; i < blockCols; i++)
                for (int j = 0; j < blockRows; j++)
                {
                    copyRect = new Rectangle(i * blockWidth, j * blockHeight, blockWidth, blockHeight);
                    Bitmap partOfBmp = BmpProcesser.copyPartOfBitmap(bmp, copyRect);
                    double[] currentCount = CountPixels(partOfBmp);
                    result[counter] = currentCount[0];
                    counter++;
                }

            for (int i = 0; i < inputsCount; i++)
                result[i] /= maxCount;
            return result;
        }

        static public List<string> getPossibleOptions()
        {
            List<string> result = new List<string>();
            using (StreamReader sr = new StreamReader("possible options.txt"))
            {
                for (int i = 0; i < outputsCount; i++)
                    result.Add(sr.ReadLine());
            }
            return result;
        }

        static public int getCorrectAnswer(string fileName)
        {
            List<String> possibleOptions = getPossibleOptions();
            int result = 0;
            for (int i = fileName.Length - 1; i >= 0; i--)
                if (fileName[i] == '\\')
                {
                    result = possibleOptions.IndexOf(fileName[i + 1].ToString());
                    break;
                }
            return result;
        }

        double[][][] backPropagationStep(double[] inputVector, double[] expectedoutput, double alpha, double learningRate)
        {
            double[][] deltaOutputWeights = new double[outputsCount][];
            int previousLayerNeuronsCount = layers[2].inputLayer.neurons.Count();

            for (int i = 0; i < outputsCount; i++)
                deltaOutputWeights[i] = new double[previousLayerNeuronsCount + 1];

            double[][] deltaHiddenWeights = new double[hiddenNeuronsCount][];   
            for (int i = 0; i < hiddenNeuronsCount; i++)
                deltaHiddenWeights[i] = new double [inputsCount + 1];

            double[] d = new double[outputsCount];

            double[] output = calculateNet(inputVector);

            //for output layer
            for (int i = 0; i < outputsCount; i++)
            {
                //d[i] = output[i] * (1 - output[i]) * (expectedoutput[i] - output[i]);
                d[i] = (expectedoutput[i] - output[i]);
                for (int j = 0; j < previousLayerNeuronsCount; j++)
                    deltaOutputWeights[i][j] = alpha * deltaOutputWeights[i][j] + (1 - alpha) * learningRate * d[i] * layers[1].lastValue[j];
                deltaOutputWeights[i][previousLayerNeuronsCount] = alpha * deltaOutputWeights[i][previousLayerNeuronsCount] 
                    + (1 - alpha) * learningRate * d[i];
            }

            // for hidden layer
            /*double[] dHidden = new double[hiddenNeuronsCount];
            for (int i = 0; i < hiddenNeuronsCount; i++)
            {
                for (int j = 0; j < outputsCount; j++)
                    dHidden[i] += d[j] * layers[2].neurons[j].inputWeights[i];

                for (int j = 0; j < inputsCount; j++)
                    deltaHiddenWeights[i][j] = alpha * deltaHiddenWeights[i][j] + (1 - alpha) * learningRate * dHidden[i] * layers[0].lastValue[j];
                deltaHiddenWeights[i][inputsCount] = alpha * deltaHiddenWeights[i][inputsCount] 
                    + (1 - alpha) * learningRate * dHidden[i];
            }*/

            double[][][] result = new double[2][][];
            result[1] = deltaOutputWeights;
            result[0] = deltaHiddenWeights;

            return result;
        }

        void updateWeights(double[][][] deltaWeights)
        {
            for (int i = 2; i < layers.Count(); i++)
                layers[i].updateWeights(deltaWeights[i-1]);
        }

        public void backPropagation(BackgroundWorker bw, int learningCount, double alpha, double learningRate)
        {
            Random rand = new Random();

            int progress, maxProgress;
            Bitmap bmp;
            string[] picturePaths = Directory.GetFiles(pathDigitsAndLetters);

            progress = 0;
            
            int bathCount = 10;
            maxProgress = learningCount*bathCount;
            for (int n = 0; n < learningCount; n++)
            {
                /*double[][][] deltaWeights = new double[2][][]; //[inputsCount + 1, outputsCount];
                deltaWeights[0] = new double[hiddenNeuronsCount][];
                deltaWeights[0] = new double[outputsCount][];*/
                
                //for (int k = 0; k < bathCount; k++)
                {
                    progress++;
                    string picturePath = picturePaths[rand.Next(picturePaths.Length)];
                   
                    bw.ReportProgress((int)((float)progress / maxProgress * 100));
                    bmp = new Bitmap(picturePath);
                    bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);
                    double[] expectedOutput = new double[outputsCount];
                    expectedOutput[getCorrectAnswer(picturePath)] = 1;
                    double[][][] backPropagationStepResult = backPropagationStep(getVector(bmp), expectedOutput, alpha, learningRate);
                    updateWeights(backPropagationStepResult);


                    /*for (int i = 0; i < inputsCount + 1; i++)
                        for (int j = 0; j < outputsCount; j++)
                            deltaWeights[i, j] += backPropagationStepResult[i, j] / bathCount;*/
                    //backPropagationStep(getVector(bmp), expectedOutput, alpha, learningRate); 
                }
                
            }
        }

        public int[,] guessAll(BackgroundWorker bw, int guessingCount)
        {
            Random rand = new Random();
            int progress, maxProgress;
            progress = 0;
            int[] count = new int[outputsCount];
            Bitmap bmp;            
            int ID;
            int[,] result = new int[outputsCount, 2];
            string[] picturePaths = Directory.GetFiles(pathDigitsAndLetters);
            maxProgress = guessingCount;
            //foreach (string picturePath in picturePaths)
            for (int j = 0; j <guessingCount; j++)
            {
                string picturePath = picturePaths[rand.Next(picturePaths.Length)];
                progress++;
                bw.ReportProgress((int)((float)progress / maxProgress * 100));
                bmp = new Bitmap(picturePath);
                int correctAnswer = getCorrectAnswer(picturePath);
                bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);
                /*arr = guess(getVector(bmp));
                ID = arr.IndexOf(arr.Min());*/
                ID = answer(bmp);
                if (ID == correctAnswer)
                    result[correctAnswer, 0]++;
                else
                    result[ID, 1]++;
            }
            return result;
        }

        public void saveGuessNew(int[,] rightNwrong, string currenPath)
        {
            List<string> possibleOptions = getPossibleOptions();

            int rightSum = 0;
            int wrongSum = 0;
            currenPath =  @"weights - all\" + currenPath +  " result = ";
            using (StreamWriter sw = new StreamWriter(currenPath + ".txt"))
            {
                for (int j = 0; j < 42; j++)
                {
                    int ID = 0;
                    /*int max = 0;
                    for (int i = 0; i < rightNwrong.GetLength(0); i++)
                        if (rightNwrong[i, 0] > max)
                        {
                            max = rightNwrong[i, 0];
                            ID = i;
                        }*/
                    ID = j;
                    sw.WriteLine(possibleOptions[ID] + " " + rightNwrong[ID, 0].ToString() + " " + rightNwrong[ID, 1].ToString());
                    rightSum += rightNwrong[ID, 0];
                    wrongSum += rightNwrong[ID, 1];
                    //rightNwrong[ID, 0] = -1;
                }
                sw.WriteLine(rightSum);
            }
            File.Delete(currenPath + rightSum + " of " + (rightSum + wrongSum) + ".txt");
            File.Move(currenPath + ".txt", currenPath + rightSum + " of " + (rightSum + wrongSum) + ".txt");
        }
    }
}
