using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace HandwrittingRecognition
{
    public class SimpleLearning
    {
        static int optionsCount = 10;
        public int vectorLength = 100*100;
        public double[][] weights = new double[optionsCount][];
        string path = @"F:\DigitDB\PictureSaver\";
        getVector handler;
        Saver.getVectorLength vectorLengthHandler;

        public SimpleLearning(bool load)
        {
            handler = getVector;
            vectorLengthHandler = getVectorLength;
            initialize(127);
            if (load)
                loadDefault(false);
        }

        private void initialize(int defaultWeight)
        {
            for (int n = 0; n < optionsCount; n++)
            {
                weights[n] = new double[vectorLength];
                for (int i = 0; i < vectorLength; i++)
                        weights[n][i] = defaultWeight;
                }
        }

        public int getVectorLength(List<double> parameters)
        {
            return vectorLength;
        }

        private void randomInitialize()
        {
            Random rand = new Random();
            for (int n = 0; n < optionsCount; n++)
            {
                weights[n] = new double[vectorLength];
                for (int i = 0; i < vectorLength; i++)
                    weights[n][i] = rand.Next(255);
            }
        }

        public void saveWeights(string path)
        {
            LearnerData LD = new LearnerData(optionsCount,vectorLength);
            Saver.saveWeights(path, LD, optionsCount, vectorLength);
        }

        public void saveWeights()
        {
            SaveFileDialog sf = new SaveFileDialog();
            if (sf.ShowDialog() == DialogResult.OK)
            {
                saveWeights(sf.FileName);
            }
        }

        public void loadWeights(string path)
        {
            LearnerData LD = Saver.loadWeights(path, 0, optionsCount, vectorLengthHandler);
            weights = LD.weights;
        }

        public void loadWeights()
        {
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            {
                loadWeights(of.FileName);
            }
        }

        public void loadDefault(bool average)
        {
            if (average)
                loadWeights(@"defaultWeights\CenterLearning\4x4average .txt");
            else
                loadWeights(@"defaultWeights\SimpleLearning\defaultWeight127kohonen nonLinearDelta 0,2.txt");
        }

        public Bitmap visualize()
        {
            Bitmap result = new Bitmap(1000, 100);
            loadWeights();
            for (int n = 0; n < optionsCount; n++)
            {
                int counter = 0;
                for (int i = 0; i < 100; i++)
                    for (int j = 0; j < 100; j++)
                    {
                        double pixel = weights[n][counter];
                        if (pixel < 0) pixel = 0;
                        if (pixel > 255) pixel = 255;
                        result.SetPixel(n * 100 + i, j, Color.FromArgb(255, (int)pixel, (int)pixel, (int)pixel));
                        counter++;
                    }
            }
            return result;
        }

        public double[] getVector(Bitmap bmp)
        {
            double[] result = new double[vectorLength];
            int counter = 0;
            for (int i = 0; i < 100; i++)
                for (int j = 0; j < 100; j++)
                {
                    result[counter] = bmp.GetPixel(i, j).R;
                    counter++;
                }
            return result;
        }

        public List<double> guess(Bitmap bmp)
        {
            return LearningProcedures.guess(getVector(bmp), optionsCount, weights);
        }

        public void learnAllKohonen(int learningCount, BackgroundWorker bw, bool linearDelta, double deltaAtTheEnd)
        {
            weights = LearningProcedures.learnAll(learningCount, bw, linearDelta, deltaAtTheEnd, optionsCount, vectorLength, handler);
        }

        public void learnAllAverage(int learningCount, BackgroundWorker bw)
        {
            weights = LearningProcedures.learnAllAverage(learningCount, bw, optionsCount, vectorLength, handler);
        }
        public int[,] guessAll(int guessingCount, BackgroundWorker bw)
        {
            return LearningProcedures.guessAll(weights, guessingCount, bw, optionsCount, vectorLength, handler);
        }

        public void AutoTest(BackgroundWorker bw)
        {
            int i = 127;
            initialize(i);
            string path = @"F:\C#\HandwrittingRecognition\HandwrittingRecognition\bin\Debug\weights\"+this.GetType().Name+@"\auto\" + "defaultWeight" + i;
            AutoTest(bw, path);
            randomInitialize();
            path = @"F:\C#\HandwrittingRecognition\HandwrittingRecognition\bin\Debug\weights\" + this.GetType().Name + @"\auto\" + "randomWeight";
            AutoTest(bw, path);
        }

        public void AutoTest(BackgroundWorker bw, string path)
        {
            string currenPath;
            bool linearDelta = false;
            for (int x = 0; x < 2; x++) //to test with different delta functions
            {
                for (double deltaAtTheEnd = 0.0; deltaAtTheEnd < 0.5; deltaAtTheEnd += 0.2)
                {
                    string deltaFunc;
                    if (linearDelta)
                        deltaFunc = " linearDelta ";
                    else
                        deltaFunc = " nonLinearDelta ";
                    currenPath = path + "kohonen" + deltaFunc + deltaAtTheEnd.ToString();
                    learnAllKohonen(100, bw, linearDelta, deltaAtTheEnd);
                    saveWeights(currenPath + ".txt");
                    LearningProcedures.saveGuess(guessAll(100, bw), currenPath);
                }
                linearDelta = true;
            }
            currenPath = path + "average ";
            learnAllAverage(100, bw);
            saveWeights(currenPath + ".txt");
            LearningProcedures.saveGuess(guessAll(100, bw), currenPath);
        } 
    }
}
