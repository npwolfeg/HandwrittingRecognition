using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Drawing;

namespace LinearBinaryPattern
{
    class LearningProcedures
    {
        string path = @"F:\DigitDB\PictureSaver\";
        public delegate double[] getVector(Bitmap bmp);     

        public LearningProcedures()
        {
        }

        static public List<double> guess(double[] vector, int optionsCount, double[][]weights)
        {
            List<double> dist = new List<double>();
            for (int n = 0; n < optionsCount; n++)
                dist.Add(Distances.EuclidDistance(vector, weights[n]));
            dist = Vector.normalyzeVektor(dist);
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

        public double[][] learnAll(int learningCount, BackgroundWorker bw, bool linearDelta, double deltaAtTheEnd, int optionsCount, int vectorLength,getVector gv)
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
            int[] count = new int[optionsCount];
            Bitmap bmp;
            using (StreamReader sr = new StreamReader(path + "count.txt"))
            {
                for (int i = 0; i < optionsCount; i++)
                    count[i] = Convert.ToInt32(sr.ReadLine());
            }
            progress = 0;
            maxProgress = learningCount * optionsCount;
            for (int n = 0; n < learningCount; n++)
            {
                for (int k = 0; k < optionsCount; k++)
                {
                    progress++;
                    bw.ReportProgress((int)((float)progress / maxProgress * 100));
                    bmp = new Bitmap(path + k.ToString() + n.ToString() + ".bmp");
                    bmp = BmpProcesser.FromAlphaToRGB(bmp);
                    bmp = BmpProcesser.normalizeBitmapRChannel(bmp, 100, 100);
                    learnKohonen(gv(bmp), k, weights, optionsCount,delta);
                }
                if (deltaAtTheEnd >= 1)
                    deltaAtTheEnd = 0.99;
                if (linearDelta)
                {
                    delta = -(double)progress / ((1/(1-deltaAtTheEnd)) * maxProgress) + 1;
                }
                else
                {
                    if (deltaAtTheEnd <=0)
                        deltaAtTheEnd = 0.01;
                    double a = deltaAtTheEnd / (1 - deltaAtTheEnd);
                    delta = a * learningCount / ((double)progress + a * learningCount);
                }
            }
            return weights;
        }

        public double[][] learnAllAverage(int learningCount, BackgroundWorker bw, int optionsCount, int vectorLength, getVector gv)
        {
            double[][] weights;
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
            using (StreamReader sr = new StreamReader(path + "count.txt"))
            {
                for (int i = 0; i < optionsCount; i++)
                    count[i] = Convert.ToInt32(sr.ReadLine());
            }
            progress = 0;
            maxProgress = learningCount * optionsCount;
            for (int n = 0; n < learningCount; n++)
            {
                for (int k = 0; k < optionsCount; k++)
                {
                    progress++;
                    bw.ReportProgress((int)((float)progress / maxProgress * 100));
                    bmp = new Bitmap(path + k.ToString() + n.ToString() + ".bmp");
                    bmp = BmpProcesser.FromAlphaToRGB(bmp);
                    bmp = BmpProcesser.normalizeBitmapRChannel(bmp, 100, 100);
                    vector1 = gv(bmp);
                    for (int i = 0; i < vectorLength; i++)
                        weights[k][i] += vector1[i];
                }
            }
            for (int k = 0; k < optionsCount; k++)
                for (int i = 0; i < vectorLength; i++)
                    weights[k][i] = weights[k][i] / learningCount;
            return weights;
        }

        public int[,] guessAll(double[][]weights, int guessingCount, BackgroundWorker bw, int optionsCount, int vectorLength, getVector gv)
        {
            int progress, maxProgress;            
            progress = 0;
            maxProgress = guessingCount * optionsCount;
            int[] count = new int[optionsCount];
            Bitmap bmp;
            List<double> arr;
            int ID;
            int[,] result = new int[optionsCount, 2];
            using (StreamReader sr = new StreamReader(path + "count.txt"))
            {
                for (int i = 0; i < 10; i++)
                    count[i] = Convert.ToInt32(sr.ReadLine());
            }
            for (int n = 0; n < guessingCount; n++)
            {
                for (int k = 0; k < optionsCount; k++)
                {
                    progress++;
                    bw.ReportProgress((int)((float)progress / maxProgress * 100));
                    bmp = new Bitmap(path + k.ToString() + n.ToString() + ".bmp");
                    bmp = BmpProcesser.FromAlphaToRGB(bmp);
                    bmp = BmpProcesser.normalizeBitmapRChannel(bmp, 100, 100);
                    arr = guess(gv(bmp),optionsCount,weights);
                    ID = arr.IndexOf(arr.Min());
                    if (ID == k)
                        result[k, 0]++;
                    else
                        result[ID, 1]++;
                }
            }
            return result;
        }

        static public void saveGuess(int[,] rightNwrong, string currenPath)
        {
            int sum = 0;
            currenPath += " resut = ";
            using (StreamWriter sw = new StreamWriter(currenPath + ".txt"))
            {
                for (int i = 0; i < 10; i++)
                {
                    sw.WriteLine(i.ToString() + " " + rightNwrong[i, 0].ToString() + " " + rightNwrong[i, 1].ToString());
                    sum += rightNwrong[i, 0];
                }
                sw.WriteLine(sum);
            }
            File.Delete(currenPath + sum + ".txt");
            File.Move(currenPath + ".txt", currenPath + sum + ".txt");
        }
    }
}
