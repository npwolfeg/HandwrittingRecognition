using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Drawing;

namespace HandwrittingRecognition
{
    public delegate double[] getVector(Bitmap bmp); 
    
    static class LearningProcedures //not needed anymore
    {
        static string path = @"F:\DigitDB\PictureSaverAll\";
        static string pathDigitsAndLetters = @"F:\DigitDB\DigitsAndLetters\";
        //static string path = @"F:\DigitDB\PictureSaverThin\";
        static int counterStart = 1072;
        static int optionsCount = 42;

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

        static public List<double> guess(double[] vector, int optionsCount, double[][]weights)
        {
            List<double> dist = new List<double>();
            for (int n = 0; n < optionsCount; n++) 
                dist.Add(Distances.Distance(vector, weights[n]));
            dist = Vector.normalyzeVektor(dist,100);
            return dist;
        }

        static public double[][] learnKohonen(double[] vector, int n, double[][] weights, int optionsCount, double delta)
        {
            List<double> arr = guess(vector,optionsCount,weights);  
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

        public static double[][] learnAll(int learningCount, BackgroundWorker bw, bool linearDelta, double deltaAtTheEnd, int optionsCount, int vectorLength,getVector gv)
        {
            double delta = 1;
            double[][] weights;
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
                learnKohonen(gv(bmp), getCorrectAnswer(picturePath), weights, optionsCount, delta);

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
            return weights;
        }

        public static double[][] learnAllAverage(int learningCount, BackgroundWorker bw, int optionsCount, int vectorLength, getVector gv)
        {
            double[][] weights = new double[optionsCount][];
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
                vector1 = gv(bmp);
                for (int i = 0; i < vectorLength; i++)
                    weights[k][i] += vector1[i];
                count[k]++;
            }
            for (int k = 0; k < optionsCount; k++)
                for (int i = 0; i < vectorLength; i++)
                    weights[k][i] = weights[k][i] / count[k];
            return weights;
        }

        public static int[,] guessAll(double[][]weights, BackgroundWorker bw, int optionsCount, int vectorLength, getVector gv)
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
            for(int j=0;j<picturePaths.Length;j++)
                {
                    string picturePath = picturePaths[j];
                    progress++;
                    bw.ReportProgress((int)((float)progress / maxProgress * 100));
                    bmp = new Bitmap(picturePath);
                    int correctAnswer = getCorrectAnswer(picturePath);
                    bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);
                    arr = guess(gv(bmp), optionsCount, weights);
                    ID = arr.IndexOf(arr.Min());
                    if (ID == correctAnswer)
                        result[correctAnswer, 0]++;
                    else
                        result[ID, 1]++;
                }
            return result;
        }

        static public void saveGuess(int[,] rightNwrong, string currenPath)
        {
            int sum = 0;
            currenPath += " resut = ";
            using (StreamWriter sw = new StreamWriter(currenPath + ".txt"))
            {
                for (int i = 0; i < /*10*/ rightNwrong.GetLength(0) ; i++)
                {
                    sw.WriteLine(i.ToString() + " " + rightNwrong[i, 0].ToString() + " " + rightNwrong[i, 1].ToString());
                    sum += rightNwrong[i, 0];
                }
                sw.WriteLine(sum);
            }
            File.Delete(currenPath + sum + ".txt");
            File.Move(currenPath + ".txt", currenPath + sum + ".txt");
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
            File.Move(currenPath + ".txt", currenPath + rightSum + " of " + (rightSum+wrongSum) + ".txt");
        }
    }
}
