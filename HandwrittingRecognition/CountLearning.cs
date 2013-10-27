﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace HandwrittingRecognition
{
    class CountLearning : ILearner
    {
        int blockRows = 16;
        int blockCols = 16;
        static int picWidth = 100;
        static int picHeight = 100;
        int blockWidth;
        int blockHeight;
        public int optionsCount = 10;
        public int vectorLength;
        public double[][] weights;
        getVector handler;
        Saver.getVectorLength vectorLengthHandler;

        public CountLearning(bool load)
        {
            handler = getVector;
            vectorLengthHandler = getVectorLength;
            initialize(16,16);
            if (load)
                loadDefault(false);
        }

        private void initialize(int blockCols, int blockRows)
        {
            this.blockCols = blockCols;
            this.blockRows = blockRows;
            weights = new double[optionsCount][];
            vectorLength = blockRows * blockCols;
            blockWidth = picWidth / blockCols;
            blockHeight = picHeight / blockRows;
            for (int n = 0; n < optionsCount; n++)
            {
                weights[n] = new double[vectorLength];
                for (int i = 0; i < vectorLength; i++)
                    weights[n][i] = 0;
            }
        }

        public int getVectorLength(List<double> parameters)
        {
            return (int)(parameters[0] * parameters[1]);
        }

        public void saveWeights(string path)
        {
            LearnerData LD = new LearnerData(blockCols, blockRows, weights);
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
            LearnerData LD = Saver.loadWeights(path, 2, optionsCount, vectorLengthHandler);
            initialize((int)LD.parameters[0], (int)LD.parameters[1]);
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
                loadWeights(@"defaultWeights\CountLearning\4x4average .txt");
            else
                loadWeights(@"defaultWeights\CountLearning\4x4kohonen nonLinearDelta 0,2.txt");
        }

        public void learnAllKohonen(int learningCount, BackgroundWorker bw, bool linearDelta, double deltaAtTheEnd)
        {
            weights = LearningProcedures.learnAll(learningCount, bw, linearDelta, deltaAtTheEnd, optionsCount, vectorLength, handler);
        }

        public void learnAllAverage(int learningCount, BackgroundWorker bw)
        {
            weights = LearningProcedures.learnAllAverage(learningCount, bw, optionsCount, vectorLength, handler);
        }
        public int[,] guessAll(int guessingCount , BackgroundWorker bw)
        {
            return LearningProcedures.guessAll(weights, guessingCount, bw, optionsCount, vectorLength, handler);
        }

        public void AutoTest(BackgroundWorker bw)
        {
            for (int i = 4; i <= 4; i*=2)
            {
                initialize(i, i);
                string dir = @"F:\C#\HandwrittingRecognition\HandwrittingRecognition\bin\Debug\weights\" + this.GetType().Name + @"\MNIST\";
                Directory.CreateDirectory(dir);
                string path = dir + i + "x" + i;
                AutoTest(bw, path);
            }
        }

        public void AutoTest(BackgroundWorker bw, string path)
        {
            string currenPath;
            bool linearDelta = false;
            for (int x = 0; x < 1; x++) //to test with different delta functions
            {
                for (double deltaAtTheEnd = 0.2; deltaAtTheEnd < 0.3; deltaAtTheEnd += 0.2)
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

        public Bitmap visualize()
        {            
            Bitmap result = new Bitmap(1000, 100);
            loadWeights();
            for (int n = 0; n < optionsCount; n++)
                using (Graphics g = Graphics.FromImage(result))
                {
                    int counter = 0;
                    for (int i = 0; i < blockCols; i++)
                        for (int j = 0; j < blockRows; j++)
                        {
                            g.DrawRectangle(new Pen(Color.Blue, 4), n * picWidth + i * blockWidth, j * blockHeight, blockWidth, blockHeight);
                            g.DrawRectangle(new Pen(Color.Orange, 4), new Rectangle(n * picWidth + i * blockWidth + (int)weights[n][counter], j * blockHeight + (int)weights[n][counter+1], 1, 1));
                            g.DrawLine(new Pen(Color.Black, 1), n * picWidth + i * blockWidth + blockWidth / 2, j * blockHeight, n * picWidth + i * blockWidth + blockWidth / 2, (j + 1) * blockHeight);
                            g.DrawLine(new Pen(Color.Black, 1), n * picWidth + i * blockWidth, j * blockHeight + blockHeight/2, n * picWidth + (i+1) * blockWidth, j * blockHeight + blockHeight/2);
                            counter++;
                        }
                }
            return result;
        }

        double[] CountPixels(Bitmap bmp)
        {
            double[] result = new double[1];
            for (int i=0;i<bmp.Width;i++)
                for(int j=0;j<bmp.Height;j++)
                    if (bmp.GetPixel(i, j).R < 255)
                        result[0] += i;           
            return result;
        }

        public double[] getVector(Bitmap bmp)
        {
            double[] result = new double[vectorLength];
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
            return result;
        }

        public List<double> guess(Bitmap bmp)
        {
            return LearningProcedures.guess(getVector(bmp), optionsCount, weights);
        } 
    }
}
