using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.IO;

namespace HandwrittingRecognition
{
    abstract public class Learner
    {
        public static int optionsCount = 42;
        public int vectorLength;
        public double[][] weights = new double[optionsCount][];
        static string pathDigitsAndLetters = @"F:\DigitDB\DigitsAndLetters\";

        abstract public void saveWeights(string path);

        public void saveWeights()
        {
            SaveFileDialog sf = new SaveFileDialog();
            if (sf.ShowDialog() == DialogResult.OK)
            {
                saveWeights(sf.FileName);
            }
        }

        abstract public void loadWeights(string path);

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
            string path = @"weights - all\" + this.GetType().Name + @".txt";
            if (average)
                path = path.Insert(path.Length - 4, " average");
            loadWeights(path);
        }

        abstract public Bitmap visualize();
        abstract public double[] getVector(Bitmap bmp);

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

        public List<double> guess(Bitmap bmp)
        {
            return guess(getVector(bmp));
        }

        public List<double> guess(double[] vector)
        {
            List<double> dist = new List<double>();
            for (int n = 0; n < optionsCount; n++)
                dist.Add(Distances.Distance(vector, weights[n]));
            dist = Vector.normalyzeVektor(dist, 100);
            return dist;
        }

        public double[][] learnKohonen(double[] vector, int n, double delta)
        {
            List<double> arr = guess(vector);
            int id = arr.IndexOf(arr.Min());
            if (n != id)
                for (int i = 0; i < vector.Length; i++)
                {
                    weights[n][i] += delta * (vector[i] - weights[n][i]);
                    weights[id][i] += delta * (weights[n][i] - vector[i]);
                }
            else
                for (int i = 0; i < vector.Length; i++)
                    weights[n][i] += delta * (vector[i] - weights[n][i]);
            return weights;
        }

        public void learnAll(int learningCount, BackgroundWorker bw, bool linearDelta, double deltaAtTheEnd)
        {
            double delta = 1;
            //double[][] weights;
            weights = new double[optionsCount][];
            for (int n = 0; n < optionsCount; n++)
            {
                weights[n] = new double[vectorLength];
                for (int i = 0; i < vectorLength; i++)
                    weights[n][i] = 0;
            }
            int progress, maxProgress;
            Bitmap bmp;
            string[] picturePaths = Directory.GetFiles(pathDigitsAndLetters);
            Random rand = new Random();

            progress = 0;
            maxProgress = learningCount;
            for (int n = 0; n < learningCount; n++)
            {
                string picturePath = picturePaths[rand.Next(picturePaths.Length)];
                progress++;
                bw.ReportProgress((int)((float)progress / maxProgress * 100));
                bmp = new Bitmap(picturePath);
                bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);
                learnKohonen(getVector(bmp), getCorrectAnswer(picturePath), delta);

                if (deltaAtTheEnd >= 1)
                    deltaAtTheEnd = 0.99;
                if (linearDelta)
                {
                    delta = -(double)progress / ((1 / (1 - deltaAtTheEnd)) * maxProgress) + 1;
                }
                else
                {
                    if (deltaAtTheEnd <= 0)
                        deltaAtTheEnd = 0.01;
                    double a = deltaAtTheEnd / (1 - deltaAtTheEnd);
                    delta = a * learningCount / ((double)progress + a * learningCount);
                }
            }
            //return weights;
        }

        public void learnAllAverage(int learningCount, BackgroundWorker bw)
        {
            //double[][] weights = new double[optionsCount][];
            weights = new double[optionsCount][];
            for (int n = 0; n < optionsCount; n++)
            {
                weights[n] = new double[vectorLength];
                for (int i = 0; i < vectorLength; i++)
                    weights[n][i] = 0;
            }

            int progress, maxProgress;
            double[] vector1 = new double[vectorLength];
            int[] count = new int[optionsCount];
            Bitmap bmp;

            string[] picturePaths = Directory.GetFiles(pathDigitsAndLetters);
            Random rand = new Random();

            progress = 0;
            maxProgress = learningCount;
            for (int n = 0; n < learningCount; n++)
            {
                string picturePath = picturePaths[rand.Next(picturePaths.Length)];
                int k = getCorrectAnswer(picturePath);
                progress++;
                bw.ReportProgress((int)((float)progress / maxProgress * 100));
                bmp = new Bitmap(100, 100);
                bmp = new Bitmap(picturePath);
                bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);
                vector1 = getVector(bmp);
                for (int i = 0; i < vectorLength; i++)
                    weights[k][i] += vector1[i];
                count[k]++;
            }
            for (int k = 0; k < optionsCount; k++)
                for (int i = 0; i < vectorLength; i++)
                    weights[k][i] = weights[k][i] / count[k];
            //return weights;
        }

        public int[,] guessAll(BackgroundWorker bw)
        {
            int progress, maxProgress;
            progress = 0;
            int[] count = new int[optionsCount];
            Bitmap bmp;
            List<double> arr;
            int ID;
            int[,] result = new int[optionsCount, 2];
            string[] picturePaths = Directory.GetFiles(pathDigitsAndLetters);
            maxProgress = picturePaths.Length;
            //foreach (string picturePath in picturePaths)
            for (int j = 0; j < picturePaths.Length; j++)
            {
                string picturePath = picturePaths[j];
                progress++;
                bw.ReportProgress((int)((float)progress / maxProgress * 100));
                bmp = new Bitmap(picturePath);
                int correctAnswer = getCorrectAnswer(picturePath);
                bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);
                arr = guess(getVector(bmp));
                ID = arr.IndexOf(arr.Min());
                if (ID == correctAnswer)
                    result[correctAnswer, 0]++;
                else
                    result[ID, 1]++;
            }
            return result;
        }

        abstract public void RunAutoTest(BackgroundWorker bw);

        public void AutoTest(BackgroundWorker bw)
        {
            string dir = @"weights - all\";
            Directory.CreateDirectory(dir);
            string path = dir + this.GetType().Name + @".txt";

            bool linearDelta = false;
            for (int x = 0; x < 1; x++) //to test with different delta functions
            {
                for (double deltaAtTheEnd = 0.2; deltaAtTheEnd < 0.3; deltaAtTheEnd += 0.2)
                {
                    learnAll(1000, bw,linearDelta,deltaAtTheEnd);
                    saveWeights(path);
                    loadWeights(path);
                    saveGuessNew(guessAll(bw), path);
                }
                //linearDelta = true;
            }
            //currenPath = path + "average ";
            //learnAllAverage(10000, bw);
            path = path.Insert(path.Length - 4, " average");
            saveWeights(path);
            loadWeights(path);
            //saveGuessNew(guessAll(bw), path);
        }

        static public void saveGuessNew(int[,] rightNwrong, string currenPath)
        {
            List<string> possibleOptions = getPossibleOptions();

            int rightSum = 0;
            int wrongSum = 0;
            currenPath += " result = ";
            using (StreamWriter sw = new StreamWriter(currenPath + ".txt"))
            {
                for (int j = 0; j < 42; j++)
                {
                    int ID = 0;
                    int max = 0;
                    for (int i = 0; i < rightNwrong.GetLength(0); i++)
                        if (rightNwrong[i, 0] > max)
                        {
                            max = rightNwrong[i, 0];
                            ID = i;
                        }
                    sw.WriteLine(possibleOptions[ID] + " " + rightNwrong[ID, 0].ToString() + " " + rightNwrong[ID, 1].ToString());
                    rightSum += rightNwrong[ID, 0];
                    wrongSum += rightNwrong[ID, 1];
                    rightNwrong[ID, 0] = 0;
                }
                sw.WriteLine(rightSum);
            }
            File.Delete(currenPath + rightSum + ".txt");
            File.Move(currenPath + ".txt", currenPath + rightSum + " of " + (rightSum + wrongSum) + ".txt");
        }
    }
}
