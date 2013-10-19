using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace HandwrittingRecognition
{
    class LBPLearning
    {
        public int optionsCount = 10;
        public int vectorLength;
        int histogramLength;
        //int pointsCount = 8; //vectorLenghth depends on it!!! (initialize method 256 = 2^8)
        //int radius = 2;
        int blockRows = 4;
        int blockCols = 4;
        static int picWidth = 100;
        static int picHeight = 100;
        int blockWidth;
        int blockHeight;
        public double[][] weights;
        LearningProcedures.getVector handler;
        Saver.getVectorLength vectorLengthHandler;

        public LBPLearning()
        {
            handler = getVector;
            vectorLengthHandler = getVectorLength;
            initialize(2, 2);
        }

        private void initialize(int blockCols, int blockRows)
        {
            this.blockCols = blockCols;
            this.blockRows = blockRows;
            weights = new double[optionsCount][];
            histogramLength = (int)Math.Pow(2, 8);
            vectorLength = histogramLength * blockRows * blockCols;

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
            return (int) (histogramLength * parameters[0] * parameters[1]);
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

        public Bitmap visualize() //stub
        {
            Bitmap result = new Bitmap(1000, 100);
            return result;
        }

        static public int analyzePixel(Bitmap bmp, int x, int y)
        {
            int sum = 0;
            if (x > 0 && x < bmp.Width-1 && y > 0 && y < bmp.Height-1)
            {
                int power = 0;
                for (int i = -1; i < 2; i++)
                    for (int j = -1; j < 2; j++)
                        if (i != 0 || j != 0)
                        {
                            if (bmp.GetPixel(x + i, y + j).R >= bmp.GetPixel(x, y).R)
                                sum += (int)Math.Pow(2, power);
                            power++;
                        }
            }
            return sum;
        }

        public double[] getHistogram(Bitmap bmp)
        {
            double[] result = new double[histogramLength];
            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                    result[analyzePixel(bmp, i, j)]++;
            result = Vector.normalyzeVektor(result,1);
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
                    double[] currentHistogram = getHistogram(partOfBmp);
                    for (int k = 0; k < histogramLength; k++)
                    {
                        result[counter] = currentHistogram[k];
                        counter++;
                    }
                }
            return result;
        }

        public List<double> guess(Bitmap bmp)
        {
            return LearningProcedures.guess(getVector(bmp), optionsCount, weights);
        }

        public void learnAllKohonen(int learningCount, BackgroundWorker bw, bool linearDelta, double deltaAtTheEnd)
        {
            LearningProcedures l = new LearningProcedures();
            weights = l.learnAll(learningCount, bw, linearDelta, deltaAtTheEnd, optionsCount, vectorLength, handler);
        }

        public void learnAllAverage(int learningCount, BackgroundWorker bw)
        {
            LearningProcedures l = new LearningProcedures();
            weights = l.learnAllAverage(learningCount, bw, optionsCount, vectorLength, handler);
        }
        public int[,] guessAll(int guessingCount, BackgroundWorker bw)
        {
            LearningProcedures l = new LearningProcedures();
            return l.guessAll(weights, guessingCount, bw, optionsCount, vectorLength, handler);
        }

        public void AutoTest(BackgroundWorker bw)
        {
            for (int i = 4; i <= 8; i += 2)
            {
                initialize(i, i);
                string path = @"F:\C#\HandwrittingRecognition\HandwrittingRecognition\bin\Debug\weights\" + this.GetType().Name + @"\auto\" + i + "x" + i;
                AutoTest(bw, path);
            }
        }

        public void AutoTest(BackgroundWorker bw, string path)
        {
            string currenPath;
            bool linearDelta = false;
            for (int x = 0; x < 2; x++) //to test with different delta functions
            {
                for (double deltaAtTheEnd = 0.0; deltaAtTheEnd < 0.3; deltaAtTheEnd += 0.2)
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
