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
    public class NeuralNetwork
    {
        static int optionsCount = 42;
        int vectorLength;
        double[][] weights;
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
            this.vectorLength = blockRows * blockCols;
            blockWidth = picWidth / blockCols;
            blockHeight = picHeight / blockRows;
            maxCount = blockWidth * blockHeight;

            weights = new double[vectorLength + 1][];
            for (int i = 0; i < vectorLength + 1;i++ )
                weights[i] = new double[optionsCount];
        }

        public double[] calculate(double[] inputVector)
        {
            double[] result = new double[optionsCount];
            double[] z = new double[optionsCount];

            for (int i = 0; i < optionsCount; i++)
            {
                for (int j = 0; j < inputVector.Length; j++)
                    z[i] += inputVector[j] * weights[j][i];
                //result[i] = 1 / (1 + Math.Exp(-z[i]));
                result[i] = z[i];
            }
            return result;
        }

        public double[] calculate(Bitmap bmp)
        {
            return calculate(getVector(bmp));
        }

        public int answer(Bitmap bmp)
        {
            double[] output = calculate(getVector(bmp));
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
            double[] result = new double[vectorLength+1];
            result[vectorLength] = 1;

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

            for (int i = 0; i < vectorLength; i++)
                result[i] /= maxCount;
            return result;
        }

        static public List<string> getPossibleOptions()
        {
            List<string> result = new List<string>();
            using (StreamReader sr = new StreamReader("possible options.txt"))
            {
                for (int i = 0; i < optionsCount; i++)
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

        double[,] backPropagationStep(double[] inputVector, double[] expectedoutput, double alpha, double learningRate)
        {
            double[,] deltaWights = new double[vectorLength + 1, optionsCount];

            double[] d = new double[optionsCount];
            double[] output = calculate(inputVector);
            for (int i = 0; i < optionsCount; i++)
            {
                //d[i] = output[i] * (1 - output[i]) * (expectedoutput[i] - output[i]);
                d[i] = (expectedoutput[i] - output[i]);
                for (int j = 0; j < inputVector.Length; j++)
                {
                    deltaWights[j, i] = alpha * deltaWights[j, i] + (1 - alpha) * learningRate * d[i] * inputVector[j];
                    //deltaWights[j, i] = learningRate * d[i] * inputVector[j];                   
                }
            }
            return deltaWights;
        }

        void updateWeights(double[,] deltaWights)
        {
            for (int i = 0; i < optionsCount; i++)
                for (int j = 0; j < vectorLength+1; j++)
                    weights[j][i] += deltaWights[j, i];
        }

        public void backPropagation(BackgroundWorker bw, int learningCount,double alpha, double learningRate)
        {
            Random rand = new Random();
            for (int i = 0; i < vectorLength + 1; i++)
                for (int j = 0; j < optionsCount; j++)
                    weights[i][j] = (rand.Next(100)/100);


            int progress, maxProgress;
            Bitmap bmp;
            string[] picturePaths = Directory.GetFiles(pathDigitsAndLetters);

            progress = 0;
            
            int bathCount = 10;
            maxProgress = learningCount*bathCount;
            for (int n = 0; n < learningCount; n++)
            {
                double[,] deltaWights = new double[vectorLength + 1, optionsCount];
                
                for (int k = 0; k < bathCount; k++)
               {
                    progress++;
                    string picturePath = picturePaths[rand.Next(picturePaths.Length)];
                   
                    bw.ReportProgress((int)((float)progress / maxProgress * 100));
                    bmp = new Bitmap(picturePath);
                    bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);
                    double[] expectedOutput = new double[optionsCount];
                    expectedOutput[getCorrectAnswer(picturePath)] = 1;
                    double[,] backPropagationStepResult = backPropagationStep(getVector(bmp), expectedOutput, alpha, learningRate);
                    for (int i = 0; i < vectorLength + 1; i++)
                        for (int j = 0; j < optionsCount; j++)
                            deltaWights[i, j] += backPropagationStepResult[i, j] / bathCount;
                }
                updateWeights(deltaWights);
            }
        }

        public int[,] guessAll(BackgroundWorker bw, int guessingCount)
        {
            Random rand = new Random();
            int progress, maxProgress;
            progress = 0;
            int[] count = new int[optionsCount];
            Bitmap bmp;            
            int ID;
            int[,] result = new int[optionsCount, 2];
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
                    ID = j;
                    sw.WriteLine(possibleOptions[ID] + " " + rightNwrong[ID, 0].ToString() + " " + rightNwrong[ID, 1].ToString());
                    rightSum += rightNwrong[ID, 0];
                    wrongSum += rightNwrong[ID, 1];
                }
                sw.WriteLine(rightSum);
            }
            File.Delete(currenPath + rightSum + " of " + (rightSum + wrongSum) + ".txt");
            File.Move(currenPath + ".txt", currenPath + rightSum + " of " + (rightSum + wrongSum) + ".txt");
        }
    }
}
