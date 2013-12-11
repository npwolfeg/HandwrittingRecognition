using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HandwrittingRecognition
{
    class LearnerData
    {
        public List<double> parameters;
        public double[][] weights;

        public LearnerData(int param1, int param2, double[][] weights)
        {
            parameters = new List<double>();
            parameters.Add(param1);
            parameters.Add(param2);
            this.weights = weights;
        }

        public LearnerData(List<double> parameters, double[][] weights)
        {
            this.parameters = parameters;
            this.weights = weights;
        }

        public LearnerData(int optionsCount, int vectorLength) //not needed anymore?
        {
            parameters = new List<double>();
            weights = new double[optionsCount][];
            for (int i = 0; i < optionsCount; i++)
                weights[i] = new double[vectorLength];
       }

        public LearnerData(double[][] weights)
        {
            parameters = new List<double>();
            this.weights = weights;
        }

    }

    static class Saver
    {
        public delegate int getVectorLength(List<double> parameters);

        static public void saveWeights(string path, LearnerData LD, int optionsCount, int vectorLength)
        {
            File.Delete(path);
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (int x = 0; x < LD.parameters.Count; x++)
                    sw.WriteLine(LD.parameters[x].ToString());
                for (int n = 0; n < optionsCount; n++)
                    for (int i = 0; i < vectorLength; i++)
                        sw.WriteLine(LD.weights[n][i].ToString());
            }
        }

        static public LearnerData loadWeights(string path, int parametersCount, int optionsCount, getVectorLength GVL)
        {
            List<double> parameters = new List<double>();
            
            using (StreamReader sw = new StreamReader(path))
            {
                for (int x = 0; x < parametersCount; x++)
                    parameters.Add(Convert.ToDouble(sw.ReadLine()));
                int vectorLength = GVL(parameters);
                LearnerData result = new LearnerData(optionsCount, vectorLength);
                result.parameters = parameters;
                for (int n = 0; n < optionsCount; n++)
                    for (int i = 0; i < vectorLength; i++)
                        result.weights[n][i] = Convert.ToDouble(sw.ReadLine());
                    return result;
            }
        }
    }
}
